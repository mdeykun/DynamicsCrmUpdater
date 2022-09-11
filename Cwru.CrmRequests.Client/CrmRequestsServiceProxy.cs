using Cwru.CrmRequests.Common.Contracts;
using System.ServiceModel;

namespace Cwru.CrmRequests.Client
{
    public class CrmRequestsServiceProxy : ClientBase<ICrmRequests>
    {
        public CrmRequestsServiceProxy(NetNamedPipeBinding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {

        }

        public ICrmRequests Client
        {
            get { return base.Channel; }
        }
    }
}
