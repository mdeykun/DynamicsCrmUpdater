using Cwru.Common.Model;
using Cwru.Common.Services;
using McTools.Xrm.Connection.WinForms.Extensions;
using McTools.Xrm.Connection.WinForms.Model;
using System;
using System.Linq;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms
{
    public partial class UpdateSolutionForm : Form
    {
        private readonly ConnectionDetail connectionDetail;
        private readonly SolutionsService solutionsService;

        public UpdateSolutionForm(
            SolutionsService solutionsService,
            ConnectionDetail connectionDetail)
        {
            InitializeComponent();
            lbSolution.Visible = false;
            comboBoxSolutions.Visible = false;
            btnOk.Visible = false;

            this.connectionDetail = connectionDetail;
            lblHeaderDesc.Text = string.Format(lblHeaderDesc.Text, connectionDetail.ConnectionName);
            this.solutionsService = solutionsService;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            var cbItem = comboBoxSolutions.SelectedItem;
            if (cbItem != null)
            {
                connectionDetail.SelectedSolutionId = ((SolutionDetail)cbItem).SolutionId;
                connectionDetail.SolutionName = ((SolutionDetail)cbItem).FriendlyName;
            }
            else
            {
                connectionDetail.SelectedSolutionId = null;
                connectionDetail.SolutionName = null;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void UpdateSolutionForm_Load(object sender, EventArgs e)
        {
            try
            {
                var connectionInfo = connectionDetail.ToCrmConnectionString();
                var solutionsResponse = await solutionsService.GetSolutionsDetailsAsync(connectionInfo.BuildConnectionString(), connectionDetail.ConnectionId, true);
                var solutions = solutionsResponse.ToArray();

                if (solutions.Length == 0)
                {
                    throw new Exception("Failed to load solutions");
                }

                comboBoxSolutions.Items.AddRange(solutions.ToArray());
                if (connectionDetail != null && connectionDetail.SelectedSolutionId != null)
                {
                    var selectedSolution = solutions.FirstOrDefault(x => x.SolutionId == connectionDetail.SelectedSolutionId);
                    if (selectedSolution != null)
                    {
                        var index = Array.IndexOf(solutions, selectedSolution);
                        comboBoxSolutions.SelectedIndex = index;
                    }
                }

                lbProgressBar.Visible = false;
                progressBar1.Visible = false;

                lbSolution.Visible = true;
                comboBoxSolutions.Visible = true;
                btnOk.Visible = true;

                comboBoxSolutions.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }
    }
}
