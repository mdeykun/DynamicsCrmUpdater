using Cwru.Common.Model;
using Cwru.Common.Services;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Cwru.Publisher.Forms
{
    public partial class CreateWebResourceForm : Form
    {
        private readonly WebResourceTypesService webResourceTypesService;
        private readonly SolutionDetail solutionDetail;
        private readonly string filePath;

        public WebResource WebResource { get; set; }

        public CreateWebResourceForm(
            string filePath,
            SolutionDetail solutionDetail,
            WebResourceTypesService webResourceTypesService)
        {
            this.webResourceTypesService = webResourceTypesService;
            this.solutionDetail = solutionDetail;
            this.filePath = filePath;

            InitializeComponent();
        }

        private void bCreate_Click(object sender, EventArgs e)
        {
            var name = tbName.Text;
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name can not be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var prefix = tbPrefix.Text;
            if (string.IsNullOrEmpty(prefix))
            {
                MessageBox.Show("Prefix can not be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cbType.SelectedIndex < 0)
            {
                MessageBox.Show("Please select web resource type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            WebResource = new WebResource()
            {
                Name = prefix + "_" + name,
                DisplayName = tbDisplayName.Text,
                Description = tbDescription.Text,
                Type = (WebResourceType)cbType.SelectedIndex + 1
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CreateWebResourceForm_Load(object sender, EventArgs e)
        {
            var prefix = solutionDetail.PublisherPrefix ?? "";

            var name = Path.GetFileName(filePath);
            name = new Regex($"^{prefix}_").Replace(name, "");

            tbPrefix.Text = prefix;
            tbName.Text = name;
            tbDisplayName.Text = prefix + "_" + name;

            var webResourcesTypes = webResourceTypesService.GetTypesLabels();
            var extension = Path.GetExtension(filePath).ToLower();
            var webResourceType = webResourceTypesService.GetTypeByExtension(extension);

            cbType.Items.AddRange(webResourcesTypes.Values.ToArray());
            cbType.SelectedIndex = webResourceType != null ? webResourcesTypes.Keys.ToList().IndexOf(webResourceType.Value) : -1;
        }
    }
}
