using Cwru.Common.Model;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Cwru.CrmRequests.Common.Contracts
{
    [ServiceContract]
    public interface ICrmRequests
    {
        [OperationContract]
        Task<Response<ConnectionResult>> ValidateConnectionAsync(string crmConnectionString);

        [OperationContract]
        Task<Response<bool>> UploadWebresourceAsync(string crmConnectionString, WebResource webResource);

        [OperationContract]
        Task<Response<bool>> CreateWebresourceAsync(string crmConnectionString, WebResource webResource, string solution);

        [OperationContract]
        Task<Response<IEnumerable<SolutionDetail>>> GetSolutionsListAsync(string crmConnectionString);

        [OperationContract]
        Task<Response<IEnumerable<WebResource>>> RetrieveSolutionWebResourcesAsync(string crmConnectionString, Guid solutionId, IEnumerable<string> webResourceNames);

        [OperationContract]
        Task<Response<IEnumerable<WebResource>>> RetrieveWebResourcesAsync(string crmConnectionString, IEnumerable<string> webResourceNames);

        [OperationContract]
        Task<Response<bool>> PublishWebResourcesAsync(string crmConnectionString, IEnumerable<Guid> webResourcesIds);

        [OperationContract]
        Task<Response<bool>> IsWebResourceExistsAsync(string crmConnectionString, string webResourceName);
    }
}
