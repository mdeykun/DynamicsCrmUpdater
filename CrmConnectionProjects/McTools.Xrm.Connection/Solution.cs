using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McTools.Xrm.Connection
{
    public class Solution
    {
        public SolutionDetail SolutionDetail { get; set; }

        public override string ToString()
        {
            return this.SolutionDetail.FriendlyName;
        }
    }
}
