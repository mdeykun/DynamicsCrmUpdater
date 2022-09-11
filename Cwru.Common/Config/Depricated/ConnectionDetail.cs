using Cwru.Common.Model;
using System;
using System.Xml.Serialization;

namespace Cwru.Common.Config.Depricated
{
    [XmlInclude(typeof(CertificateInfo))]
    public class ConnectionDetail
    {
        public Guid AzureAdAppId { get; set; }
        [XmlElement("CertificateInfo")]
        public CertificateInfo Certificate { get; set; }
        [XmlElement("ClientSecret")]
        public string ClientSecretEncrypted { get; set; }
        public Guid? ConnectionId { get; set; }
        public string ConnectionName { get; set; }
        public string ConnectionString { get; set; }
        public string HomeRealmUrl { get; set; }
        public bool IsCustomAuth { get; set; }
        public AuthenticationType NewAuthType { get; set; }
        public string Organization { get; set; }
        public string OrganizationVersion { get; set; }
        public string OriginalUrl { get; set; }
        public string ReplyUrl { get; set; }
        public bool SavePassword { get; set; }
        public TimeSpan Timeout { get; set; }
        public bool UseIfd { get; set; }
        public bool UseMfa { get; set; }
        public string UserDomain { get; set; }
        public string UserName { get; set; }
        [XmlElement("UserPassword")]
        public string UserPasswordEncrypted { get; set; }
        public SolutionDetail SelectedSolution { get; set; }
        public bool UseOnline => OriginalUrl.IndexOf(".dynamics.com", StringComparison.InvariantCultureIgnoreCase) > 0;
    }

    public class CertificateInfo
    {
        public string Issuer { get; set; }
        public string Name { get; set; }
        public string Thumbprint { get; set; }
    }
}