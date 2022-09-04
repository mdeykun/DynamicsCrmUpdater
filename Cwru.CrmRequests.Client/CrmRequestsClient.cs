using Cwru.Common.Model;
using Cwru.CrmRequests.Common;
using Cwru.CrmRequests.Common.Contracts;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Cwru.CrmRequests.Client
{
    public class CrmRequestsClient : ICrmRequests
    {
        private readonly NetNamedPipeBinding binding;
        private readonly EndpointAddress remoteAddress;

        public CrmRequestsClient(NetNamedPipeBinding binding, EndpointAddress remoteAddress)
        {
            this.binding = binding;
            this.remoteAddress = remoteAddress;
        }

        private CrmRequestsServiceProxy proxy;
        private ICrmRequests Client
        {
            get
            {
                proxy = proxy ?? new CrmRequestsServiceProxy(binding, remoteAddress);

                if (proxy.State == CommunicationState.Faulted)
                {
                    proxy.Abort();
                    proxy = new CrmRequestsServiceProxy(binding, remoteAddress);
                }

                return proxy.Client;
            }
        }

        public async Task<Response<IEnumerable<SolutionDetail>>> GetSolutionsListAsync(string crmConnection)
        {
            return await Client.GetSolutionsListAsync(crmConnection);
        }
        public async Task<Response<ConnectionResult>> ValidateConnectionAsync(string crmConnection)
        {
            return await Client.ValidateConnectionAsync(crmConnection);
        }
        public async Task<Response<bool>> CreateWebresourceAsync(string crmConnection, WebResource webResource, string solution)
        {
            return await Client.CreateWebresourceAsync(crmConnection, webResource, solution);
        }
        public async Task<Response<bool>> UploadWebresourceAsync(string crmConnection, WebResource webResource)
        {
            return await Client.UploadWebresourceAsync(crmConnection, webResource);
        }
        public async Task<Response<IEnumerable<WebResource>>> RetrieveWebResourcesAsync(string crmConnection, Guid solutionId, List<string> webResourceNames)
        {
            return await Client.RetrieveWebResourcesAsync(crmConnection, solutionId, webResourceNames);
        }
        public async Task<Response<bool>> PublishWebResourcesAsync(string crmConnection, IEnumerable<Guid> webResourcesIds)
        {
            return await Client.PublishWebResourcesAsync(crmConnection, webResourcesIds);
        }
        public async Task<Response<bool>> IsWebResourceExistsAsync(string crmConnection, string webResourceName)
        {
            return await Client.IsWebResourceExistsAsync(crmConnection, webResourceName);
        }
    }
}
