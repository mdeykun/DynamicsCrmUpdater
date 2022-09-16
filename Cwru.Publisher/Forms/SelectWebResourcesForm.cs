using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Extensions;
using Cwru.Common.Model;
using Cwru.Common.Services;
using Cwru.CrmRequests.Common.Contracts;
using Cwru.Publisher.Forms.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cwru.Publisher.Forms
{
    public partial class SelectWebResourcesForm : Form
    {
        private readonly Logger logger;
        private readonly ProjectConfig projectConfig;
        private readonly ICrmRequests crmRequests;
        private readonly SolutionsService solutionsService;
        private readonly CustomProgressBar customProgressBar;

        public Guid? SelectedEnvironmentId { get; set; }
        public Guid? SelectedSolutionId { get; set; }
        public string SelectedSolutionName { get; set; }
        public List<string> SelectedWebResources { get; set; }

        public SelectWebResourcesForm(
            Logger logger,
            ProjectConfig projectConfig,
            ICrmRequests crmRequests,
            SolutionsService solutionsService)
        {
            this.logger = logger;
            this.projectConfig = projectConfig;
            this.crmRequests = crmRequests;
            this.solutionsService = solutionsService;

            InitializeComponent();

            this.customProgressBar = new CustomProgressBar(progressBar);
        }

        private async void SelectWebResourcesForm_Load(object sender, EventArgs e)
        {
            try
            {
                LockForm(true);
                customProgressBar.Show();

                var environment = projectConfig.GetDefaultEnvironment();
                SelectedEnvironmentId = environment.Id;
                SelectedSolutionId = environment.SelectedSolutionId;
                SelectedWebResources = new List<string>();

                environvmentsComboBox.DisplayMember = nameof(EnvironmentConfig.Name);
                solutionsComboBox.DisplayMember = nameof(SolutionDetail.FriendlyName);
                allWrList.DisplayMember = nameof(WebResource.Name);
                selectedWrList.DisplayMember = nameof(WebResource.Name);

                var environments = projectConfig.Environments.ToArray();
                environvmentsComboBox.Items.AddRange(environments);

                var selectedEnvironment = environments.FirstOrDefault(x => x.Id == SelectedEnvironmentId);
                if (selectedEnvironment != null)
                {
                    var index = Array.IndexOf(environments, selectedEnvironment);
                    environvmentsComboBox.SelectedIndex = index;
                }

                await LoadSolutionsAsync();
                await LoadWebResourcesAsync();

                environvmentsComboBox.SelectedIndexChanged += environvmentsComboBox_SelectedIndexChanged;
                solutionsComboBox.SelectedIndexChanged += solutionsComboBox_SelectedIndexChanged;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
            finally
            {
                LockForm(false);
                customProgressBar.Hide();
            }
        }

        private async void environvmentsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LockForm(true);
                customProgressBar.Show();
                var selectedEnvironament = (EnvironmentConfig)environvmentsComboBox.SelectedItem;
                SelectedEnvironmentId = selectedEnvironament?.Id;

                await LoadSolutionsAsync();
                await LoadWebResourcesAsync();
            }
            finally
            {
                LockForm(false);
                customProgressBar.Hide();
            }
        }

        private async void solutionsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LockForm(true);
                customProgressBar.Show();

                var selectedSolution = (SolutionDetail)solutionsComboBox.SelectedItem;

                SelectedSolutionId = selectedSolution?.SolutionId;
                SelectedSolutionName = selectedSolution?.UniqueName;

                await LoadWebResourcesAsync();
            }
            finally
            {
                LockForm(false);
                customProgressBar.Hide();
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var selectedItems = new WebResource[allWrList.SelectedItems.Count];
            allWrList.SelectedItems.CopyTo(selectedItems, 0);

            selectedWrList.Items.AddRange(selectedItems);
            foreach (var item in selectedItems)
            {
                allWrList.Items.Remove(item);
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            var selectedItems = new WebResource[selectedWrList.SelectedItems.Count];
            selectedWrList.SelectedItems.CopyTo(selectedItems, 0);

            allWrList.Items.AddRange(selectedItems);
            foreach (var item in selectedItems)
            {
                selectedWrList.Items.Remove(item);
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {

        }

        private void cancelButton_Click(object sender, EventArgs e)
        {

        }

        private async Task LoadWebResourcesAsync()
        {
            ShowStatus("Loading web resources...");
            selectedWrList.Items.Clear();
            allWrList.Items.Clear();

            var selectedEnvironment = GetSelectedEnvironment();
            if (selectedEnvironment == null)
            {
                return;
            }

            if (SelectedSolutionId == null || SelectedSolutionId == Guid.Empty)
            {
                ShowStatus("Error: Solutions is not selected or was not found", Color.Red);
                return;
            }

            var webResourcesResponse = await crmRequests.RetrieveAllSolutionWebResourcesAsync(selectedEnvironment.ConnectionString.BuildConnectionString(), SelectedSolutionId.Value);
            webResourcesResponse.EnsureSuccess();

            var webResources = webResourcesResponse.Payload.OrderBy(x => x.Name).ToArray();

            allWrList.Items.Clear();
            allWrList.Items.AddRange(webResources);

            ShowStatus(webResources.Length > 0 ?
                $"{webResources.Length} web resource{webResources.Length.Select(" was", "s were")} loaded" :
                $"Solution dosn't have any web resource", Color.Green);
        }

        //TODO: WR names are case sensetive now

        private async Task LoadSolutionsAsync()
        {
            ShowStatus("Loading solutions...");

            var selectedEnvironment = GetSelectedEnvironment();
            if (selectedEnvironment == null)
            {
                return;
            }

            var solutionsResponse = await solutionsService.GetSolutionsDetailsAsync(selectedEnvironment.ConnectionString.BuildConnectionString(), selectedEnvironment.Id, true);
            var solutions = solutionsResponse.ToArray();
            if (solutions.Length == 0)
            {
                ShowStatus("Failed to load solutions", Color.Red);
                return;
            }

            solutionsComboBox.Items.Clear();
            solutionsComboBox.Items.AddRange(solutions.ToArray());

            var selectedSolution = solutions[0];
            if (SelectedSolutionId != null && SelectedEnvironmentId != Guid.Empty)
            {
                var solution = solutions.FirstOrDefault(x => x.SolutionId == SelectedSolutionId) ??
                    solutions.FirstOrDefault(x => x.UniqueName == SelectedSolutionName);

                if (solution != null)
                {
                    selectedSolution = solution;
                }
            }

            var index = Array.IndexOf(solutions, selectedSolution);
            solutionsComboBox.SelectedIndexChanged -= solutionsComboBox_SelectedIndexChanged;
            solutionsComboBox.SelectedIndex = index;
            solutionsComboBox.SelectedIndexChanged += solutionsComboBox_SelectedIndexChanged;
            SelectedSolutionId = selectedSolution.SolutionId;
            SelectedSolutionName = selectedSolution.UniqueName;

            ShowStatus("Solutions where loaded", Color.Green);
        }

        private void LockForm(bool lockForm)
        {
            environvmentsComboBox.Enabled = !lockForm;
            solutionsComboBox.Enabled = !lockForm;
            allWrList.Enabled = !lockForm;
            selectedWrList.Enabled = !lockForm;
            addButton.Enabled = !lockForm;
            removeButton.Enabled = !lockForm;
        }

        private void ShowStatus(string status)
        {
            ShowStatus(status, Color.Black);
        }

        private void ShowStatus(string status, Color color)
        {
            statusLabel.ForeColor = color;
            statusLabel.Text = status;
        }

        private EnvironmentConfig GetSelectedEnvironment()
        {
            var environments = projectConfig.Environments.ToArray();
            if (SelectedEnvironmentId == null || SelectedEnvironmentId == Guid.Empty)
            {
                ShowStatus("Error: Environment is not selected or environment config was not found", Color.Red);
                return null;
            }

            var selectedEnvironment = environments.FirstOrDefault(x => x.Id == SelectedEnvironmentId);
            if (selectedEnvironment == null)
            {
                ShowStatus("Error: Environment is not selected or environment config was not found", Color.Red);
                return null;
            }

            return selectedEnvironment;
        }
    }
}
