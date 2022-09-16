using Cwru.Common.Extensions;
using Cwru.Common.Model;
using Cwru.Common.Services;
using Cwru.CrmRequests.Common.Contracts;
using McTools.Xrm.Connection.WinForms.CustomControls;
using McTools.Xrm.Connection.WinForms.Extensions;
using McTools.Xrm.Connection.WinForms.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms
{
    public partial class ConnectionWizard : Form
    {
        private readonly bool isNew;
        private readonly List<Type> navigationHistory = new List<Type>();
        private UserControl ctrl;
        private string lastError;
        private ConnectionDetail originalDetail;
        private ConnectionType type;

        private readonly ICrmRequests crmRequests;
        private readonly SolutionsService solutionsService;

        public ConnectionWizard(ICrmRequests crmRequests, SolutionsService solutionsService, ConnectionDetail detail = null)
        {
            InitializeComponent();

            isNew = detail == null;
            originalDetail = (ConnectionDetail)detail?.Clone();
            CrmConnectionDetail = detail ?? new ConnectionDetail(true);

            Text = originalDetail == null ? "New connection" : "Update connection";

            btnBack.Visible = false;
            btnReset.Visible = false;

            this.crmRequests = crmRequests;
            this.solutionsService = solutionsService;
        }

        public ConnectionDetail CrmConnectionDetail { get; private set; }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        #region Buttons events

        private void btnBack_Click(object sender, EventArgs e)
        {
            navigationHistory.RemoveAt(navigationHistory.Count - 1);
            var type = navigationHistory.Last();
            navigationHistory.RemoveAt(navigationHistory.Count - 1);

            if (type == typeof(ConnectionFirstStepControl))
                DisplayControl<ConnectionFirstStepControl>();
            else if (type == typeof(ConnectionCredentialsControl))
                DisplayControl<ConnectionCredentialsControl>();
            else if (type == typeof(ConnectionFailedControl))
                DisplayControl<ConnectionFailedControl>();
            else if (type == typeof(ConnectionIfdControl))
                DisplayControl<ConnectionIfdControl>();
            else if (type == typeof(ConnectionLoadingControl))
                DisplayControl<ConnectionLoadingControl>();
            else if (type == typeof(ConnectionOauthControl))
                DisplayControl<ConnectionOauthControl>();
            else if (type == typeof(ConnectionStringControl))
                DisplayControl<ConnectionStringControl>();
            else if (type == typeof(ConnectionSucceededControl))
                DisplayControl<ConnectionSucceededControl>();
            else if (type == typeof(StartPageControl))
                DisplayControl<StartPageControl>();
            else if (type == typeof(ConnectionCertificateControl))
                DisplayControl<ConnectionCertificateControl>();
            else if (type == typeof(ConnectionUrlControl))
                DisplayControl<ConnectionUrlControl>();
            else if (type == typeof(ConnectionClientSecretControl))
                DisplayControl<ConnectionClientSecretControl>();
            else if (type == typeof(ConnectionAppIdControl))
                DisplayControl<ConnectionAppIdControl>();
            else if (type == typeof(ConnectionMfaControl))
                DisplayControl<ConnectionMfaControl>();
        }

        private async void btnNext_Click(object sender, EventArgs e)
        {
            if (ctrl is ConnectionFirstStepControl cfsc)
            {
                CrmConnectionDetail.OriginalUrl = cfsc.Url;
                CrmConnectionDetail.IntegratedSecurity = cfsc.UseIntegratedAuth;
                CrmConnectionDetail.UseMfa = cfsc.UseMfa;
                CrmConnectionDetail.Timeout = cfsc.Timeout;

                if (CrmConnectionDetail.Timeout.Ticks == 0 || CrmConnectionDetail.OriginalUrl == null)
                {
                    return;
                }

                if (CrmConnectionDetail.OrganizationUrlName == null)
                {
                    if (CrmConnectionDetail.UseOnline)
                    {
                        if (!cfsc.UseIntegratedAuth)
                        {
                            if (CrmConnectionDetail.UseMfa)
                            {
                                DisplayControl<ConnectionOauthControl>();
                            }
                            else if (!CrmConnectionDetail.UseOnline && CrmConnectionDetail.OriginalUrl.Split('.').Length > 1)
                            {
                                DisplayControl<ConnectionIfdControl>();
                            }
                            else
                            {
                                DisplayControl<ConnectionCredentialsControl>();
                            }
                        }
                        else
                        {
                            DisplayControl<ConnectionLoadingControl>();
                            await Connect();
                        }
                    }
                    else
                    {
                        DisplayControl<ConnectionIfdControl>();
                    }
                }
                else
                {
                    if (CrmConnectionDetail.IntegratedSecurity != true)
                    {
                        if (CrmConnectionDetail.UseMfa)
                        {
                            DisplayControl<ConnectionOauthControl>();
                        }
                        else if (!CrmConnectionDetail.UseOnline && CrmConnectionDetail.OriginalUrl.Split('.').Length > 1)
                        {
                            DisplayControl<ConnectionIfdControl>();
                        }
                        else
                        {
                            DisplayControl<ConnectionCredentialsControl>();
                        }
                    }
                    else
                    {
                        DisplayControl<ConnectionLoadingControl>();
                        await Connect();
                    }
                }
            }
            else if (ctrl is ConnectionCredentialsControl ccc)
            {
                CrmConnectionDetail.UserDomain = ccc.Domain;
                CrmConnectionDetail.UserName = ccc.Username;
                CrmConnectionDetail.SavePassword = ccc.SavePassword;

                if (ccc.PasswordChanged)
                {
                    CrmConnectionDetail.UserPassword = ccc.Password.ToSecureString();
                }

                if (string.IsNullOrEmpty(CrmConnectionDetail.UserName) || CrmConnectionDetail.UserPassword == null)
                {
                    MessageBox.Show(this,
                        @"Please enter your credentials before trying to connect",
                        @"Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                if (originalDetail == null)
                {
                    DisplayControl<ConnectionLoadingControl>();
                    await Connect();
                }
                else if (CrmConnectionDetail.IsConnectionBrokenWithUpdatedData(originalDetail))
                {
                    if (DialogResult.Yes == MessageBox.Show(this, @"You changed some values that require to test the connection. Would you like to test it now?

                Note that this is required to validate this wizard",
                            @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        DisplayControl<ConnectionLoadingControl>();
                        await Connect();
                    }
                }
                else
                {
                    DisplayControl<ConnectionSucceededControl>();
                }
            }
            else if (ctrl is ConnectionIfdControl cic)
            {
                CrmConnectionDetail.UseIfd = cic.IsIfd;
                CrmConnectionDetail.HomeRealmUrl = cic.HomeRealmUrl;
                CrmConnectionDetail.AuthType = cic.IsIfd ? AuthenticationType.IFD : AuthenticationType.AD;

                if (CrmConnectionDetail.IntegratedSecurity != true)
                {
                    DisplayControl<ConnectionCredentialsControl>();
                }
                else if (originalDetail == null)
                {
                    DisplayControl<ConnectionLoadingControl>();
                    await Connect();
                }
                else
                {
                    if (CrmConnectionDetail.IsConnectionBrokenWithUpdatedData(originalDetail))
                    {
                        if (DialogResult.Yes == MessageBox.Show(this, @"You changed some values that require to test the connection. Would you like to test it now?

                Note that this is required to validate this wizard",
                                @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                        {
                            DisplayControl<ConnectionLoadingControl>();
                            await Connect();
                        }
                    }
                    else
                    {
                        DisplayControl<ConnectionSucceededControl>();
                    }
                }
            }
            else if (ctrl is ConnectionOauthControl coc)
            {
                CrmConnectionDetail.AzureAdAppId = coc.AzureAdAppId;
                CrmConnectionDetail.ReplyUrl = coc.ReplyUrl;

                if (coc.ClientSecretChanged)
                {
                    CrmConnectionDetail.ClientSecret = coc.ClientSecret.ToSecureString();
                }

                if (CrmConnectionDetail.AzureAdAppId == Guid.Empty
                    || string.IsNullOrEmpty(CrmConnectionDetail.ReplyUrl))
                {
                    MessageBox.Show(this,
                        @"Please provide all information for OAuth authentication",
                        @"Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                if (CrmConnectionDetail.ClientSecret != null)
                {
                    DisplayControl<ConnectionLoadingControl>();
                    await Connect();
                }
                else
                {
                    DisplayControl<ConnectionCredentialsControl>();
                }
            }
            else if (ctrl is ConnectionStringControl csc)
            {
                CrmConnectionDetail.ConnectionString = csc.ConnectionString;

                DisplayControl<ConnectionLoadingControl>();
                await Connect();
            }
            else if (ctrl is ConnectionSucceededControl cokc)
            {
                CrmConnectionDetail.ConnectionName = cokc.ConnectionName;
                CrmConnectionDetail.SelectedSolutionId = cokc.SelectedSolution?.SolutionId;
                CrmConnectionDetail.SolutionName = cokc.SelectedSolution?.FriendlyName;

                DialogResult = DialogResult.OK;
                Close();
            }
            //if (ctrl is SdkLoginControlControl slcc)
            //{
            //    CrmConnectionDetail = slcc.ConnetctionDetail;
            //    DisplayControl<ConnectionSucceededControl>();
            //}
            else if (ctrl is ConnectionUrlControl cuc)
            {
                if (string.IsNullOrEmpty(cuc.Url) || !Uri.TryCreate(cuc.Url, UriKind.Absolute, out _))
                {
                    MessageBox.Show(this, @"Please provide a valid url", @"Warning", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                CrmConnectionDetail.OriginalUrl = cuc.Url;
                CrmConnectionDetail.Timeout = cuc.Timeout;

                if (type == ConnectionType.Certificate)
                {
                    DisplayControl<ConnectionCertificateControl>();
                }
                else if (type == ConnectionType.ClientSecret)
                {
                    DisplayControl<ConnectionClientSecretControl>();
                }
                else if (type == ConnectionType.Mfa)
                {
                    DisplayControl<ConnectionMfaControl>();
                }
            }
            else if (ctrl is ConnectionClientSecretControl ccsc)
            {
                CrmConnectionDetail.AzureAdAppId = ccsc.AzureAdAppId;
                CrmConnectionDetail.AuthType = AuthenticationType.ClientSecret;

                if (ccsc.ClientSecretChanged)
                {
                    CrmConnectionDetail.ClientSecret = ccsc.ClientSecret.ToSecureString();
                }

                if (CrmConnectionDetail.AzureAdAppId == Guid.Empty
                    || CrmConnectionDetail.ClientSecret == null)
                {
                    MessageBox.Show(this,
                        @"Please provide all information for Client Id/Secret authentication",
                        @"Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                if (CrmConnectionDetail.ClientSecret != null)
                {
                    DisplayControl<ConnectionLoadingControl>();
                    await Connect();
                }
            }
            else if (ctrl is ConnectionMfaControl cmfac)
            {
                CrmConnectionDetail.AzureAdAppId = cmfac.AzureAdAppId;
                CrmConnectionDetail.ReplyUrl = cmfac.ReplyUrl;
                CrmConnectionDetail.UserName = cmfac.Username;
                CrmConnectionDetail.AzureAdAppId = cmfac.AzureAdAppId;
                CrmConnectionDetail.AuthType = AuthenticationType.OAuth;
                CrmConnectionDetail.UseMfa = true;

                if (CrmConnectionDetail.AzureAdAppId == Guid.Empty
                    || string.IsNullOrEmpty(CrmConnectionDetail.ReplyUrl))
                {
                    MessageBox.Show(this,
                        @"Please provide at least Application Id and Reply Url for multi factor authentication",
                        @"Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                DisplayControl<ConnectionLoadingControl>();
                await Connect();
            }
            else if (ctrl is ConnectionCertificateControl ccertc)
            {
                CrmConnectionDetail.Certificate = new CertificateInfo
                {
                    Thumbprint = ccertc.Certificate.Thumbprint,
                    Issuer = ccertc.Certificate.Issuer,
                    Name = ccertc.Certificate.GetNameInfo(X509NameType.SimpleName, false)
                };

                CrmConnectionDetail.AuthType = AuthenticationType.Certificate;

                DisplayControl<ConnectionAppIdControl>();
            }
            else if (ctrl is ConnectionAppIdControl cac)
            {
                if (Guid.TryParse(cac.AppId, out Guid appId))
                {
                    CrmConnectionDetail.AzureAdAppId = appId;

                    DisplayControl<ConnectionLoadingControl>();
                    await Connect();
                }
                else
                {
                    MessageBox.Show(this, @"Invalid Application Id", @"Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            CrmConnectionDetail = new ConnectionDetail(true);
            navigationHistory.Clear();
            DisplayControl<StartPageControl>();
        }

        #endregion Buttons events

        private async Task Connect()
        {
            try
            {
                var currentDetail = CrmConnectionDetail;

                var crmConnectionString = currentDetail.ToCrmConnectionString();
                var validationResponse = await crmRequests.ValidateConnectionAsync(crmConnectionString.BuildConnectionString());
                if (validationResponse.IsSuccessful == false)
                {
                    throw new Exception($"Failed to validate connection: {validationResponse.ErrorMessage}");
                }
                var connectionResult = validationResponse.Payload;

                if (!connectionResult.IsReady)
                {
                    lastError = connectionResult.LastCrmError;
                    DisplayControl<ConnectionFailedControl>();

                    return;
                }

                CrmConnectionDetail.Organization = connectionResult.OrganizationUniqueName;
                CrmConnectionDetail.OrganizationVersion = connectionResult.OrganizationVersion;

                DisplayControl<ConnectionSucceededControl>();
            }
            catch (Exception ex)
            {
                lastError = ex.ToString();
                DisplayControl<ConnectionFailedControl>();
            }
        }

        private void ConnectionWizard2_Load(object sender, EventArgs e)
        {
            if (!isNew)
            {
                if (CrmConnectionDetail.UseConnectionString)
                {
                    DisplayControl<ConnectionStringControl>();
                }
                else if (CrmConnectionDetail.Certificate != null)
                {
                    type = ConnectionType.Certificate;
                    DisplayControl<ConnectionUrlControl>();
                }
                else if (CrmConnectionDetail.AuthType == AuthenticationType.ClientSecret)
                {
                    type = ConnectionType.ClientSecret;
                    DisplayControl<ConnectionUrlControl>();
                }
                else if (CrmConnectionDetail.UseMfa)
                {
                    type = ConnectionType.Mfa;
                    DisplayControl<ConnectionUrlControl>();
                }
                else
                {
                    DisplayControl<ConnectionFirstStepControl>();
                }
            }
            else
            {
                DisplayControl<StartPageControl>();
            }
        }

        private void DisplayControl<T>() where T : UserControl
        {
            btnBack.Visible = navigationHistory.Count > 0;

            if (typeof(T) != typeof(ConnectionLoadingControl))
                navigationHistory.Add(typeof(T));

            if (typeof(T) == typeof(StartPageControl))
            {
                pnlFooter.Visible = false;
                lblHeader.Text = @"Choose a connection method";

                ctrl = new StartPageControl();
                ((StartPageControl)ctrl).TypeSelected += (sender, e) =>
                {
                    type = ((StartPageControl)ctrl).Type;

                    switch (((StartPageControl)ctrl).Type)
                    {
                        case ConnectionType.Wizard:
                            DisplayControl<ConnectionFirstStepControl>();
                            break;
                        case ConnectionType.ConnectionString:
                            DisplayControl<ConnectionStringControl>();
                            break;

                        case ConnectionType.Certificate:
                            DisplayControl<ConnectionUrlControl>();
                            break;

                        case ConnectionType.ClientSecret:
                            DisplayControl<ConnectionUrlControl>();
                            break;

                        case ConnectionType.Mfa:
                            DisplayControl<ConnectionUrlControl>();
                            break;
                    }
                };

                btnReset.Visible = false;
                btnNext.Visible = false;
            }
            else if (typeof(T) == typeof(ConnectionFirstStepControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"General information and options";

                var timespan = CrmConnectionDetail?.Timeout;
                if (!timespan.HasValue || timespan.Value.Ticks == 0)
                {
                    timespan = new TimeSpan(0, 2, 0);
                }
                ctrl = new ConnectionFirstStepControl
                {
                    Url = CrmConnectionDetail?.OriginalUrl,
                    UseIntegratedAuth = !isNew && CrmConnectionDetail?.IntegratedSecurity == true,
                    UseMfa = CrmConnectionDetail?.UseMfa ?? false,
                    Timeout = timespan.Value
                };

                btnReset.Visible = true;
                btnNext.Visible = true;
                btnNext.Text = @"Next";
            }
            else if (typeof(T) == typeof(ConnectionCredentialsControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"User credentials";

                ctrl = new ConnectionCredentialsControl
                {
                    Domain = CrmConnectionDetail?.UserDomain,
                    Username = CrmConnectionDetail?.UserName,
                    IsOnline = CrmConnectionDetail?.UseOnline ?? false,
                    PasswordIsSet = CrmConnectionDetail?.UserPassword != null,
                    SavePassword = CrmConnectionDetail?.SavePassword ?? false,
                };

                btnReset.Visible = true;
                btnNext.Visible = true;
                btnNext.Text = @"Next";
            }
            else if (typeof(T) == typeof(ConnectionIfdControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"Internet Facing Deployment settings";

                ctrl = new ConnectionIfdControl
                {
                    IsIfd = CrmConnectionDetail?.UseIfd ?? false,
                    HomeRealmUrl = CrmConnectionDetail?.HomeRealmUrl
                };

                btnReset.Visible = true;
                btnNext.Visible = true;
                btnNext.Text = @"Next";
            }
            else if (typeof(T) == typeof(ConnectionLoadingControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"Connecting...";

                ctrl = new ConnectionLoadingControl();

                btnBack.Visible = false;
                btnReset.Visible = false;
                btnNext.Visible = false;
                btnNext.Text = @"Next";
            }
            else if (typeof(T) == typeof(ConnectionSucceededControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"Connection validated!";

                ctrl = new ConnectionSucceededControl(solutionsService)
                {
                    ConnectionName = CrmConnectionDetail.ConnectionName,
                    ConnectionDetail = CrmConnectionDetail
                };

                btnBack.Visible = true;
                btnReset.Visible = false;
                btnNext.Visible = true;
                btnNext.Text = @"Finish";
            }
            else if (typeof(T) == typeof(ConnectionFailedControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"Connection failed!";

                ctrl = new ConnectionFailedControl
                {
                    ErrorMEssage = lastError
                };

                btnBack.Visible = true;
                btnReset.Visible = true;
                btnNext.Visible = false;
                btnNext.Text = @"Finish";
            }
            else if (typeof(T) == typeof(ConnectionStringControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"Connectionstring settings";

                ctrl = new ConnectionStringControl
                {
                    ConnectionString = CrmConnectionDetail.ConnectionString
                };

                btnReset.Visible = true;
                btnNext.Visible = true;
                btnNext.Text = @"Next";
            }
            else if (typeof(T) == typeof(ConnectionOauthControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"OAuth protocol settings";

                ctrl = new ConnectionOauthControl
                {
                    AzureAdAppId = CrmConnectionDetail.AzureAdAppId,
                    ReplyUrl = CrmConnectionDetail.ReplyUrl,
                    HasClientSecret = CrmConnectionDetail.ClientSecret != null
                };

                btnReset.Visible = true;
                btnNext.Visible = true;
                btnNext.Text = @"Next";
            }
            else if (typeof(T) == typeof(ConnectionUrlControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"Provide environment information";

                if (!CrmConnectionDetail.ConnectionId.HasValue)
                {
                    CrmConnectionDetail.ConnectionId = Guid.NewGuid();
                }

                ctrl = new ConnectionUrlControl(CrmConnectionDetail);

                btnReset.Visible = true;
                btnNext.Visible = true;
                btnNext.Text = @"Next";
            }
            else if (typeof(T) == typeof(ConnectionCertificateControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"Connection with certificate";

                if (!CrmConnectionDetail.ConnectionId.HasValue)
                {
                    CrmConnectionDetail.ConnectionId = Guid.NewGuid();
                }

                ctrl = new ConnectionCertificateControl(CrmConnectionDetail);

                btnReset.Visible = true;
                btnNext.Visible = true;
                btnNext.Text = @"Next";
            }
            else if (typeof(T) == typeof(ConnectionAppIdControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"Application user Application ID";

                if (!CrmConnectionDetail.ConnectionId.HasValue)
                {
                    CrmConnectionDetail.ConnectionId = Guid.NewGuid();
                }

                ctrl = new ConnectionAppIdControl();
                if (CrmConnectionDetail.AzureAdAppId != Guid.Empty)
                {
                    ((ConnectionAppIdControl)ctrl).AppId = CrmConnectionDetail.AzureAdAppId.ToString("B");
                }

                btnReset.Visible = true;
                btnNext.Visible = true;
                btnNext.Text = @"Next";
            }
            else if (typeof(T) == typeof(ConnectionClientSecretControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"Client Id / Secret";

                if (!CrmConnectionDetail.ConnectionId.HasValue)
                {
                    CrmConnectionDetail.ConnectionId = Guid.NewGuid();
                }

                ctrl = new ConnectionClientSecretControl();
                ((ConnectionClientSecretControl)ctrl).HasClientSecret = CrmConnectionDetail.ClientSecret != null;
                if (CrmConnectionDetail.AzureAdAppId != Guid.Empty)
                {
                    ((ConnectionClientSecretControl)ctrl).AzureAdAppId = CrmConnectionDetail.AzureAdAppId;
                }

                btnReset.Visible = true;
                btnNext.Visible = true;
                btnNext.Text = @"Next";
            }
            else if (typeof(T) == typeof(ConnectionMfaControl))
            {
                pnlFooter.Visible = true;
                lblHeader.Text = @"Mutli Factor Authentication";

                if (!CrmConnectionDetail.ConnectionId.HasValue)
                {
                    CrmConnectionDetail.ConnectionId = Guid.NewGuid();
                }

                ctrl = new ConnectionMfaControl();
                ((ConnectionMfaControl)ctrl).Username = CrmConnectionDetail.UserName;
                ((ConnectionMfaControl)ctrl).ReplyUrl = CrmConnectionDetail.ReplyUrl;
                if (CrmConnectionDetail.AzureAdAppId != Guid.Empty)
                {
                    ((ConnectionMfaControl)ctrl).AzureAdAppId = CrmConnectionDetail.AzureAdAppId;
                }

                btnReset.Visible = true;
                btnNext.Visible = true;
                btnNext.Text = @"Next";
            }

            ctrl.Dock = DockStyle.Fill;
            pnlMain.Controls.Clear();
            pnlMain.Controls.Add((UserControl)ctrl);
        }
    }
}