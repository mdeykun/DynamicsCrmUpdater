using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McTools.Xrm.Connection
{
    public class ConnectionResult
    {
        public List<SolutionDetail> Solutions { get; set; }
        public bool IsReady { get; set; }
        public string LastCrmError { get; set; }
        public string Organization { get; set; }
        public string OrganizationFriendlyName { get; set; }
        public string OrganizationVersion { get; set; }
        public string OrganizationDataServiceUrl { get; set; }
        public string OrganizationServiceUrl { get; set; }
        public string UserName { get; set; }
        public string WebApplicationUrl { get; set; }
        public Guid TenantId { get; set; }
        public string EnvironmentId { get; set; }
        public string ServerName { get; set; }
        public int? ServerPort { get; set; }
    }
}
