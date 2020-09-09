using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McTools.Xrm.Connection
{
    public class ConnectionResult
    {
        public CrmServiceClient CrmServiceClient { get; set; }
        public List<SolutionDetail> Solutions { get; set; }
    }
}
