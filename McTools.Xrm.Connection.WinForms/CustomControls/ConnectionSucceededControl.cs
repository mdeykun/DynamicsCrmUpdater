using Cwru.Common.Model;
using Cwru.CrmRequests.Common.Contracts;
using McTools.Xrm.Connection.WinForms.Extensions;
using McTools.Xrm.Connection.WinForms.Model;
using System;
using System.Linq;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms.CustomControls
{
    public partial class ConnectionSucceededControl : UserControl, IConnectionWizardControl
    {
        private readonly ICrmRequests crmRequestsClient;

        public ConnectionSucceededControl(ICrmRequests crmRequestsClient)
        {
            this.crmRequestsClient = crmRequestsClient;
            InitializeComponent();
        }

        public ConnectionDetail ConnectionDetail { private get; set; }

        public string ConnectionName
        {
            get => txtConnectionName.Text;
            set => txtConnectionName.Text = value;
        }

        public SolutionDetail SelectedSolution
        {
            get => (SolutionDetail)comboBoxSolutions.SelectedItem;
        }

        private async void ConnectionSucceededControl_Load(object sender, System.EventArgs e)
        {
            var connectionInfo = ConnectionDetail.ToCrmConnectionString();
            var solutionsResponse = await crmRequestsClient.GetSolutionsListAsync(connectionInfo.BuildConnectionString());

            if (solutionsResponse.IsSuccessful == false)
            {
                throw new Exception($"Failed to retrieve solutions: {solutionsResponse.Error}");
            }

            var solutions = solutionsResponse.Payload.ToList();
            comboBoxSolutions.Items.AddRange(solutions.ToArray());

            if (ConnectionDetail != null && ConnectionDetail.SelectedSolutionId != null)
            {
                var selectedSolution = solutions.FirstOrDefault(x => x.SolutionId == ConnectionDetail.SelectedSolutionId);
                if (selectedSolution != null)
                {
                    var index = solutions.IndexOf(selectedSolution);
                    comboBoxSolutions.SelectedIndex = index;
                }
            }
        }
    }
}