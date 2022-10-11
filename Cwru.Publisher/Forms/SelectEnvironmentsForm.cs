using Cwru.Common.Config;
using Cwru.Publisher.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Cwru.Publisher.Forms
{
    public partial class SelectEnvironmentsForm : Form
    {
        private readonly ProjectConfig projectConfig;
        public List<EnvironmentConfig> SelectedEnvironments { get; set; } = new List<EnvironmentConfig>();

        public SelectEnvironmentsForm(ProjectConfig projectConfig)
        {
            InitializeComponent();
            this.projectConfig = projectConfig;
        }

        private void selectEnvironmentsForm_Load(object sender, System.EventArgs e)
        {
            allEnvironmentsListBox.DisplayMember = nameof(EnvironmentConfig.Name);

            var environments = projectConfig.Environments.AsEnumerable();
            var selectedEnvironments = Enumerable.Empty<EnvironmentConfig>();

            if (projectConfig.SelectedEnvironments != null)
            {
                environments = projectConfig.Environments.Where(x => !projectConfig.SelectedEnvironments.Contains(x.Id));
                selectedEnvironments = projectConfig.Environments.Where(x => projectConfig.SelectedEnvironments.Contains(x.Id));
            }

            allEnvironmentsListBox.Items.AddRange(environments.ToArray());
            selectedEnvironmentsListBox.Items.AddRange(selectedEnvironments.ToArray());
        }

        private void addButton_Click(object sender, System.EventArgs e)
        {
            var selectedItems = allEnvironmentsListBox.SelectedItems.ToArray<EnvironmentConfig>();
            if (selectedItems.Length == 0)
            {
                return;
            }

            selectedEnvironmentsListBox.Items.AddRange(selectedItems);
            allEnvironmentsListBox.Items.RemoveRange(selectedItems);

            SelectedEnvironments.Clear();
            SelectedEnvironments.AddRange(selectedEnvironmentsListBox.Items.ToArray<EnvironmentConfig>());
        }

        private void removeButton_Click(object sender, System.EventArgs e)
        {
            var selectedItems = selectedEnvironmentsListBox.SelectedItems.ToArray<EnvironmentConfig>();
            if (selectedItems.Length == 0)
            {
                return;
            }

            allEnvironmentsListBox.Items.AddRange(selectedItems);
            selectedEnvironmentsListBox.Items.RemoveRange(selectedItems);

            SelectedEnvironments.Clear();
            SelectedEnvironments.AddRange(selectedEnvironmentsListBox.Items.ToArray<EnvironmentConfig>());
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
