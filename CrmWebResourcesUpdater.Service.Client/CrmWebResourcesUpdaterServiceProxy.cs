using CrmWebResourcesUpdater.Service.Common.Interfaces;
using System;
using System.ServiceModel;

namespace CrmWebResourcesUpdater.Service.Client
{
    public class CrmWebResourcesUpdaterServiceProxy : System.ServiceModel.ClientBase<ICrmWebResourcesUpdaterService>
    {
        public CrmWebResourcesUpdaterServiceProxy() : base(new NetNamedPipeBinding() {
            ReceiveTimeout = new TimeSpan(0, 10, 0),
            SendTimeout = new TimeSpan(0, 10, 0),
            MaxReceivedMessageSize = 102400000,
            MaxBufferSize = 102400000,
            MaxBufferPoolSize = 102400000
        }, new EndpointAddress("net.pipe://localhost/CrmWebResourceUpdaterSvc"))
        {

        }

        public ICrmWebResourcesUpdaterService Client
        {
            get { return base.Channel; }
        }

    }
}
