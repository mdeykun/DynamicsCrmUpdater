using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms
{
    public partial class UpdateSolutionForm : Form
    {
        private readonly ConnectionDetail connectionDetail;
        public UpdateSolutionForm(ConnectionDetail connectionDetail)
        {
            InitializeComponent();
            lbSolution.Visible = false;
            comboBoxSolutions.Visible = false;
            btnOk.Visible = false;

            this.connectionDetail = connectionDetail;
            lblHeaderDesc.Text = string.Format(lblHeaderDesc.Text, connectionDetail.ConnectionName);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            var cbItem = (Solution)comboBoxSolutions.SelectedItem;
            if(cbItem != null)
            {
                connectionDetail.SelectedSolution = cbItem.SolutionDetail;
            }
            else
            {
                connectionDetail.SelectedSolution = null;
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
                var solutionDetails = new List<SolutionDetail>();
                if (connectionDetail.IsFromSdkLoginCtrl)
                {
                    var cd = connectionDetail;

                    var ctrl = new CRMLoginForm1(cd.ConnectionId.Value);
                    if (cd.AzureAdAppId != Guid.Empty)
                    {
                        ctrl.AppId = cd.AzureAdAppId.ToString();
                        ctrl.RedirectUri = new Uri(cd.ReplyUrl);
                    }

                    ctrl.ShowDialog();

                    ConnectionManager.Instance.ConnectToServerWithSdkLoginCtrl(connectionDetail, ctrl.CrmConnectionMgr.CrmSvc, null);
                }
                solutionDetails = await connectionDetail.GetSolutionsListAsync();

                if (solutionDetails.Count == 0)
                {
                    throw new Exception("Failed to load solutions");
                }
                var solutions = solutionDetails.Select(x => new Solution() { SolutionDetail = x });
                comboBoxSolutions.Items.AddRange(solutions.ToArray());
                if (connectionDetail != null && connectionDetail.SelectedSolution != null)
                {
                    var selectedSolution = connectionDetail.Solutions.FirstOrDefault(x => x.SolutionId == connectionDetail.SelectedSolution.SolutionId);
                    if (selectedSolution != null)
                    {
                        var index = connectionDetail.Solutions.IndexOf(selectedSolution);
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
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }
    }
}
