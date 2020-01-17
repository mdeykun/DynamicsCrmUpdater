using System;

namespace McTools.Xrm.Connection
{
    public class SolutionDetail
    {
        public Guid SolutionId { get; set; }
        public string FriendlyName { get; set; }
        public string UniqueName { get; set; }
        public string PublisherPrefix { get; set; }
    }
}