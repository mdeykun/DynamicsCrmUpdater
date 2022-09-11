namespace Cwru.Common.Model
{
    public class ConnectionResult
    {
        public bool IsReady { get; set; }
        public string LastCrmError { get; set; }
        public string OrganizationUniqueName { get; set; }
        public string OrganizationVersion { get; set; }
    }
}
