using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Linq;
using Microsoft.Xrm.Tooling.Connector;
using System.ServiceModel.Description;
using System.Threading.Tasks;

namespace McTools.Xrm.Connection.WinForms
{
    /// <summary>
    /// Formulaire de création et d'édition d'une connexion
    /// à un serveur Crm.
    /// </summary>
    public partial class ConnectionForm : Form
    {
        #region Variables

        /// <summary>
        /// Détail de connexion courant
        /// </summary>
        ConnectionDetail detail;

        /// <summary>
        /// List of Crm server organizations
        /// </summary>
        List<OrganizationDetail> organizations;

        /// <summary>
        /// List of organization solutions
        /// </summary>
        List<SolutionDetail> solutions;

        /// <summary>
        /// Indique si l'utilisateur a demandé à se connecter
        /// au serveur
        /// </summary>
        bool doConnect;

        private bool proposeToConnect;
        public CrmConnections ConnectionList { get; set; }

        #endregion

        #region Propriétés

        /// <summary>
        /// Obtient la valeur qui définit si l'utilisateur a demandé 
        /// à se connecter au serveur
        /// </summary>
        public bool DoConnect
        {
            get { return doConnect; }
        }

        /// <summary>
        /// Obtient ou définit le détail de la connexion courante
        /// </summary>
        public ConnectionDetail CrmConnectionDetail
        {
            get { return detail; }
            set { detail = value; }
        }

        readonly bool isCreationMode;


        #endregion

        #region Constructeur

        /// <summary>
        /// Créé une nouvelle instance de la classe ConnectionForm
        /// </summary>
        public ConnectionForm(bool isCreation, bool proposeToConnect = true)
        {
            InitializeComponent();
            isCreationMode = isCreation;
            this.proposeToConnect = proposeToConnect;
            cbbOnlineEnv.SelectedIndex = 0;

            var tip = new ToolTip { ToolTipTitle = "Information" };
            tip.SetToolTip(tbServerName, "For CRM Online or Office 365, use:\r\ncrm.dynamics.com for North America\r\ncrm2.dynamics.com for LATAM\r\ncrm4.dynamics.com for EMEA\r\ncrm5.dynamics.com for Asia Pacific\r\ncrm9.dynamics.com for CRM Online for Government Instances\r\n\r\nFor OnPremise:\r\nUse the server name\r\n\r\nFor IFD:\r\nUse <discovery_name>.<domain>.<extension>");
            tip.SetToolTip(tbServerPort, "Specify port only if different from 80 or 443 (SSL)");
            tip.SetToolTip(tbHomeRealmUrl, "(Optional) In specific case, you should need to specify the home realm url to authenticate through ADFS");
            
        }

       


        #endregion

        #region Méthodes

        protected override void OnLoad(EventArgs e)
        {
            if (detail != null)
            {
                FillValues();
            }


            if (proposeToConnect == false && isCreationMode == false)
            {
                if (!rbAuthenticationCustom.Checked || !String.IsNullOrEmpty(tbUserPassword.Text))
                {
                    bValidate.Enabled = true;
                    bGetSolutions.Enabled = true;
                }
            }

            base.OnLoad(e);
        }

        private void BValidateClick(object sender, EventArgs e)
        {
            if (tbName.Text.Length == 0)
            {
                MessageBox.Show(this, "You must define a name for this connection!", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            int serverPort = 80;
            if (tbServerPort.Text.Length > 0)
            {
                if (!int.TryParse(tbServerPort.Text, out serverPort))
                {
                    MessageBox.Show(this, "Server port must be a integer value!", "Warning", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            if (proposeToConnect && comboBoxOrganizations.Text.Length == 0 && comboBoxOrganizations.SelectedItem == null &&
                !(cbUseIfd.Checked || cbUseOSDP.Checked))
            {
                MessageBox.Show(this, "You must select an organization!", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (tbUserPassword.Text.Length == 0 && (cbUseIfd.Checked || rbAuthenticationCustom.Checked))
            {
                MessageBox.Show(this, "You must define a password!", "Warning", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (detail == null)
                detail = new ConnectionDetail();

            // Save connection details in structure
            detail.ConnectionName = tbName.Text;
            detail.IsCustomAuth = rbAuthenticationCustom.Checked;
            detail.UseSsl = cbUseSsl.Checked;
            detail.UseOnline = cbUseOnline.Checked;
            detail.UseOsdp = cbUseOSDP.Checked;
            detail.ServerName = (cbUseOSDP.Checked || cbUseOnline.Checked)
                ? cbbOnlineEnv.SelectedItem.ToString()
                : tbServerName.Text;
            detail.ServerPort = serverPort;
            detail.UserDomain = tbUserDomain.Text;
            detail.UserName = tbUserLogin.Text;
            detail.UserPassword = tbUserPassword.Text;
            detail.SavePassword = chkSavePassword.Checked;
            detail.UseIfd = cbUseIfd.Checked;
            detail.HomeRealmUrl = (tbHomeRealmUrl.Text.Length > 0 ? tbHomeRealmUrl.Text : null);

            TimeSpan timeOut;
            if (!TimeSpan.TryParse(tbTimeoutValue.Text, CultureInfo.InvariantCulture, out timeOut))
            {
                MessageBox.Show(this, "Wrong timeout value!\r\n\r\nExpected format : HH:mm:ss", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            detail.Timeout = timeOut;

            OrganizationDetail selectedOrganization = comboBoxOrganizations.SelectedItem != null
                ? ((Organization)comboBoxOrganizations.SelectedItem).OrganizationDetail
                : null;
            if (selectedOrganization != null)
            {
                detail.OrganizationServiceUrl = selectedOrganization.Endpoints[EndpointType.OrganizationService];
                detail.WebApplicationUrl = selectedOrganization.Endpoints[EndpointType.WebApplication];
                detail.Organization = selectedOrganization.UniqueName;
                detail.OrganizationUrlName = selectedOrganization.UrlName;
                detail.OrganizationFriendlyName = selectedOrganization.FriendlyName;
                detail.OrganizationVersion = selectedOrganization.OrganizationVersion;
                detail.OrganizationId = selectedOrganization.OrganizationId.ToString();
            }


            SolutionDetail selectedSolution = comboBoxSolutions.SelectedItem != null
                ? ((Solution)comboBoxSolutions.SelectedItem).SolutionDetail
                : null;
            if (selectedSolution != null)
            {
                detail.Solution = selectedSolution.UniqueName;
                detail.SolutionFriendlyName = selectedSolution.FriendlyName;
                detail.SolutionId = selectedSolution.SolutionId.ToString();
                detail.PublisherPrefix = selectedSolution.PublisherPrefix;
            }

            try
            {
                if (proposeToConnect || isCreationMode)
                {
                    FillDetails();

                    if (proposeToConnect &&
                        MessageBox.Show(this, "Do you want to connect now to this server?", "Question",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        doConnect = true;
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception error)
            {
                if (detail.OrganizationServiceUrl != null && detail.OrganizationServiceUrl.IndexOf(detail.ServerName, StringComparison.Ordinal) < 0)
                {
                    var uri = new Uri(detail.OrganizationServiceUrl);
                    var hostName = uri.Host;

                    const string format = "The server name you provided ({0}) is not the same as the one defined in deployment manager ({1}). Please make sure that the server name defined in deployment manager is reachable from you computer.\r\n\r\nError:\r\n{2}";
                    MessageBox.Show(this, string.Format(format, detail.ServerName, hostName, error.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(this, error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ComboBoxSolutionsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSolutions.Text.Length > 0 || comboBoxSolutions.SelectedItem != null)
            {
                bValidate.Enabled = true;
            }
            else
            {
                bValidate.Enabled = false;
            }
        }

        private void ComboBoxSolutionsTextChanged(object sender, EventArgs e)
        {
            if (comboBoxSolutions.Text.Length > 0 || comboBoxSolutions.SelectedItem != null)
            {
                bValidate.Enabled = true;
            }
            else
            {
                bValidate.Enabled = false;
            }
        }

        private void ComboBoxOrganizationsSelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxSolutions.Items.Clear();
            comboBoxSolutions.Enabled = false;
            bValidate.Enabled = false;
        }

        


        private void ComboBoxOrganizationsTextChanged(object sender, EventArgs e)
        {
            
        }

        private void CbUseIfdCheckedChanged(object sender, EventArgs e)
        {
            if (cbUseIfd.Checked)
            {
                cbUseOnline.Checked = false;
                cbUseOSDP.Checked = false;
            }

            bValidate.Enabled = cbUseIfd.Checked;

            rbAuthenticationCustom.Checked = cbUseIfd.Checked;
            rbAuthenticationIntegrated.Enabled = !cbUseIfd.Checked;
            rbAuthenticationIntegrated.Checked = !cbUseIfd.Checked;

            tbUserDomain.Enabled = cbUseIfd.Checked;
            tbUserLogin.Enabled = cbUseIfd.Checked;
            tbUserPassword.Enabled = cbUseIfd.Checked;

            tbServerName.Visible = !cbUseOnline.Checked;
            cbbOnlineEnv.Visible = cbUseOnline.Checked;
            tbHomeRealmUrl.Enabled = cbUseIfd.Checked;

            cbUseSsl.Checked = cbUseIfd.Checked;
            cbUseSsl.Enabled = !cbUseIfd.Checked;
        }

        private void CbUseOnlineCheckedChanged(object sender, EventArgs e)
        {
            if (cbUseOnline.Checked)
            {
                cbUseIfd.Checked = false;
                cbUseOSDP.Checked = true;

                rbAuthenticationCustom.Checked = true;
                rbAuthenticationIntegrated.Enabled = false;
                rbAuthenticationIntegrated.Checked = false;

                tbUserDomain.Text = string.Empty;

                tbUserDomain.Enabled = false;
                tbUserLogin.Enabled = true;
                tbUserPassword.Enabled = true;

                tbServerName.Visible = !cbUseOnline.Checked;
                cbbOnlineEnv.Visible = cbUseOnline.Checked;
                tbServerPort.Text = string.Empty;
                tbHomeRealmUrl.Text = string.Empty;

                cbUseSsl.Checked = true;
                cbUseSsl.Enabled = false;
                tbServerPort.Enabled = false;
                tbHomeRealmUrl.Enabled = false;
            }
            else
            {
                rbAuthenticationCustom.Checked = false;
                rbAuthenticationIntegrated.Enabled = true;
                rbAuthenticationIntegrated.Checked = true;

                tbUserDomain.Enabled = false;
                tbUserLogin.Enabled = false;
                tbUserPassword.Enabled = false;

                cbUseSsl.Checked = false;
                cbUseSsl.Enabled = true;
                cbUseOSDP.Checked = false;
                tbServerPort.Enabled = true;

                tbServerName.Visible = true;
                cbbOnlineEnv.Visible = false;
            }
        }

        private void CbUseOsdpCheckedChanged(object sender, EventArgs e)
        {
            if (cbUseOSDP.Checked)
            {
                cbUseIfd.Checked = false;
                cbUseOnline.Checked = true;
                //cbUseOnline.Enabled = false;

                rbAuthenticationCustom.Checked = true;
                rbAuthenticationIntegrated.Enabled = false;
                rbAuthenticationIntegrated.Checked = false;

                tbUserDomain.Text = string.Empty;

                tbUserDomain.Enabled = false;
                tbUserLogin.Enabled = true;
                tbUserPassword.Enabled = true;

                tbServerName.Visible = !cbUseOnline.Checked;
                cbbOnlineEnv.Visible = cbUseOnline.Checked;
                tbServerPort.Text = string.Empty;
                tbHomeRealmUrl.Text = string.Empty;

                cbUseSsl.Checked = true;
                cbUseSsl.Enabled = false;
                tbServerPort.Enabled = false;
                tbHomeRealmUrl.Enabled = false;
            }
            else
            {
                rbAuthenticationCustom.Checked = cbUseOnline.Checked;
                rbAuthenticationIntegrated.Enabled = !cbUseOnline.Checked;
                //cbUseOnline.Enabled = true;

                tbUserDomain.Enabled = false;
                tbUserLogin.Enabled = cbUseOnline.Checked;
                tbUserPassword.Enabled = cbUseOnline.Checked;

                cbUseSsl.Checked = cbUseOnline.Checked;
                cbUseSsl.Enabled = !cbUseOnline.Checked;
                tbServerPort.Enabled = !cbUseOnline.Checked;
            }
        }

        private void cbUseSsl_CheckedChanged(object sender, EventArgs e)
        {
            if (tbServerPort.Text == "80" || tbServerPort.Text == "443")
            {
                tbServerPort.Text = cbUseSsl.Checked ? "443" : "80";
            }
        }

        private void RbAuthenticationIntegratedCheckedChanged(object sender, EventArgs e)
        {
            if (rbAuthenticationIntegrated.Checked)
            {
                tbUserDomain.Text = string.Empty;
                tbUserLogin.Text = string.Empty;
                tbUserPassword.Text = string.Empty;
            }

            tbUserDomain.Enabled = rbAuthenticationCustom.Checked;
            tbUserLogin.Enabled = rbAuthenticationCustom.Checked;
            tbUserPassword.Enabled = rbAuthenticationCustom.Checked;
        }

        private bool FillConnectionDetails()
        {
            var warningMessage = string.Empty;
            bool goodAuthenticationData = false;
            bool goodServerData = false;

            // Check data filled by user
            if (rbAuthenticationIntegrated.Checked ||
                (
                rbAuthenticationCustom.Checked &&
                (tbUserDomain.Text.Length > 0 || cbUseIfd.Checked || cbUseOSDP.Checked) &&
                tbUserLogin.Text.Length > 0 &&
                tbUserPassword.Text.Length > 0
                )
                ||
                    (cbUseOnline.Checked && !string.IsNullOrEmpty(tbUserLogin.Text) &&
                    !string.IsNullOrEmpty(tbUserPassword.Text)))
                goodAuthenticationData = true;

            if (tbServerName.Text.Length > 0 || cbbOnlineEnv.SelectedIndex >= 0)
                goodServerData = true;

            int serverPort = 0;
            if (tbServerPort.Text.Length > 0)
            {
                if (!int.TryParse(tbServerPort.Text, out serverPort))
                {
                    MessageBox.Show(this, "Server port must be a integer value!", "Warning", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return false;
                }
            }

            if (tbUserPassword.Text.IndexOf(";") >= 0)
            {
                warningMessage += "Password cannot contains semicolon character, which is a split character for Microsoft Dynamics CRM simplified connection strings\r\n";
            }

            if (!goodServerData)
            {
                warningMessage += "Please provide server name\r\n";
            }
            if (!goodAuthenticationData)
            {
                warningMessage += "Please fill all authentication textboxes\r\n";
            }

            TimeSpan timeOut;
            if (!TimeSpan.TryParse(tbTimeoutValue.Text, CultureInfo.InvariantCulture, out timeOut))
            {
                warningMessage += "Wrong timeout value!\r\n\r\nExpected format : HH:mm:ss";
            }
            if (warningMessage.Length > 0)
            {
                MessageBox.Show(this, warningMessage, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else
            {
                // Save connection details in structure
                if (isCreationMode)
                {
                    detail = new ConnectionDetail();
                }

                detail.Timeout = timeOut;

                detail.ConnectionName = tbName.Text;
                detail.IsCustomAuth = rbAuthenticationCustom.Checked;
                detail.UseSsl = cbUseSsl.Checked;
                detail.ServerName = (cbUseOSDP.Checked || cbUseOnline.Checked) ? cbbOnlineEnv.SelectedItem.ToString() : tbServerName.Text;
                detail.ServerPort = serverPort;
                detail.UserDomain = tbUserDomain.Text;
                detail.UserName = tbUserLogin.Text;
                detail.UserPassword = tbUserPassword.Text;
                detail.UseIfd = cbUseIfd.Checked;
                detail.UseOnline = cbUseOnline.Checked;
                detail.UseOsdp = cbUseOSDP.Checked;
                detail.HomeRealmUrl = (tbHomeRealmUrl.Text.Length > 0 ? tbHomeRealmUrl.Text : null);

                detail.AuthType = AuthenticationProviderType.ActiveDirectory;
                if (cbUseIfd.Checked)
                {
                    detail.AuthType = AuthenticationProviderType.Federation;
                }
                else if (cbUseOSDP.Checked)
                {
                    detail.AuthType = AuthenticationProviderType.OnlineFederation;
                }
                else if (cbUseOnline.Checked)
                {
                    detail.AuthType = AuthenticationProviderType.LiveId;
                }
            }
            return true;
        }


        private async void BGetOrganizationsClick(object sender, EventArgs e)
        {
            if (FillConnectionDetails())
            {
                // Launch organization retrieval
                comboBoxOrganizations.Items.Clear();
                organizations = new List<OrganizationDetail>();
                Cursor = Cursors.WaitCursor;
                bGetOrganizations.Enabled = false;
                var orgs = await RetrieveOrganizationsAsync(detail);

                try
                {
                    foreach (OrganizationDetail orgDetail in orgs)
                    {
                        organizations.Add(orgDetail);

                        comboBoxOrganizations.Items.Add(new Organization { OrganizationDetail = orgDetail });
                        comboBoxOrganizations.SelectedIndex = 0;
                    }
                    if (comboBoxOrganizations.Items.Count > 0)
                    {
                        comboBoxOrganizations.Enabled = true;
                        bGetSolutions.Enabled = true;
                    }
                }
                catch(Exception ex)
                {
                    var errorMessage = CrmExceptionHelper.GetErrorMessage(ex, false);
                    MessageBox.Show(this, "An error occured while retrieving organizations: " + errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                bGetOrganizations.Enabled = true;
                Cursor = Cursors.Default;

                //var bw = new BackgroundWorker();
                //bw.DoWork += BwGetOrgsDoWork;
                //bw.RunWorkerCompleted += BwGetOrgsRunWorkerCompleted;
                //bw.RunWorkerAsync();
            }
        }


        private async void BGetSolutionsClick(object sender, EventArgs e)
        {
            if (FillConnectionDetails())
            {
                var organization = (Organization)comboBoxOrganizations.SelectedItem;
                var organizationDetail = organization.OrganizationDetail;

                detail.OrganizationId = organizationDetail.OrganizationId.ToString();
                detail.OrganizationServiceUrl = organizationDetail.Endpoints[EndpointType.OrganizationService];
                detail.Organization = organizationDetail.UniqueName;
                detail.OrganizationUrlName = organizationDetail.UrlName;
                detail.OrganizationFriendlyName = organizationDetail.FriendlyName;
                detail.OrganizationVersion = organizationDetail.OrganizationVersion;


                // Launch organization retrieval
                comboBoxSolutions.Items.Clear();
                solutions = new List<SolutionDetail>();
                Cursor = Cursors.WaitCursor;
                bGetSolutions.Enabled = false;

                try
                {
                    var solutionsResponse = await RetrieveSolutionsAsync(detail);
                    foreach (Entity entity in solutionsResponse)
                    {

                        var solutionDetail = new SolutionDetail()
                        {
                            SolutionId = entity.Id,
                            UniqueName = entity.GetAttributeValue<string>("uniquename"),
                            FriendlyName = entity.GetAttributeValue<string>("friendlyname"),
                            PublisherPrefix = entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix") == null ? null : entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix").Value.ToString()
                        };

                        solutions.Add(solutionDetail);

                        comboBoxSolutions.Items.Add(new Solution() { SolutionDetail = solutionDetail });
                        comboBoxSolutions.SelectedIndex = 0;
                    }
                    if (comboBoxSolutions.Items.Count > 0)
                    {
                        comboBoxSolutions.Enabled = true;
                    }
                } 
                catch(Exception ex)
                {
                    var errorMessage = CrmExceptionHelper.GetErrorMessage(ex, false);
                    MessageBox.Show(this, "An error occured while retrieving solutions: " + errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                bGetSolutions.Enabled = true;
                Cursor = Cursors.Default;

                //var bw = new BackgroundWorker();
                //bw.DoWork += BwGetSolutionsDoWork;
                //bw.RunWorkerCompleted += BwGetSolutionsRunWorkerCompleted;
                //bw.RunWorkerAsync();
            }
        }

        void BwGetOrgsDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = RetrieveOrganizations(detail);
        }

        void BwGetOrgsRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                string errorMessage = e.Error.Message;
                var ex = e.Error.InnerException;
                while (ex != null)
                {
                    errorMessage += "\r\nInner Exception: " + ex.Message;
                    ex = ex.InnerException;
                }

                MessageBox.Show(this, "An error occured while retrieving organizations: " + errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                foreach (OrganizationDetail orgDetail in (OrganizationDetailCollection)e.Result)
                {
                    organizations.Add(orgDetail);

                    comboBoxOrganizations.Items.Add(new Organization { OrganizationDetail = orgDetail });
                    comboBoxOrganizations.SelectedIndex = 0;
                }
                if(comboBoxOrganizations.Items.Count > 0)
                {
                    comboBoxOrganizations.Enabled = true;
                    bGetSolutions.Enabled = true;
                }
            }

            Cursor = Cursors.Default;
        }

        private async Task<OrganizationDetailCollection> RetrieveOrganizationsAsync(ConnectionDetail currentDetail)
        {
            WebRequest.GetSystemWebProxy();
            var service = await CrmConnectionHelper.GetDiscoveryServiceAsync(currentDetail);

            var request = new RetrieveOrganizationsRequest();
            var response = (RetrieveOrganizationsResponse)service.Execute(request);
            return response.Details;
        }
        private OrganizationDetailCollection RetrieveOrganizations(ConnectionDetail currentDetail)
        {
            WebRequest.GetSystemWebProxy();
            var service = CrmConnectionHelper.GetDiscoveryService(currentDetail);

            var request = new RetrieveOrganizationsRequest();
            var response = (RetrieveOrganizationsResponse)service.Execute(request);
            return response.Details;
        }

        void BwGetSolutionsDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = RetrieveSolutions(detail);
        }

        void BwGetSolutionsRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                string errorMessage = e.Error.Message;
                var ex = e.Error.InnerException;
                while (ex != null)
                {
                    errorMessage += "\r\nInner Exception: " + ex.Message;
                    ex = ex.InnerException;
                }

                MessageBox.Show(this, "An error occured while retrieving solutions: " + errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                foreach (Entity entity in (DataCollection<Entity>)e.Result)
                {

                    var solutionDetail = new SolutionDetail()
                    {
                        SolutionId = entity.Id,
                        UniqueName = entity.GetAttributeValue<string>("uniquename"),
                        FriendlyName = entity.GetAttributeValue<string>("friendlyname"),
                        PublisherPrefix = entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix") == null ? null : entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix").Value.ToString()
                    };

                    solutions.Add(solutionDetail);

                    comboBoxSolutions.Items.Add(new Solution() { SolutionDetail = solutionDetail });
                    comboBoxSolutions.SelectedIndex = 0;
                }
                if (comboBoxSolutions.Items.Count > 0)
                {
                    comboBoxSolutions.Enabled = true;
                }
            }

            Cursor = Cursors.Default;
        }

        private DataCollection<Entity> RetrieveSolutions(ConnectionDetail currentDetail)
        {
            WebRequest.GetSystemWebProxy();
            var service = CrmConnectionHelper.GetOrganizationServiceProxy(currentDetail);

            QueryExpression query = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(new string[] { "friendlyname", "uniquename", "publisherid" }),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("isvisible", ConditionOperator.Equal, true);

            query.LinkEntities.Add(new LinkEntity("solution", "publisher", "publisherid", "publisherid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("customizationprefix");
            query.LinkEntities[0].EntityAlias = "publisher";

            var response = service.RetrieveMultiple(query);
            return response.Entities;
        }

        private async Task<DataCollection<Entity>> RetrieveSolutionsAsync(ConnectionDetail currentDetail)
        {
            WebRequest.GetSystemWebProxy();
            var service = await CrmConnectionHelper.GetOrganizationServiceProxyAsync(currentDetail);

            QueryExpression query = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(new string[] { "friendlyname", "uniquename", "publisherid" }),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("isvisible", ConditionOperator.Equal, true);

            query.LinkEntities.Add(new LinkEntity("solution", "publisher", "publisherid", "publisherid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("customizationprefix");
            query.LinkEntities[0].EntityAlias = "publisher";

            var response = service.RetrieveMultiple(query);
            return response.Entities;
        }



        /// <summary>
        /// Remplit les contrôles du formulaire avec les données
        /// du détail de connexion courant
        /// </summary>
        private void FillValues()
        {
            rbAuthenticationCustom.Checked = detail.IsCustomAuth;
            rbAuthenticationIntegrated.Checked = !detail.IsCustomAuth;

            //rbAuthenticationIntegrated.CheckedChanged += new EventHandler(rbAuthenticationIntegrated_CheckedChanged);

            tbName.Text = detail.ConnectionName;
            tbServerPort.Text = detail.ServerPort.ToString(CultureInfo.InvariantCulture);
            tbUserDomain.Text = detail.UserDomain;
            tbUserLogin.Text = detail.UserName;
            tbUserPassword.Text = detail.UserPassword;
            chkSavePassword.Checked = detail.SavePassword;
            cbUseIfd.Checked = detail.UseIfd;
            cbUseOSDP.Checked = detail.UseOsdp;
            cbUseOnline.Checked = detail.UseOnline;
            cbUseSsl.Checked = detail.UseSsl;
            tbHomeRealmUrl.Text = detail.HomeRealmUrl;
            tbTimeoutValue.Text = detail.Timeout.ToString(@"hh\:mm\:ss");

            if (rbAuthenticationIntegrated.Checked || !String.IsNullOrEmpty(tbUserPassword.Text)) {
                FillOrgSolValuesAsync();
                GetSelectedOrganizationAsync();
                GetSelectedSolutionAsync();
                //GetSelectedOrganization();
                //GetSelectedSolution();
            }
            

            tbHomeRealmUrl.Enabled = detail.UseIfd;

            if (detail.UseOnline || detail.UseOsdp)
            {
                tbServerName.Visible = false;
                cbbOnlineEnv.Visible = true;

                cbbOnlineEnv.SelectedItem = detail.ServerName;
            }
            else
            {
                tbServerName.Visible = true;
                cbbOnlineEnv.Visible = false;

                tbServerName.Text = detail.ServerName;
            }

            cbUseSsl_CheckedChanged(null, null);
        }
        private async Task FillOrgSolValuesAsync()
        {
            Cursor = Cursors.WaitCursor;
            await GetSelectedOrganizationAsync();
            await GetSelectedSolutionAsync();
            Cursor = Cursors.Default;
        }
        private async Task GetSelectedOrganizationAsync()
        {
            comboBoxOrganizations.Items.Clear();
            organizations = new List<OrganizationDetail>();

            try
            {
                var orgDetail = await RetrieveOrganizationAsync(detail);
                organizations.Add(orgDetail);
                comboBoxOrganizations.Items.Add(new Organization { OrganizationDetail = orgDetail });
                comboBoxOrganizations.SelectedIndex = 0;
            } 
            catch(Exception ex)
            {
                var errorMessage = CrmExceptionHelper.GetErrorMessage(ex, false);
                MessageBox.Show(this, "An error occured while retrieving organizations: " + errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetSelectedOrganization()
        {

            comboBoxOrganizations.Items.Clear();
            organizations = new List<OrganizationDetail>();
            Cursor = Cursors.WaitCursor;

            var bw = new BackgroundWorker();
            bw.DoWork += BwGetOrgDoWork;
            bw.RunWorkerCompleted += BwGetOrgRunWorkerCompleted;
            bw.RunWorkerAsync();


        }

        private void BwGetOrgRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                string errorMessage = e.Error.Message;
                var ex = e.Error.InnerException;
                while (ex != null)
                {
                    errorMessage += "\r\nInner Exception: " + ex.Message;
                    ex = ex.InnerException;
                }

                MessageBox.Show(this, "An error occured while retrieving organizations: " + errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    var orgDetail = e.Result as OrganizationDetail;
                    organizations.Add(orgDetail);
                    comboBoxOrganizations.Items.Add(new Organization { OrganizationDetail = orgDetail });
                    comboBoxOrganizations.SelectedIndex = 0;
                }
            }

            Cursor = Cursors.Default;
        }

        private void BwGetOrgDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = RetrieveOrganization(detail);
        }

        private object RetrieveOrganization(ConnectionDetail currentDetail)
        {
            if (currentDetail.OrganizationId == null)
            {
                return null;
            }
            WebRequest.GetSystemWebProxy();

            var service = CrmConnectionHelper.GetDiscoveryService(currentDetail);

            var request = new RetrieveOrganizationsRequest();
            var response = (RetrieveOrganizationsResponse)service.Execute(request);


            return response.Details.Where(d => d.OrganizationId == new Guid(currentDetail.OrganizationId)).FirstOrDefault();
        }

        private async Task<OrganizationDetail> RetrieveOrganizationAsync(ConnectionDetail currentDetail)
        {
            if (currentDetail.OrganizationId == null)
            {
                return null;
            }
            WebRequest.GetSystemWebProxy();

            var service = await CrmConnectionHelper.GetDiscoveryServiceAsync(currentDetail);

            var request = new RetrieveOrganizationsRequest();
            var response = (RetrieveOrganizationsResponse)service.Execute(request);


            return response.Details.Where(d => d.OrganizationId == new Guid(currentDetail.OrganizationId)).FirstOrDefault();
        }

        private async Task GetSelectedSolutionAsync()
        {
            comboBoxSolutions.Items.Clear();
            solutions = new List<SolutionDetail>();

            try
            {
                var entity = await RetrieveSolutionAsync(detail);
                var solDetail = new SolutionDetail()
                {
                    SolutionId = entity.Id,
                    UniqueName = entity.GetAttributeValue<string>("uniquename"),
                    FriendlyName = entity.GetAttributeValue<string>("friendlyname"),
                    PublisherPrefix = entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix") == null ? null : entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix").Value.ToString()
                };

                if (solutions == null)
                {
                    solutions = new List<SolutionDetail>();
                }

                solutions.Add(solDetail);
                comboBoxSolutions.Items.Add(new Solution { SolutionDetail = solDetail });
                comboBoxSolutions.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                var errorMessage = CrmExceptionHelper.GetErrorMessage(ex, false);
                MessageBox.Show(this, "An error occured while retrieving solutions: " + errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void GetSelectedSolution()
        {

            comboBoxOrganizations.Items.Clear();
            organizations = new List<OrganizationDetail>();
            Cursor = Cursors.WaitCursor;

            var bwSolution = new BackgroundWorker();
            bwSolution.DoWork += BwGetSolutionDoWork;
            bwSolution.RunWorkerCompleted += BwGetSolutionRunWorkerCompleted;
            bwSolution.RunWorkerAsync();
        }

        private void BwGetSolutionRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                string errorMessage = e.Error.Message;
                var ex = e.Error.InnerException;
                while (ex != null)
                {
                    errorMessage += "\r\nInner Exception: " + ex.Message;
                    ex = ex.InnerException;
                }

                MessageBox.Show(this, "An error occured while retrieving organizations: " + errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    var entity = e.Result as Entity;
                    var solDetail = new SolutionDetail()
                    {
                        SolutionId = entity.Id,
                        UniqueName = entity.GetAttributeValue<string>("uniquename"),
                        FriendlyName = entity.GetAttributeValue<string>("friendlyname"),
                        PublisherPrefix = entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix") == null ? null : entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix").Value.ToString()
                    };

                    if (solutions == null)
                    {
                        solutions = new List<SolutionDetail>();
                    }

                    solutions.Add(solDetail);
                    comboBoxSolutions.Items.Add(new Solution { SolutionDetail = solDetail });
                    comboBoxSolutions.SelectedIndex = 0;
                }
            }

            Cursor = Cursors.Default;
        }

        private void BwGetSolutionDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = RetrieveSolution(detail);
        }

        private Entity RetrieveSolution(ConnectionDetail currentDetail)
        {
            if (currentDetail.SolutionId == null)
            {
                return null;
            }
            WebRequest.GetSystemWebProxy();

            var service = CrmConnectionHelper.GetOrganizationServiceProxy(currentDetail);

            QueryExpression query = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(new string[] { "friendlyname", "uniquename", "publisherid" }),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("isvisible", ConditionOperator.Equal, true);
            query.Criteria.AddCondition("solutionid", ConditionOperator.Equal, currentDetail.SolutionId);

            query.LinkEntities.Add(new LinkEntity("solution", "publisher", "publisherid", "publisherid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("customizationprefix");
            query.LinkEntities[0].EntityAlias = "publisher";

            var response = service.RetrieveMultiple(query);
            return response.Entities.FirstOrDefault();
        }

        private async Task<Entity> RetrieveSolutionAsync(ConnectionDetail currentDetail)
        {
            if (currentDetail.SolutionId == null)
            {
                return null;
            }
            WebRequest.GetSystemWebProxy();

            var service = await CrmConnectionHelper.GetOrganizationServiceProxyAsync(currentDetail);

            QueryExpression query = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(new string[] { "friendlyname", "uniquename", "publisherid" }),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("isvisible", ConditionOperator.Equal, true);
            query.Criteria.AddCondition("solutionid", ConditionOperator.Equal, currentDetail.SolutionId);

            query.LinkEntities.Add(new LinkEntity("solution", "publisher", "publisherid", "publisherid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("customizationprefix");
            query.LinkEntities[0].EntityAlias = "publisher";

            var response = service.RetrieveMultiple(query);
            return response.Entities.FirstOrDefault();
        }

        /// <summary>
        /// Remplit le détail de connexion avec le contenu 
        /// des contrôles du formulaire
        /// </summary>
        /// <returns></returns>
        private void FillDetails()
        {
            bool hasFoundOrg = false;

            OrganizationDetail selectedOrganization = comboBoxOrganizations.SelectedItem != null ? ((Organization)comboBoxOrganizations.SelectedItem).OrganizationDetail : null;

            if (organizations == null || organizations.Count == 0)
            {
                var orgs = RetrieveOrganizations(detail);
                foreach (OrganizationDetail orgDetail in orgs)
                {
                    if (organizations == null)
                        organizations = new List<OrganizationDetail>();

                    organizations.Add(orgDetail);

                    comboBoxOrganizations.Items.Add(new Organization { OrganizationDetail = orgDetail });
                    comboBoxOrganizations.SelectedIndex = 0;
                    comboBoxOrganizations.Enabled = true;
                }
            }

            if (organizations == null)
            {
                MessageBox.Show(this, "Organizations list is empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (OrganizationDetail organization in organizations)
            {
                if (organization.UniqueName == selectedOrganization.UniqueName)
                {
                    detail.OrganizationServiceUrl = organization.Endpoints[EndpointType.OrganizationService];
                    detail.Organization = organization.UniqueName;
                    detail.OrganizationUrlName = organization.UrlName;
                    detail.OrganizationFriendlyName = organization.FriendlyName;
                    detail.OrganizationVersion = organization.OrganizationVersion;
                    detail.OrganizationId = organization.OrganizationId.ToString();

                    detail.ConnectionName = tbName.Text;

                    if (isCreationMode)
                    {
                        detail.ConnectionId = Guid.NewGuid();
                    }

                    hasFoundOrg = true;

                    break;
                }
            }

            if (!hasFoundOrg)
            {
                throw new Exception("Unable to match selected organization with list of organizations in this deployment");
            }
        }


        #endregion


    }
}