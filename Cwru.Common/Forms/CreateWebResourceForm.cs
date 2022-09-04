using Cwru.Common;
using Cwru.Common.Model;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cwru.Forms
{
    public partial class CreateWebResourceForm : Form
    {
        private string Prefix { get; set; }
        public Func<WebResource, Task> OnCreate;
        public string ProjectItemPath { get; set; }
        private readonly Logger logger;

        public CreateWebResourceForm(Logger logger, string filePath, string prefix)
        {
            this.Prefix = prefix;
            this.ProjectItemPath = filePath;
            this.logger = logger;

            InitializeComponent();
        }

        private async void bCreateClick(object sender, EventArgs e)
        {
            try
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

                var webresourceName = prefix + "_" + name;

                var webResource = new WebResource()
                {
                    Name = webresourceName,
                    DisplayName = tbDisplayName.Text,
                    Description = tbDescription.Text,
                    Type = cbType.SelectedIndex + 1
                };

                bCreate.Enabled = false;
                bCancel.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;
                await OnCreate(webResource);

            }
            catch (Exception ex)
            {
                await logger.WriteLineAsync($"Failed to create web resource: {ex}");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                bCreate.Enabled = true;
                bCancel.Enabled = true;
            }
        }


        private void bCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CreateWebResourceFormLoad(object sender, EventArgs e)
        {
            var prefix = Prefix == null ? "" : Prefix;
            var name = Path.GetFileName(ProjectItemPath);
            var extension = Path.GetExtension(ProjectItemPath).ToLower();

            var re = new Regex("^" + prefix + "_");
            name = re.Replace(name, "");


            tbPrefix.Text = prefix;
            tbName.Text = name;
            tbDisplayName.Text = prefix + "_" + name;
            tbDescription.Text = "";

            cbType.Items.Add("Webpage (HTML)");
            cbType.Items.Add("Stylesheet (CSS)");
            cbType.Items.Add("Script (JScript)");
            cbType.Items.Add("Data (XML)");
            cbType.Items.Add("Image (PNG)");
            cbType.Items.Add("Image (JPG)");
            cbType.Items.Add("Image (GIF)");
            cbType.Items.Add("Silverlight (XAP)");
            cbType.Items.Add("Stylesheet (XSL)");
            cbType.Items.Add("Image (ICO)");
            cbType.Items.Add("Vector format (SVG)");
            cbType.Items.Add("String (RESX)");
            switch (extension)
            {
                case ".htm":
                case ".html": { cbType.SelectedIndex = 0; break; }
                case ".css": { cbType.SelectedIndex = 1; break; }
                case ".js": { cbType.SelectedIndex = 2; break; }
                case ".xml": { cbType.SelectedIndex = 3; break; }
                case ".png": { cbType.SelectedIndex = 4; break; }
                case ".jpg":
                case ".jpeg": { cbType.SelectedIndex = 5; break; }
                case ".gif": { cbType.SelectedIndex = 6; break; }
                case ".xap": { cbType.SelectedIndex = 7; break; }
                case ".xsl": { cbType.SelectedIndex = 8; break; }
                case ".ico": { cbType.SelectedIndex = 9; break; }
                case ".svg": { cbType.SelectedIndex = 10; break; }
                case ".resx": { cbType.SelectedIndex = 11; break; }
                default: { cbType.SelectedIndex = -1; break; }
            }
        }
    }
}
