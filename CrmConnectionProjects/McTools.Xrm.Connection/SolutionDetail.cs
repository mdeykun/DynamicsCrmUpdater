using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
