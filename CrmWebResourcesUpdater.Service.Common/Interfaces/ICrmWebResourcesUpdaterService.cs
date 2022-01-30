using CrmWebResourcesUpdater.DataModel;
using McTools.Xrm.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CrmWebResourcesUpdater.Service.Common.Interfaces
{
    [ServiceContract]
    public interface ICrmWebResourcesUpdaterService
    {

        [OperationContract]
        Task<UpdaterServiceResponse<ConnectionResult>> ValidateConnectionAsync(ConnectionDetail connectionDetail);

        [OperationContract]
        Task<UpdaterServiceResponse<bool>> UploadWebresourceAsync(ConnectionDetail connectionDetail, WebResource webResource);

        [OperationContract]
        Task<UpdaterServiceResponse<bool>> CreateWebresourceAsync(ConnectionDetail connectionDetail, WebResource webResource, string solution);

        [OperationContract]
        Task<UpdaterServiceResponse<List<SolutionDetail>>> GetSolutionsListAsync(ConnectionDetail connectionDetail);

        [OperationContract]
        Task<UpdaterServiceResponse<IEnumerable<WebResource>>> RetrieveWebResourcesAsync(ConnectionDetail connectionDetail, List<string> webResourceNames);

        [OperationContract]
        Task<UpdaterServiceResponse<bool>> PublishWebResourcesAsync(ConnectionDetail connectionDetail, IEnumerable<Guid> webResourcesIds);

        [OperationContract]
        Task<UpdaterServiceResponse<SolutionDetail>> RetrieveSolutionAsync(ConnectionDetail connectionDetail, Guid solutionId);

        [OperationContract]
        Task<UpdaterServiceResponse<bool>> IsWebResourceExistsAsync(ConnectionDetail connectionDetail, string webResourceName);

        [OperationContract]
        Task<ConnectionDetail> UseSdkLoginControlAsync(Guid connectionDetailId, bool isUpdate);
    }
}
