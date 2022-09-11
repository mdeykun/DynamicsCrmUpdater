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

        public async Task<Response<IEnumerable<SolutionDetail>>> GetSolutionsListAsync(string crmConnectionString)
        {
            return await Client.GetSolutionsListAsync(crmConnectionString);
        }
        public async Task<Response<ConnectionResult>> ValidateConnectionAsync(string crmConnectionString)
        {
            return await Client.ValidateConnectionAsync(crmConnectionString);
        }
        public async Task<Response<bool>> CreateWebresourceAsync(string crmConnectionString, WebResource webResource, string solution)
        {
            return await Client.CreateWebresourceAsync(crmConnectionString, webResource, solution);
        }
        public async Task<Response<bool>> UploadWebresourceAsync(string crmConnectionString, WebResource webResource)
        {
            return await Client.UploadWebresourceAsync(crmConnectionString, webResource);
        }
        public async Task<Response<IEnumerable<WebResource>>> RetrieveSolutionWebResourcesAsync(string crmConnectionString, Guid solutionId, IEnumerable<string> webResourceNames)
        {
            return await Client.RetrieveSolutionWebResourcesAsync(crmConnectionString, solutionId, webResourceNames);
        }
        public async Task<Response<IEnumerable<WebResource>>> RetrieveWebResourcesAsync(string crmConnectionString, IEnumerable<string> webResourceNames)
        {
            return await Client.RetrieveWebResourcesAsync(crmConnectionString, webResourceNames);
        }
        public async Task<Response<bool>> PublishWebResourcesAsync(string crmConnectionString, IEnumerable<Guid> webResourcesIds)
        {
            return await Client.PublishWebResourcesAsync(crmConnectionString, webResourcesIds);
        }
        public async Task<Response<bool>> IsWebResourceExistsAsync(string crmConnectionString, string webResourceName)
        {
            return await Client.IsWebResourceExistsAsync(crmConnectionString, webResourceName);
        }
    }
}
