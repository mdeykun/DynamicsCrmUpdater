using CrmWebResourcesUpdater.DataModels;
using CrmWebResourcesUpdater.Service.Client;
using System;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms.CustomControls
{
    public partial class SdkLoginControlControl : UserControl, IConnectionWizardControl
    {
        private readonly Guid connectionDetailId;
        private readonly bool isNew;
        private readonly CrmWebResourcesUpdaterClient crmWebResourceUpdaterClient;

        public ConnectionDetail ConnetctionDetail = null;

        public SdkLoginControlControl(Guid connectionDetailId, bool isNew)
        {
            InitializeComponent();

            this.connectionDetailId = connectionDetailId;
            this.isNew = isNew;
            crmWebResourceUpdaterClient = CrmWebResourcesUpdaterClient.Instance;
        }

        public event EventHandler ConnectionSucceeded;

        public AuthenticationProviderType AuthType { get; private set; }

        private async void btnOpenSdkLoginCtrl_Click(object sender, EventArgs e)
        {
            var connectionDetail = await crmWebResourceUpdaterClient.UseSdkLoginControlAsync(this.connectionDetailId, false);
            if (connectionDetail != null) {
                ConnetctionDetail = connectionDetail;
                ConnectionSucceeded?.Invoke(this, null);
            }
            //txtReplyUrl.Text
            //txtAzureAdAppId.Text
            //rdbUseCustom.Checked
        }

        private void rdbUseCustom_CheckedChanged(object sender, EventArgs e)
        {
            tableLayoutPanel1.Enabled = rdbUseCustom.Checked;
        }

        private void SdkLoginControlControl_Load(object sender, System.EventArgs e)
        {
        }

        //private void SetAuthType()
        //{
        //    switch (ConnectionManager.CrmSvc.ActiveAuthenticationType)
        //    {
        //        case AuthenticationType.AD:
        //            AuthType = AuthenticationProviderType.ActiveDirectory;
        //            break;

        //        case AuthenticationType.IFD:
        //        case AuthenticationType.Claims:
        //            AuthType = AuthenticationProviderType.Federation;
        //            break;

        //        default:
        //            AuthType = AuthenticationProviderType.OnlineFederation;
        //            break;
        //    }
        //}
    }
}