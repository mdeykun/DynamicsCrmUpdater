using System;

namespace Cwru.CrmRequests.Common
{
    public class ConnectionResult
    {
        public bool IsReady { get; set; }
        public string LastCrmError { get; set; }
        public string OrganizationUniqueName { get; set; }
        public string OrganizationVersion { get; set; }
        public Exception LastCrmException { get; set; }
    }
}
