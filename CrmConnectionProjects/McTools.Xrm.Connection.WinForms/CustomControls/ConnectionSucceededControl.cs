using System.Linq;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms.CustomControls
{
    public partial class ConnectionSucceededControl : UserControl, IConnectionWizardControl
    {
        public ConnectionSucceededControl()
        {
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
            get => ((Solution)comboBoxSolutions.SelectedItem).SolutionDetail;
        }
        private void btnClearEnvHighlight_Click(object sender, System.EventArgs e)
        {
            ConnectionDetail.EnvironmentHighlightingInfo = null;
            //ConnectionDetail.EnvironmentColor = null;
            //ConnectionDetail.EnvironmentTextColor = null;
            //ConnectionDetail.EnvironmentText = null;

            btnClearEnvHighlight.Visible = false;
        }

        private void btnSetEnvHighlight_Click(object sender, System.EventArgs e)
        {
            var dialog = new EnvHighlightDialog(ConnectionDetail);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                ConnectionDetail.EnvironmentHighlightingInfo = new EnvironmentHighlighting
                {
                    Color = dialog.BackColorSelected,
                    TextColor = dialog.TextColorSelected,
                    Text = dialog.TextSelected
                };

                btnClearEnvHighlight.Visible = true;
            }
        }

        private void ConnectionSucceededControl_Load(object sender, System.EventArgs e)
        {
            var solutions = ConnectionDetail.Solutions.Select(x => new Solution() { SolutionDetail = x }).ToList();
            comboBoxSolutions.Items.AddRange(solutions.ToArray());
            if(ConnectionDetail != null && ConnectionDetail.SelectedSolution != null)
            {
                var selectedSolution = ConnectionDetail.Solutions.FirstOrDefault(x => x.SolutionId == ConnectionDetail.SelectedSolution.SolutionId);
                if (selectedSolution != null)
                {
                    var index = ConnectionDetail.Solutions.IndexOf(selectedSolution);
                    comboBoxSolutions.SelectedIndex = index;
                }
            }
            if (!ConnectionManager.Instance.FromXrmToolBox)
            {
                lblHighlight.Visible = false;
                btnClearEnvHighlight.Visible = ConnectionDetail.IsEnvironmentHighlightSet;
                btnSetEnvHighlight.Visible = false;
            }
            else
            {
                btnClearEnvHighlight.Visible = ConnectionDetail.IsEnvironmentHighlightSet;
                lblHighlight.Visible = true;
                btnSetEnvHighlight.Visible = true;
            }
        }
    }
}