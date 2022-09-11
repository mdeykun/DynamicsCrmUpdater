using Cwru.Common.Model;
using Cwru.Common.Services;
using McTools.Xrm.Connection.WinForms.Extensions;
using McTools.Xrm.Connection.WinForms.Model;
using System;
using System.Linq;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms.CustomControls
{
    public partial class ConnectionSucceededControl : UserControl, IConnectionWizardControl
    {
        private readonly SolutionsService solutionsService;

        public ConnectionSucceededControl(SolutionsService solutionsService)
        {
            this.solutionsService = solutionsService;
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
            var solutionsResponse = await solutionsService.GetSolutionsDetailsAsync(connectionInfo.BuildConnectionString(), ConnectionDetail.ConnectionId, true);
            var solutions = solutionsResponse.ToArray();

            comboBoxSolutions.Items.AddRange(solutions);

            if (ConnectionDetail != null && ConnectionDetail.SelectedSolutionId != null)
            {
                var selectedSolution = solutions.FirstOrDefault(x => x.SolutionId == ConnectionDetail.SelectedSolutionId);
                if (selectedSolution != null)
                {
                    var index = Array.IndexOf(solutions, selectedSolution);
                    comboBoxSolutions.SelectedIndex = index;
                }
            }
        }
    }
}