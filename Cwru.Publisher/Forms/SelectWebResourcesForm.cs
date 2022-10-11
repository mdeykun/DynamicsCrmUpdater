using Cwru.Common.Config;
using Cwru.Common.Extensions;
using Cwru.Common.Model;
using Cwru.Common.Services;
using Cwru.CrmRequests.Common.Contracts;
using Cwru.Publisher.Extensions;
using Cwru.Publisher.Forms.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cwru.Publisher.Forms
{
    //TODO: Test cases preparation

    public partial class SelectWebResourcesForm : Form
    {
        private readonly ProjectConfig projectConfig;
        private readonly ICrmRequests crmRequests;
        private readonly SolutionsService solutionsService;
        private readonly CustomProgressBar customProgressBar;
        private readonly WebResourceTypesService webResourceTypesService;
        private const string searchPlaceHolderText = "Search...";

        private List<WebResource> allWebResources;
        private string lastSerchString;

        public Guid? SelectedEnvironmentId { get; set; }
        public Guid? SelectedSolutionId { get; set; }
        public string SelectedSolutionName { get; set; }
        public List<WebResource> SelectedWebResources { get; set; }

        public SelectWebResourcesForm(
            ProjectConfig projectConfig,
            ICrmRequests crmRequests,
            SolutionsService solutionsService,
            WebResourceTypesService webResourceTypesService)
        {
            this.SelectedWebResources = new List<WebResource>();

            this.projectConfig = projectConfig;
            this.crmRequests = crmRequests;
            this.solutionsService = solutionsService;
            this.webResourceTypesService = webResourceTypesService;

            InitializeComponent();

            this.customProgressBar = new CustomProgressBar(progressBar);
        }

        private async void selectWebResourcesForm_Load(object sender, EventArgs e)
        {
            try
            {
                LockForm(true);
                customProgressBar.Show();

                var environment = projectConfig.GetDefaultEnvironment();
                SelectedEnvironmentId = environment.Id;
                SelectedSolutionId = environment.SelectedSolutionId;
                SelectedWebResources = new List<WebResource>();
                allWebResources = new List<WebResource>();

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

                var webResourcesTypes = webResourceTypesService.GetTypesLabels();
                wrTypesComboBox.Items.Add("All Types");
                wrTypesComboBox.Items.AddRange(webResourcesTypes.Values.ToArray());
                wrTypesComboBox.SelectedIndexChanged += wrTypesComboBox_SelectedIndexChanged;

                wrSearchTextBox.ForeColor = Color.Gray;
                wrSearchTextBox.Text = searchPlaceHolderText;
                wrSearchTextBox.GotFocus += wrSearchTextBox_GotFocus;
                wrSearchTextBox.LostFocus += wrSearchTextBox_LostFocus;

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
            var selectedItems = allWrList.SelectedItems.ToArray<WebResource>();
            if (selectedItems.Length == 0)
            {
                return;
            }

            selectedWrList.Items.AddRange(selectedItems);

            SelectedWebResources.Clear();
            SelectedWebResources.AddRange(selectedWrList.Items.ToArray<WebResource>());

            FilterAllItems(true);
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            var selectedItems = selectedWrList.SelectedItems.ToArray<WebResource>();
            if (selectedItems.Length == 0)
            {
                return;
            }

            selectedWrList.Items.RemoveRange(selectedItems);

            SelectedWebResources.Clear();
            SelectedWebResources.AddRange(selectedWrList.Items.ToArray<WebResource>());

            FilterAllItems(true);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void wrSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            FilterAllItems(false);
        }

        private void wrTypesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterAllItems(true);
        }

        private void wrSearchTextBox_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(wrSearchTextBox.Text))
            {
                wrSearchTextBox.Text = searchPlaceHolderText;
                wrSearchTextBox.ForeColor = Color.Gray;
            }
        }

        private void wrSearchTextBox_GotFocus(object sender, EventArgs e)
        {
            if (wrSearchTextBox.Text.IsEqualToLower(searchPlaceHolderText))
            {
                wrSearchTextBox.ForeColor = Color.Black;
                wrSearchTextBox.Text = string.Empty;
            }
        }

        private void FilterAllItems(bool force)
        {
            var itemsToShow = allWebResources.AsEnumerable();

            var searchText = wrSearchTextBox.Text?.Trim();
            if (searchText == null || searchText.IsEqualToLower(searchPlaceHolderText))
            {
                searchText = string.Empty;
            }

            if (lastSerchString.IsEqualToLower(searchText) && !force)
            {
                return;
            }

            lastSerchString = searchText;
            var startWithStar = searchText.StartsWith("*");
            searchText = startWithStar ? searchText.Substring(1) : searchText;

            if (wrTypesComboBox.SelectedIndex != 0)
            {
                var wrTypeLabelToShow = (string)wrTypesComboBox.SelectedItem;
                var wrTypeToShow = webResourceTypesService.GetTypeByLabel(wrTypeLabelToShow);
                if (wrTypeToShow != null)
                {
                    itemsToShow = itemsToShow.Where(x => x.Type == wrTypeToShow);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (startWithStar)
                {
                    itemsToShow = itemsToShow.
                        Where(x => x.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                }
                else
                {
                    itemsToShow = itemsToShow.
                        Where(x => x.Name.StartWithLower(searchText));
                }
            }

            var selectedItems = selectedWrList.SelectedItems.ToArray<WebResource>();
            if (selectedItems.Length > 0)
            {
                itemsToShow = itemsToShow.Where(x => !selectedItems.Contains(x));
            }

            allWrList.Items.Clear();
            allWrList.Items.AddRange(itemsToShow.ToArray());
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

            allWebResources.Clear();
            allWebResources.AddRange(webResources);

            ShowStatus(webResources.Length > 0 ?
                $"{webResources.Length} web resource{webResources.Length.Select(" was", "s were")} loaded" :
                $"Solution dosn't have any web resource", Color.Green);
        }

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
            wrSearchTextBox.Enabled = !lockForm;
            wrTypesComboBox.Enabled = !lockForm;
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
