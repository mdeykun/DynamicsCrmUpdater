using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmWebResourcesUpdater.Client
{
    public class CrmWebResourcesUpdaterClient
    {
        private ICrmWebResourcesUpdaterService _client;
        private ICrmWebResourcesUpdaterService Client
        {
            get
            {
                if (_client == null)
                {
                    var pipeFactory = new ChannelFactory<ICrmWebResourcesUpdaterService>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/CrmWebResourceUpdaterSvc"));
                    _client = pipeFactory.CreateChannel();
                }
                return _client;
            }
        }

        public List<SolutionDetail> GetSolutionsList(ConnectionDetail connectionDetail)
        {
            return Client.GetSolutionsList(connectionDetail);
        }

        public ConnectionResult ValidateConnection(ConnectionDetail connectionDetail)
        {
            return Client.ValidateConnection(connectionDetail);
        }

        public void CreateWebresource(ConnectionDetail connectionDetail, WebResource webResource, string solution)
        {
            Client.CreateWebresource(connectionDetail, webResource, solution);
        }
        public void UploadWebresource(ConnectionDetail connectionDetail, WebResource webResource)
        {
            Client.UploadWebresource(connectionDetail, webResource);
        }

        public IEnumerable<WebResource> RetrieveWebResources(ConnectionDetail connectionDetail, List<string> webResourceNames)
        {
            return Client.RetrieveWebResources(connectionDetail, webResourceNames);
        }

        public void PublishWebResources(ConnectionDetail connectionDetail, IEnumerable<Guid> webResourcesIds)
        {
            Client.PublishWebResources(connectionDetail, webResourcesIds);
        }

        public SolutionDetail RetrieveSolution(ConnectionDetail connectionDetail, Guid solutionId)
        {
            return Client.RetrieveSolution(connectionDetail, solutionId);
        }

        public bool IsWebResourceExists(ConnectionDetail connectionDetail, string webResourceName)
        {
            return Client.IsWebResourceExists(connectionDetail, webResourceName);
        }
    }
}
