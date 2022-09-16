using Cwru.Common.Model;
using System;
using System.Security;

namespace McTools.Xrm.Connection.WinForms.Model
{
    public class CertificateInfo
    {
        public string Issuer { get; set; }
        public string Name { get; set; }
        public string Thumbprint { get; set; }
    }

    public class ConnectionDetail : IComparable, ICloneable
    {
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
        public Guid AzureAdAppId { get; set; }
        public CertificateInfo Certificate { get; set; }
        public SecureString ClientSecret { get; set; }
        public SecureString UserPassword { get; set; }
        public Guid? ConnectionId { get; set; }
        public string ConnectionName { get; set; }
        public string ConnectionString { get; set; }
        //public string EnvironmentId { get; set; }
        public string HomeRealmUrl { get; set; }
        public bool? IntegratedSecurity { get; set; }
        public AuthenticationType AuthType { get; set; }
        public string Organization { get; set; }
        public string OrganizationVersion { get; set; }
        public string OriginalUrl { get; set; }
        public string RefreshToken { get; set; }
        public string ReplyUrl { get; set; }
        public bool SavePassword { get; set; }
        public string ServerName { get; set; }
        public int? ServerPort { get; set; }
        public TimeSpan Timeout { get; set; }
        public bool UseConnectionString => !string.IsNullOrEmpty(ConnectionString);
        public bool UseIfd { get; set; }
        public bool UseMfa { get; set; }
        public bool UseOnline => !string.IsNullOrWhiteSpace(OriginalUrl) ? OriginalUrl.IndexOf(".dynamics.com", StringComparison.InvariantCultureIgnoreCase) > 0 : false;
        public string UserDomain { get; set; }
        public string UserName { get; set; }
        public bool UseSsl => OriginalUrl?.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) ?? OriginalUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase);
        public Guid? SelectedSolutionId { get; set; } = null;
        public string SolutionName { get; set; }
        public string OrganizationUrlName
        {
            get
            {
                var urlWithoutProtocol = OriginalUrl.Remove(0, UseSsl ? 8 : 7);
                if (urlWithoutProtocol.EndsWith("/"))
                {
                    urlWithoutProtocol = urlWithoutProtocol.Substring(0, urlWithoutProtocol.Length - 1);
                }
                var urlParts = urlWithoutProtocol.Split('/');

                return urlParts.Length > 1 && !urlParts[1].ToLower().StartsWith("main.aspx") ? urlParts[1] : urlParts[0].Split('.')[0];
            }
        }

        public object Clone()
        {
            var cd = new ConnectionDetail
            {
                ConnectionId = Guid.NewGuid(),
                ConnectionName = ConnectionName,
                ConnectionString = ConnectionString,
                HomeRealmUrl = HomeRealmUrl,
                Organization = Organization,
                OrganizationVersion = OrganizationVersion,
                SavePassword = SavePassword,
                ServerName = ServerName,
                ServerPort = ServerPort,
                UseIfd = UseIfd,
                UserDomain = UserDomain,
                UserName = UserName,
                OriginalUrl = OriginalUrl,
                Timeout = Timeout,
                UseMfa = UseMfa,
                AzureAdAppId = AzureAdAppId,
                ReplyUrl = ReplyUrl,
                RefreshToken = RefreshToken,
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
        public bool IsConnectionBrokenWithUpdatedData(ConnectionDetail originalDetail)
        {
            if (originalDetail == null)
            {
                return true;
            }

            if (originalDetail.HomeRealmUrl != HomeRealmUrl
                || originalDetail.IntegratedSecurity != IntegratedSecurity
                || originalDetail.Organization != Organization
                || originalDetail.ServerName.ToLower() != ServerName.ToLower()
                || originalDetail.ServerPort != ServerPort
                || originalDetail.UseIfd != UseIfd
                || originalDetail.UseOnline != UseOnline
                || originalDetail.UseSsl != UseSsl
                || originalDetail.UseMfa != UseMfa
                || originalDetail.ClientSecret != ClientSecret
                || originalDetail.AzureAdAppId != AzureAdAppId
                || originalDetail.ReplyUrl != ReplyUrl
                || originalDetail.UserDomain?.ToLower() != UserDomain?.ToLower()
                || originalDetail.UserName?.ToLower() != UserName?.ToLower()
                || SavePassword && UserPassword != null && originalDetail.UserPassword != UserPassword
                || SavePassword && ClientSecret != null && originalDetail.ClientSecret != ClientSecret
                || originalDetail.Certificate.Thumbprint != Certificate.Thumbprint)
            {
                return true;
            }

            return false;
        }
        public int CompareTo(object obj)
        {
            var detail = (ConnectionDetail)obj;
            return String.Compare(ConnectionName, detail.ConnectionName, StringComparison.Ordinal);
        }
        public override string ToString()
        {
            var name = this.ConnectionName ?? this.Organization;
            if (name != null)
            {
                return name;
            }
            else if (!string.IsNullOrEmpty(this.OriginalUrl))
            {
                var uri = new Uri(this.OriginalUrl);
                return uri.Host;
            }
            else if (ConnectionId != null)
            {
                return ConnectionId?.ToString("B");
            }

            return "Connection name is not set";
        }
    }
}