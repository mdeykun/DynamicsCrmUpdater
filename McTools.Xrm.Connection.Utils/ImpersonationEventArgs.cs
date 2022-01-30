using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McTools.Xrm.Connection.Utils
{
    public class ImpersonationEventArgs : EventArgs
    {
        public ImpersonationEventArgs(Guid userId, string username = null)
        {
            UserId = userId;
            UserName = username;
        }

        public Guid UserId { get; }

        public string UserName { get; }
    }
}
