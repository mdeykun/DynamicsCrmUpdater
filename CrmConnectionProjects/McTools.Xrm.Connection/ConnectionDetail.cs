using CrmWebResourcesUpdater.DataModels;
using McTools.Xrm.Connection.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace McTools.Xrm.Connection
{
    public enum SensitiveDataNotFoundReason
    {
        NotAllowedByUser,
        NotAccessible,
        None
    }

    public class CertificateInfo
    {
        public string Issuer { get; set; }
        public string Name { get; set; }
        public string Thumbprint { get; set; }
    }

    /// <summary>
    /// Stores data regarding a specific connection to Crm server
    /// </summary>
    [XmlInclude(typeof(CertificateInfo))]
    [XmlInclude(typeof(EnvironmentHighlighting))]
    public class ConnectionDetail : IComparable, ICloneable
    {
        private string clientSecret;
        private Guid impersonatedUserId;
        private string impersonatedUserName;
        private string userPassword;

        #region Constructeur

        public ConnectionDetail()
        {
        }

        public ConnectionDetail(bool createNewId = false)
        {
            if (createNewId)
            {
                ConnectionId = Guid.NewGuid();
            }
        }

        #endregion Constructeur

        #region Propriétés

        [XmlIgnore] [IgnoreDataMember]
        public bool AllowPasswordSharing { get; set; }

        public AuthenticationProviderType AuthType { get; set; }
        public Guid AzureAdAppId { get; set; }

        [XmlIgnore] [IgnoreDataMember]
        public bool CanImpersonate { get; set; }

        [XmlElement("CertificateInfo")]
        public CertificateInfo Certificate { get; set; }

        [XmlElement("ClientSecret")]
        public string ClientSecretEncrypted
        {
            get => clientSecret;
            set => clientSecret = value;
        }

        [XmlIgnore] [IgnoreDataMember]
        public bool ClientSecretIsEmpty => string.IsNullOrEmpty(clientSecret);

        /// <summary>
        /// Gets or sets the connection unique identifier
        /// </summary>
        public Guid? ConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection
        /// </summary>
        public string ConnectionName { get; set; }

        public string ConnectionString { get; set; }

        [XmlIgnore] [IgnoreDataMember]
        public Color? EnvironmentColor { get; set; }

        ///// <summary>
        ///// Gets or sets custom information for use by consuming application
        ///// </summary>
        //public Dictionary<string, string> CustomInformation { get; set; }
        public EnvironmentHighlighting EnvironmentHighlightingInfo { get; set; }

        public string EnvironmentId { get; set; }

        [XmlIgnore] [IgnoreDataMember]
        public string EnvironmentText { get; set; }

        [XmlIgnore] [IgnoreDataMember]
        public Color? EnvironmentTextColor { get; set; }

        /// <summary>
        /// Gets or sets the Home realm url for ADFS authentication
        /// </summary>
        public string HomeRealmUrl { get; set; }

        [XmlIgnore] [IgnoreDataMember]
        public Guid ImpersonatedUserId {
            get => impersonatedUserId;
            set => impersonatedUserId = value;
        }

        [XmlIgnore] [IgnoreDataMember]
        public string ImpersonatedUserName {
            get => impersonatedUserName;
            set => impersonatedUserName = value;
        }

        /// <summary>
        /// Get or set flag to know if custom authentication
        /// </summary>
        public bool IsCustomAuth { get; set; }

        [XmlIgnore] [IgnoreDataMember] public bool IsEnvironmentHighlightSet => EnvironmentHighlightingInfo != null;
        public bool IsFromSdkLoginCtrl { get; set; }

        [XmlIgnore] [IgnoreDataMember]
        public DateTime LastUsedOn { get; set; }

        [XmlElement("LastUsedOn")]
        public string LastUsedOnString
        {
            get => LastUsedOn.ToString("yyyy-MM-dd HH:mm:ss");
            set
            {
                //if (DateTime.TryParse(value, CultureInfo.CurrentUICulture, DateTimeStyles.AssumeLocal, out DateTime parsed))
                if (DateTime.TryParseExact(value, "MM/dd/yyyy HH:mm:ss", CultureInfo.CurrentUICulture, DateTimeStyles.AssumeLocal, out DateTime parsed))
                {
                    LastUsedOn = parsed;
                }
                else
                {
                    LastUsedOn = DateTime.Parse(value);
                }
            }
        }

        public AuthenticationType NewAuthType { get; set; }

        /// <summary>
        /// Get or set the organization name
        /// </summary>
        public string Organization { get; set; }

        public string OrganizationDataServiceUrl { get; set; }

        /// <summary>
        /// Get or set the organization friendly name
        /// </summary>
        public string OrganizationFriendlyName { get; set; }

        public int OrganizationMajorVersion => OrganizationVersion != null ? int.Parse(OrganizationVersion.Split('.')[0]) : -1;
        public int OrganizationMinorVersion => OrganizationVersion != null ? int.Parse(OrganizationVersion.Split('.')[1]) : -1;

        /// <summary>
        /// Gets or sets the Crm Service Url
        /// </summary>
        public string OrganizationServiceUrl { get; set; }

        /// <summary>
        /// Get or set the organization name
        /// </summary>
        public string OrganizationUrlName { get; set; }

        public string OrganizationVersion { get; set; }
        public string OriginalUrl { get; set; }

        /// <summary>
        /// Gets an information if the password is empty
        /// </summary>
        public bool PasswordIsEmpty => string.IsNullOrEmpty(userPassword);

        /// <summary>
        /// OAuth Refresh Token
        /// </summary>
        public string RefreshToken { get; set; }

        public string ReplyUrl { get; set; }

        /// <summary>
        /// Client Secret used for S2S Auth
        /// </summary>
        public string S2SClientSecret
        {
            get => clientSecret;
            set => clientSecret = value;
        }

        /// <summary>
        /// Gets or sets the information if the password must be saved
        /// </summary>
        public bool SavePassword { get; set; }

        /// <summary>
        /// Get or set the server name
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Get or set the server port
        /// </summary>
        [DefaultValue(80)]
        [XmlIgnore] [IgnoreDataMember]
        public int? ServerPort { get; set; }

        [XmlElement("ServerPort")]
        public string ServerPortString
        {
            get => ServerPort.ToString();
            set => ServerPort = string.IsNullOrEmpty(value) ? 80 : int.Parse(value);
        }

        public Guid TenantId { get; set; }
        public TimeSpan Timeout { get; set; }

        public long TimeoutTicks
        {
            get { return Timeout.Ticks; }
            set { Timeout = new TimeSpan(value); }
        }

        [XmlIgnore] [IgnoreDataMember]
        public bool UseConnectionString => !string.IsNullOrEmpty(ConnectionString);

        /// <summary>
        /// Get or set flag to know if we use IFD
        /// </summary>
        public bool UseIfd { get; set; }

        /// <summary>
        /// Get or set flag to know if we use Multi Factor Authentication
        /// </summary>
        public bool UseMfa { get; set; }

        /// <summary>
        /// Get or set flag to know if we use CRM Online
        /// </summary>
        [XmlIgnore] [IgnoreDataMember]
        public bool UseOnline => OriginalUrl.IndexOf(".dynamics.com", StringComparison.InvariantCultureIgnoreCase) > 0;

        /// <summary>
        /// Get or set the user domain name
        /// </summary>
        public string UserDomain { get; set; }

        /// <summary>
        /// Get or set flag to know if we use Online Services
        /// </summary>
        //public bool UseOsdp { get; set; }
        /// <summary>
        /// Get or set user login
        /// </summary>
        public string UserName { get; set; }

        [XmlElement("UserPassword")]
        public string UserPasswordEncrypted
        {
            get => userPassword;
            set => userPassword = value;
        }

        /// <summary>
        /// Get or set the use of SSL connection
        /// </summary>
        [XmlIgnore] [IgnoreDataMember]
        public bool UseSsl => WebApplicationUrl?.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) ?? OriginalUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase);

        public string WebApplicationUrl { get; set; }

        public List<SolutionDetail> Solutions { get; set; }
        
        public SolutionDetail SelectedSolution { get; set; }
        public string SolutionName { get => SelectedSolution == null ? "" : SelectedSolution.FriendlyName; }

        #endregion Propriétés

        #region Méthodes

        public void ErasePassword()
        {
            userPassword = null;
            clientSecret = null;
        }

        public void SetClientSecret(string secret, bool isEncrypted = false)
        {
            if (!string.IsNullOrEmpty(secret))
            {
                if (isEncrypted)
                {
                    clientSecret = secret;
                }
                else
                {
                    clientSecret = CryptoManager.Encrypt(secret, EncriptionSettings.CryptoPassPhrase,
                        EncriptionSettings.CryptoSaltValue,
                        EncriptionSettings.CryptoHashAlgorythm,
                        EncriptionSettings.CryptoPasswordIterations,
                        EncriptionSettings.CryptoInitVector,
                        EncriptionSettings.CryptoKeySize);
                }
            }
        }

        public string GetClientSecret(bool isEncrypted = false)
        {
            return clientSecret;
        }
        public string GetUserPassword(bool isEncrypted = false)
        {
            return userPassword;
        }
        

        public void SetConnectionString(string connectionString)
        {
            var csb = new DbConnectionStringBuilder { ConnectionString = connectionString };

            OriginalUrl = (csb.ContainsKey("ServiceUri") ? csb["ServiceUri"] :
                csb.ContainsKey("Service Uri") ? csb["Service Uri"] :
                csb.ContainsKey("Url") ? csb["Url"] :
                csb.ContainsKey("Server") ? csb["Server"] : "").ToString();


            if (csb.ContainsKey("Password"))
            {
                csb["Password"] = CryptoManager.Encrypt(csb["Password"].ToString(), EncriptionSettings.CryptoPassPhrase,
                    EncriptionSettings.CryptoSaltValue,
                    EncriptionSettings.CryptoHashAlgorythm,
                    EncriptionSettings.CryptoPasswordIterations,
                    EncriptionSettings.CryptoInitVector,
                    EncriptionSettings.CryptoKeySize);
            }
            if (csb.ContainsKey("ClientSecret"))
            {
                csb["ClientSecret"] = CryptoManager.Encrypt(csb["ClientSecret"].ToString(), EncriptionSettings.CryptoPassPhrase,
                    EncriptionSettings.CryptoSaltValue,
                    EncriptionSettings.CryptoHashAlgorythm,
                    EncriptionSettings.CryptoPasswordIterations,
                    EncriptionSettings.CryptoInitVector,
                    EncriptionSettings.CryptoKeySize);
            }

            ConnectionString = csb.ToString();
        }

        public void SetPassword(string password, bool isEncrypted = false)
        {
            if (!string.IsNullOrEmpty(password))
            {
                if (isEncrypted)
                {
                    userPassword = password;
                }
                else
                {
                    userPassword = CryptoManager.Encrypt(password, EncriptionSettings.CryptoPassPhrase,
                        EncriptionSettings.CryptoSaltValue,
                        EncriptionSettings.CryptoHashAlgorythm,
                        EncriptionSettings.CryptoPasswordIterations,
                        EncriptionSettings.CryptoInitVector,
                        EncriptionSettings.CryptoKeySize);
                }
            }
        }

        public string GetPassword(string password)
        {
            return CryptoManager.Decrypt(password, EncriptionSettings.CryptoPassPhrase,
                    EncriptionSettings.CryptoSaltValue,
                    EncriptionSettings.CryptoHashAlgorythm,
                    EncriptionSettings.CryptoPasswordIterations,
                    EncriptionSettings.CryptoInitVector,
                    EncriptionSettings.CryptoKeySize);
        }

        /// <summary>
        /// Retourne le nom de la connexion
        /// </summary>
        /// <returns>Nom de la connexion</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(ConnectionName) ? "Connection name not set" : ConnectionName;
        }

        public bool TryRequestClientSecret(Control parent, string secretUsageDescription, out string secret, out SensitiveDataNotFoundReason notFoundReason)
        {
            var prd = new PasswordRequestDialog(secretUsageDescription, this, "client secret");
            if (AllowPasswordSharing || prd.ShowDialog(parent) == DialogResult.OK && prd.Accepted)
            {
                if (string.IsNullOrEmpty(clientSecret))
                {
                    secret = string.Empty;
                    notFoundReason = SensitiveDataNotFoundReason.NotAccessible;
                    return false;
                }

                secret = CryptoManager.Decrypt(clientSecret, EncriptionSettings.CryptoPassPhrase,
                    EncriptionSettings.CryptoSaltValue,
                    EncriptionSettings.CryptoHashAlgorythm,
                    EncriptionSettings.CryptoPasswordIterations,
                    EncriptionSettings.CryptoInitVector,
                    EncriptionSettings.CryptoKeySize);

                notFoundReason = SensitiveDataNotFoundReason.None;
                return true;
            }

            notFoundReason = SensitiveDataNotFoundReason.NotAllowedByUser;
            secret = string.Empty;
            return false;
        }

        public bool TryRequestPassword(Control parent, string passwordUsageDescription, out string password, out SensitiveDataNotFoundReason notFoundReason)
        {
            var prd = new PasswordRequestDialog(passwordUsageDescription, this, "password");
            if (AllowPasswordSharing || prd.ShowDialog(parent) == DialogResult.OK && prd.Accepted)
            {
                if (string.IsNullOrEmpty(userPassword))
                {
                    password = string.Empty;
                    notFoundReason = SensitiveDataNotFoundReason.NotAccessible;
                    return false;
                }

                password = CryptoManager.Decrypt(userPassword, EncriptionSettings.CryptoPassPhrase,
                    EncriptionSettings.CryptoSaltValue,
                    EncriptionSettings.CryptoHashAlgorythm,
                    EncriptionSettings.CryptoPasswordIterations,
                    EncriptionSettings.CryptoInitVector,
                    EncriptionSettings.CryptoKeySize);

                notFoundReason = SensitiveDataNotFoundReason.None;
                return true;
            }

            notFoundReason = SensitiveDataNotFoundReason.NotAllowedByUser;
            password = string.Empty;
            return false;
        }

        public void UpdateAfterEdit(ConnectionDetail editedConnection)
        {
            ConnectionName = editedConnection.ConnectionName;
            ConnectionString = editedConnection.ConnectionString;
            OrganizationServiceUrl = editedConnection.OrganizationServiceUrl;
            OrganizationDataServiceUrl = editedConnection.OrganizationDataServiceUrl;
            Organization = editedConnection.Organization;
            OrganizationFriendlyName = editedConnection.OrganizationFriendlyName;
            ServerName = editedConnection.ServerName;
            ServerPort = editedConnection.ServerPort;
            UseIfd = editedConnection.UseIfd;
            UserDomain = editedConnection.UserDomain;
            UserName = editedConnection.UserName;
            userPassword = editedConnection.userPassword;
            HomeRealmUrl = editedConnection.HomeRealmUrl;
            Timeout = editedConnection.Timeout;
            UseMfa = editedConnection.UseMfa;
            ReplyUrl = editedConnection.ReplyUrl;
            AzureAdAppId = editedConnection.AzureAdAppId;
            clientSecret = editedConnection.clientSecret;
            RefreshToken = editedConnection.RefreshToken;
            EnvironmentText = editedConnection.EnvironmentText;
            EnvironmentColor = editedConnection.EnvironmentColor;
            EnvironmentTextColor = editedConnection.EnvironmentTextColor;
        }

        

        

        #endregion Méthodes

        public object Clone()
        {
            var cd = new ConnectionDetail
            {
                AuthType = AuthType,
                ConnectionId = Guid.NewGuid(),
                ConnectionName = ConnectionName,
                ConnectionString = ConnectionString,
                HomeRealmUrl = HomeRealmUrl,
                Organization = Organization,
                OrganizationFriendlyName = OrganizationFriendlyName,
                OrganizationServiceUrl = OrganizationServiceUrl,
                OrganizationDataServiceUrl = OrganizationDataServiceUrl,
                OrganizationUrlName = OrganizationUrlName,
                OrganizationVersion = OrganizationVersion,
                SavePassword = SavePassword,
                ServerName = ServerName,
                ServerPort = ServerPort,
                TimeoutTicks = TimeoutTicks,
                UseIfd = UseIfd,
                UserDomain = UserDomain,
                UserName = UserName,
                userPassword = userPassword,
                WebApplicationUrl = WebApplicationUrl,
                OriginalUrl = OriginalUrl,
                Timeout = Timeout,
                UseMfa = UseMfa,
                AzureAdAppId = AzureAdAppId,
                ReplyUrl = ReplyUrl,
                EnvironmentText = EnvironmentText,
                EnvironmentColor = EnvironmentColor,
                EnvironmentTextColor = EnvironmentTextColor,
                RefreshToken = RefreshToken,
                S2SClientSecret = S2SClientSecret,
                IsFromSdkLoginCtrl = IsFromSdkLoginCtrl,
            };

            if (Certificate != null)
            {
                cd.Certificate = new CertificateInfo
                {
                    Issuer = Certificate.Issuer,
                    Thumbprint = Certificate.Thumbprint,
                    Name = Certificate.Name
                };
            }

            return cd;
        }

        public void CopyClientSecretTo(ConnectionDetail detail)
        {
            detail.clientSecret = clientSecret;
        }

        public void CopyPasswordTo(ConnectionDetail detail)
        {
            detail.userPassword = userPassword;
        }

        public string GetConnectionString()
        {
            var csb = new DbConnectionStringBuilder();

            switch (AuthType)
            {
                default:
                    csb["AuthType"] = "AD";
                    break;

                case AuthenticationProviderType.OnlineFederation:
                    csb["AuthType"] = "Office365";
                    break;

                case AuthenticationProviderType.Federation:
                    csb["AuthType"] = "IFD";
                    break;
            }

            csb["Url"] = WebApplicationUrl;

            if (Certificate != null)
            {
                csb["AuthType"] = "Certificate";
                csb["ClientId"] = AzureAdAppId;
                csb["Thumbprint"] = Certificate.Thumbprint;

                return csb.ToString();
            }

            if (!string.IsNullOrEmpty(clientSecret))
            {
                csb["AuthType"] = "ClientSecret";
                csb["ClientId"] = AzureAdAppId.ToString("B");
                csb["ClientSecret"] = "*************";

                return csb.ToString();
            }

            if (UseMfa)
            {
                csb["Username"] = UserName;
                csb["AuthType"] = "OAuth";
                csb["ClientId"] = AzureAdAppId.ToString("B");
                csb["LoginPrompt"] = "Auto";
                csb["RedirectUri"] = ReplyUrl;
                csb["TokenCacheStorePath"] = Path.Combine(Path.GetTempPath(), ConnectionId.Value.ToString("B"), "oauth-cache.txt");
                return csb.ToString();
            }

            if (!string.IsNullOrEmpty(UserDomain))
            {
                csb["Domain"] = UserDomain;
                csb["Username"] = UserName;
                csb["Password"] = "********";
            }

            if (!string.IsNullOrEmpty(HomeRealmUrl))
            {
                csb["HomeRealmUri"] = HomeRealmUrl;
            }

            return csb.ToString();
        }

        public bool IsConnectionBrokenWithUpdatedData(ConnectionDetail originalDetail)
        {
            if (originalDetail == null)
            {
                return true;
            }

            if (originalDetail.HomeRealmUrl != HomeRealmUrl
                || originalDetail.IsCustomAuth != IsCustomAuth
                || originalDetail.Organization != Organization
                || originalDetail.OrganizationUrlName != OrganizationUrlName
                || originalDetail.ServerName.ToLower() != ServerName.ToLower()
                || originalDetail.ServerPort != ServerPort
                || originalDetail.UseIfd != UseIfd
                || originalDetail.UseOnline != UseOnline
                || originalDetail.UseSsl != UseSsl
                || originalDetail.UseMfa != UseMfa
                || originalDetail.clientSecret != clientSecret
                || originalDetail.AzureAdAppId != AzureAdAppId
                || originalDetail.ReplyUrl != ReplyUrl
                || originalDetail.UserDomain?.ToLower() != UserDomain?.ToLower()
                || originalDetail.UserName?.ToLower() != UserName?.ToLower()
                || SavePassword && !string.IsNullOrEmpty(userPassword) && originalDetail.userPassword != userPassword
                || SavePassword && !string.IsNullOrEmpty(clientSecret) && originalDetail.clientSecret != clientSecret
                || originalDetail.Certificate.Thumbprint != Certificate.Thumbprint)
            {
                return true;
            }

            return false;
        }

        public bool PasswordIsDifferent(string password)
        {
            return password != userPassword;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            var detail = (ConnectionDetail)obj;

            return String.Compare(ConnectionName, detail.ConnectionName, StringComparison.Ordinal);
        }

        #endregion IComparable Members
    }

    public class EnvironmentHighlighting
    {
        [XmlIgnore] [IgnoreDataMember]
        public Color? Color { get; set; }

        [XmlElement("Color")]
        public string ColorString
        {
            get => ColorTranslator.ToHtml(Color ?? System.Drawing.Color.Black);
            set => Color = ColorTranslator.FromHtml(value);
        }

        public string Text { get; set; }

        [XmlIgnore] [IgnoreDataMember]
        public Color? TextColor { get; set; }

        [XmlElement("TextColor")]
        public string TextColorString
        {
            get => ColorTranslator.ToHtml(TextColor ?? System.Drawing.Color.Black);
            set => TextColor = ColorTranslator.FromHtml(value);
        }
    }
}