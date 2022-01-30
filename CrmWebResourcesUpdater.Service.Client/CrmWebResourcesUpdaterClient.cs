using CrmWebResourcesUpdater.DataModel;
using CrmWebResourcesUpdater.Service.Common;
using CrmWebResourcesUpdater.Service.Common.Interfaces;
using McTools.Xrm.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CrmWebResourcesUpdater.Service.Client
{
    public class CrmWebResourcesUpdaterClient
    {
        private CrmWebResourcesUpdaterClient()
        {

        }
        private static CrmWebResourcesUpdaterClient _instance;
        public static CrmWebResourcesUpdaterClient Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new CrmWebResourcesUpdaterClient();
                }
                return _instance;
            }
        }
        private CrmWebResourcesUpdaterServiceProxy _proxy;
        private ICrmWebResourcesUpdaterService Client
        {
            get
            {
                if (_proxy == null)
                {
                    _proxy = new CrmWebResourcesUpdaterServiceProxy();
                }
                if (_proxy.State == CommunicationState.Faulted)
                {
                    //Logger.WriteLineAsync("Service proxy is in a Faulted state, restarting...");
                    _proxy.Abort();
                    _proxy = new CrmWebResourcesUpdaterServiceProxy();
                }
                return _proxy.Client;
            }
        }

        public UpdaterServiceResponse<List<SolutionDetail>> GetSolutionsList(ConnectionDetail connectionDetail)
        {
            return Client.GetSolutionsListAsync(connectionDetail).Result;
        }

        public UpdaterServiceResponse<ConnectionResult> ValidateConnection(ConnectionDetail connectionDetail)
        {
            return Client.ValidateConnectionAsync(connectionDetail).Result;
        }

        public UpdaterServiceResponse<bool> CreateWebresource(ConnectionDetail connectionDetail, WebResource webResource, string solution)
        {
            return Client.CreateWebresourceAsync(connectionDetail, webResource, solution).Result;
        }
        public UpdaterServiceResponse<bool> UploadWebresource(ConnectionDetail connectionDetail, WebResource webResource)
        {
            return Client.UploadWebresourceAsync(connectionDetail, webResource).Result;
        }

        public UpdaterServiceResponse<IEnumerable<WebResource>> RetrieveWebResources(ConnectionDetail connectionDetail, List<string> webResourceNames)
        {
            return Client.RetrieveWebResourcesAsync(connectionDetail, webResourceNames).Result;
        }

        public UpdaterServiceResponse<bool> PublishWebResources(ConnectionDetail connectionDetail, IEnumerable<Guid> webResourcesIds)
        {
            return Client.PublishWebResourcesAsync(connectionDetail, webResourcesIds).Result;
        }

        public UpdaterServiceResponse<SolutionDetail> RetrieveSolution(ConnectionDetail connectionDetail, Guid solutionId)
        {
            return Client.RetrieveSolutionAsync(connectionDetail, solutionId).Result;
        }

        public UpdaterServiceResponse<bool> IsWebResourceExists(ConnectionDetail connectionDetail, string webResourceName)
        {
            return Client.IsWebResourceExistsAsync(connectionDetail, webResourceName).Result;
        }

        public async Task<UpdaterServiceResponse<List<SolutionDetail>>> GetSolutionsListAsync(ConnectionDetail connectionDetail)
        {
            return await Client.GetSolutionsListAsync(connectionDetail);
        }

        public async Task<UpdaterServiceResponse<ConnectionResult>> ValidateConnectionAsync(ConnectionDetail connectionDetail)
        {
            return await Client.ValidateConnectionAsync(connectionDetail);
        }

        public async Task<UpdaterServiceResponse<bool>> CreateWebresourceAsync(ConnectionDetail connectionDetail, WebResource webResource, string solution)
        {
            return await Client.CreateWebresourceAsync(connectionDetail, webResource, solution);
        }
        public async Task<UpdaterServiceResponse<bool>> UploadWebresourceAsync(ConnectionDetail connectionDetail, WebResource webResource)
        {
            return await Client.UploadWebresourceAsync(connectionDetail, webResource);
        }

        public async Task<UpdaterServiceResponse<IEnumerable<WebResource>>> RetrieveWebResourcesAsync(ConnectionDetail connectionDetail, List<string> webResourceNames)
        {
            return await Client.RetrieveWebResourcesAsync(connectionDetail, webResourceNames);
        }

        public async Task<UpdaterServiceResponse<bool>> PublishWebResourcesAsync(ConnectionDetail connectionDetail, IEnumerable<Guid> webResourcesIds)
        {
            return await Client.PublishWebResourcesAsync(connectionDetail, webResourcesIds);
        }

        public async Task<UpdaterServiceResponse<SolutionDetail>> RetrieveSolutionAsync(ConnectionDetail connectionDetail, Guid solutionId)
        {
            return await Client.RetrieveSolutionAsync(connectionDetail, solutionId);
        }

        public async Task<UpdaterServiceResponse<bool>> IsWebResourceExistsAsync(ConnectionDetail connectionDetail, string webResourceName)
        {
            return await Client.IsWebResourceExistsAsync(connectionDetail, webResourceName);
        }

        public async Task<ConnectionDetail> UseSdkLoginControlAsync(Guid connectionDetailId, bool isUpdate)
        {
            return await Client.UseSdkLoginControlAsync(connectionDetailId, isUpdate);
        }
    }
}
