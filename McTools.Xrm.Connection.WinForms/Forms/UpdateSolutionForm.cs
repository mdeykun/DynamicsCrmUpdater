using Cwru.Common.Model;
using Cwru.CrmRequests.Common.Contracts;
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
        private readonly ICrmRequests crmRequestsClient;

        public UpdateSolutionForm(
            ICrmRequests crmRequestsClient,
            ConnectionDetail connectionDetail)
        {
            InitializeComponent();
            lbSolution.Visible = false;
            comboBoxSolutions.Visible = false;
            btnOk.Visible = false;

            this.connectionDetail = connectionDetail;
            lblHeaderDesc.Text = string.Format(lblHeaderDesc.Text, connectionDetail.ConnectionName);
            this.crmRequestsClient = crmRequestsClient;
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
                var getSolutionsListResponse = await crmRequestsClient.GetSolutionsListAsync(connectionInfo.BuildConnectionString());

                if (getSolutionsListResponse.IsSuccessful == false)
                {
                    throw new Exception($"Failed to retrieve solution list: {getSolutionsListResponse.Error}");
                }

                var solutions = getSolutionsListResponse.Payload.ToList();
                if (solutions.Count == 0)
                {
                    throw new Exception("Failed to load solutions");
                }

                comboBoxSolutions.Items.AddRange(solutions.ToArray());
                if (connectionDetail != null && connectionDetail.SelectedSolutionId != null)
                {
                    var selectedSolution = solutions.FirstOrDefault(x => x.SolutionId == connectionDetail.SelectedSolutionId);
                    if (selectedSolution != null)
                    {
                        var index = solutions.IndexOf(selectedSolution);
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
