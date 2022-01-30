using CrmWebResourcesUpdater.DataModel;
using CrmWebResourcesUpdater.Service.Common;
using CrmWebResourcesUpdater.Service.Common.Interfaces;
using CrmWebResourcesUpdater.Service.SdkLogin;
using McTools.Xrm.Connection;
using McTools.Xrm.Connection.Utils;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CrmWebResourcesUpdater.Service
{
    [ServiceBehavior(Name = "CrmWebResourceUpdaterServerSvc", InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class CrmWebResourcesUpdaterService : ICrmWebResourcesUpdaterService
    {
        public UpdaterServiceResponse<List<SolutionDetail>> GetSolutionsList(ConnectionDetail connectionDetail)
        {
            Console.WriteLine("Solution list requested");
            var response = new UpdaterServiceResponse<List<SolutionDetail>>();
            try
            {
                var client = connectionDetail.GetCrmServiceClient(true);

                QueryExpression query = new QueryExpression
                {
                    EntityName = "solution",
                    ColumnSet = new ColumnSet(new string[] { "friendlyname", "uniquename", "publisherid" }),
                    Criteria = new FilterExpression()
                };
                query.Criteria.AddCondition("isvisible", ConditionOperator.Equal, true);
                query.AddOrder("friendlyname", OrderType.Ascending);

                query.LinkEntities.Add(new LinkEntity("solution", "publisher", "publisherid", "publisherid", JoinOperator.Inner));
                query.LinkEntities[0].Columns.AddColumns("customizationprefix");
                query.LinkEntities[0].EntityAlias = "publisher";

                var solutionEntities = client.RetrieveMultiple(query).Entities;
                var solutions = solutionEntities.Select(x => new SolutionDetail()
                {
                    UniqueName = x.GetAttributeValue<string>("uniquename"),
                    FriendlyName = x.GetAttributeValue<string>("friendlyname"),
                    PublisherPrefix = x.GetAttributeValue<Microsoft.Xrm.Sdk.AliasedValue>("publisher.customizationprefix") == null ? null : x.GetAttributeValue<Microsoft.Xrm.Sdk.AliasedValue>("publisher.customizationprefix").Value.ToString(),
                    SolutionId = x.Id
                }).ToList();
                response.Payload = solutions;
                response.IsSuccessful = true;
                return response;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                response.IsSuccessful = false;
                response.Error = ex.ToString();
                return response;
            }
        }

        public UpdaterServiceResponse<ConnectionResult> ValidateConnection(ConnectionDetail connectionDetail)
        {
            Console.WriteLine("Connection validation requested");
            var response = new UpdaterServiceResponse<ConnectionResult>();
            try
            {
                var client = connectionDetail.GetCrmServiceClient(true);

                var webApplicationUrl = client.ConnectedOrgPublishedEndpoints[EndpointType.WebApplication];
                var webAppURi = new Uri(webApplicationUrl);

                response.IsSuccessful = true;
                response.Payload = new ConnectionResult()
                {
                    IsReady = client.IsReady,
                    LastCrmError = client.LastCrmError,
                    Organization = client.ConnectedOrgUniqueName,
                    OrganizationFriendlyName = client.ConnectedOrgFriendlyName,
                    OrganizationVersion = client.ConnectedOrgVersion.ToString(),
                    OrganizationDataServiceUrl = client.ConnectedOrgPublishedEndpoints[EndpointType.OrganizationDataService],
                    OrganizationServiceUrl = client.ConnectedOrgPublishedEndpoints[EndpointType.OrganizationService],
                    WebApplicationUrl = webApplicationUrl,
                    TenantId = client.TenantId,
                    EnvironmentId = client.EnvironmentId,
                    ServerName = webAppURi.Host,
                    ServerPort = webAppURi.Port,
                    UserName = connectionDetail.UserName?.Length > 0
                                ? connectionDetail.UserName
                                : client.OAuthUserId?.Length > 0
                                    ? client.OAuthUserId
                                    : connectionDetail.AzureAdAppId != Guid.Empty
                                        ? connectionDetail.AzureAdAppId.ToString("B")
                                        : null
                };
                 
                return response;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                response.Payload = new ConnectionResult()
                {
                    IsReady = false,
                    LastCrmError = ex.Message
                };
                response.IsSuccessful = false;
                response.Error = ex.ToString();

                return response;
            }
        }

        public UpdaterServiceResponse<bool> CreateWebresource(ConnectionDetail connectionDetail, WebResource webResource, string solution)
        {
            var response = new UpdaterServiceResponse<bool>();

            try
            {
                Console.WriteLine("Requesting WR create");
                var client = connectionDetail.GetCrmServiceClient(true);
                var webResourceEntity = new Entity("webresource");
                webResourceEntity["name"] = webResource.Name;
                webResourceEntity["displayname"] = webResource.DisplayName;
                webResourceEntity["description"] = webResource.Description;
                webResourceEntity["content"] = webResource.Content;
                webResourceEntity["webresourcetype"] = new OptionSetValue(webResource.Type);
                
                CreateRequest createRequest = new CreateRequest
                {
                    Target = webResourceEntity
                };
                createRequest.Parameters.Add("SolutionUniqueName", solution);
                client.Execute(createRequest);

                response.IsSuccessful = true;
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                response.Error = ex.ToString();
                response.IsSuccessful = false;
                return response;
            }
        }
        public UpdaterServiceResponse<bool> UploadWebresource(ConnectionDetail connectionDetail, WebResource webResource)
        {
            var response = new UpdaterServiceResponse<bool>();
            try
            {
                Console.WriteLine("Requesting WR update");
                var client = connectionDetail.GetCrmServiceClient(true);
                var update = new Entity("webresource", webResource.Id.Value)
                {
                    Attributes =
                {
                    { "content", webResource.Content }
                }
                };
                client.Update(update);

                response.IsSuccessful = true;
                return response;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                response.Error = ex.ToString();
                response.IsSuccessful = false;
                return response;
            }
        }

        public UpdaterServiceResponse<IEnumerable<WebResource>> RetrieveWebResources(ConnectionDetail connectionDetail, List<string> webResourceNames)
        {
            var response = new UpdaterServiceResponse<IEnumerable<WebResource>>();
            try
            {
                Console.WriteLine("Requesting WR retrieve");
                var client = connectionDetail.GetCrmServiceClient(true);
                var query = new QueryExpression("webresource")
                {
                    ColumnSet = new ColumnSet("name", "content"),
                    LinkEntities =
                {
                    new LinkEntity("webresource", "solutioncomponent", "webresourceid", "objectid", JoinOperator.Inner)
                    {
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("solutionid", ConditionOperator.Equal, connectionDetail.SelectedSolution.SolutionId)
                            }
                        }
                    }
                }
                };

                if (webResourceNames != null && webResourceNames.Count > 0)
                {
                    query.Criteria = new FilterExpression(LogicalOperator.Or);
                    query.Criteria.Conditions.AddRange(webResourceNames.Select(x => new ConditionExpression("name", ConditionOperator.Equal, x)));
                }
                var retrieveWebresourcesResponse = client.RetrieveMultiple(query);
                var webResources = retrieveWebresourcesResponse.Entities.ToList();

                response.Payload = webResources.Select(x => new WebResource()
                {
                    Id = x.Id,
                    Name = x.GetAttributeValue<string>("name"),
                    Content = x.GetAttributeValue<string>("content")
                });
                response.IsSuccessful = true;
                return response;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                response.Error = ex.ToString();
                response.IsSuccessful = false;
                return response;
            }
        }

        public UpdaterServiceResponse<bool> PublishWebResources(ConnectionDetail connectionDetail, IEnumerable<Guid> webResourcesIds)
        {
            var response = new UpdaterServiceResponse<bool>();
            try
            {
                Console.WriteLine("Requesting WR publish");
                var client = connectionDetail.GetCrmServiceClient(true);
                var orgContext = new OrganizationServiceContext(client);

                if (webResourcesIds == null || !webResourcesIds.Any())
                {
                    throw new ArgumentNullException("webresourcesId");
                }

                var request = new OrganizationRequest { RequestName = "PublishXml" };
                request.Parameters = new ParameterCollection();
                request.Parameters.Add(new KeyValuePair<string, object>("ParameterXml",
                string.Format("<importexportxml><webresources>{0}</webresources></importexportxml>",
                string.Join("", webResourcesIds.Select(a => string.Format("<webresource>{0}</webresource>", a)))
                )));

                orgContext.Execute(request);
                
                response.IsSuccessful = true;
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                response.Error = ex.ToString();
                response.IsSuccessful = false;
                return response;
            }
        }

        public UpdaterServiceResponse<SolutionDetail> RetrieveSolution(ConnectionDetail connectionDetail, Guid solutionId)
        {
            var response = new UpdaterServiceResponse<SolutionDetail>();
            try
            {
                Console.WriteLine("Requesting Solution retrieve");
                var client = connectionDetail.GetCrmServiceClient(true);
                QueryExpression query = new QueryExpression
                {
                    EntityName = "solution",
                    ColumnSet = new ColumnSet(new string[] { "friendlyname", "uniquename", "publisherid" }),
                    Criteria = new FilterExpression()
                };
                query.Criteria.AddCondition("isvisible", ConditionOperator.Equal, true);
                query.Criteria.AddCondition("solutionid", ConditionOperator.Equal, solutionId);

                query.LinkEntities.Add(new LinkEntity("solution", "publisher", "publisherid", "publisherid", JoinOperator.Inner));
                query.LinkEntities[0].Columns.AddColumns("customizationprefix");
                query.LinkEntities[0].EntityAlias = "publisher";

                var retrieveSolutionResponse = client.RetrieveMultiple(query);
                var entity = retrieveSolutionResponse.Entities.FirstOrDefault();
                if(entity == null)
                {
                    return null;
                }
                var solution = new SolutionDetail()
                {
                    SolutionId = entity.Id,
                    PublisherPrefix = entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix") == null ? null : entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix").Value.ToString()
                };

                response.Payload = solution;
                response.IsSuccessful = true;
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                response.Error = ex.ToString();
                response.IsSuccessful = false;
                return response;
            }
        }

        public UpdaterServiceResponse<bool> IsWebResourceExists(ConnectionDetail connectionDetail, string webResourceName)
        {
            var response = new UpdaterServiceResponse<bool>();
            try
            {
                Console.WriteLine("Requesting Is WebResource Exists");
                var client = connectionDetail.GetCrmServiceClient(true);
                QueryExpression query = new QueryExpression
                {
                    EntityName = "webresource",
                    ColumnSet = new ColumnSet(new string[] { "name" }),
                    Criteria = new FilterExpression()
                };
                query.Criteria.AddCondition("name", ConditionOperator.Equal, webResourceName);
                //query.Criteria.AddCondition("solutionid", ConditionOperator.Equal, _connectionDetail.SolutionId);

                var isWebResourceExistsResponse = client.RetrieveMultiple(query);
                var entity = isWebResourceExistsResponse.Entities.FirstOrDefault();

                response.Payload = entity == null ? false : true;
                response.IsSuccessful = true;
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                response.Error = ex.ToString();
                response.IsSuccessful = false;
                return response;
            }
        }

        public async Task<ConnectionDetail> UseSdkLoginControlAsync(Guid connectionDetailId, bool isUpdate)
        {
            try
            {
                var signal = new SemaphoreSlim(0, 1);
                var connectionDetail = new ConnectionDetail();
                var thread = new Thread(() => {
                    bool rdbUseCustom = false; string appId = null; string redirectUri = null;
                    var ctrl = new CRMLoginForm1(connectionDetailId, isUpdate);

                    if (rdbUseCustom)
                    {
                        ctrl.AppId = appId;
                        ctrl.RedirectUri = new Uri(redirectUri);
                    }
                    else
                    {
                        ctrl.AppId = "51f81489-12ee-4a9e-aaae-a2591f45987d";
                        ctrl.RedirectUri = new Uri("app://58145B91-0C36-4500-8554-080854F2AC97");
                    }
                    ctrl.ConnectionToCrmCompleted += (loginCtrl, evt) =>
                    {
                        try
                        {
                            var connectionManager = ((CRMLoginForm1)loginCtrl).CrmConnectionMgr;
                            var authType = DataModels.AuthenticationProviderType.None;
                            switch (connectionManager.CrmSvc.ActiveAuthenticationType)
                            {
                                case Microsoft.Xrm.Tooling.Connector.AuthenticationType.AD:
                                    authType = DataModels.AuthenticationProviderType.ActiveDirectory;
                                    break;

                                case Microsoft.Xrm.Tooling.Connector.AuthenticationType.IFD:
                                case Microsoft.Xrm.Tooling.Connector.AuthenticationType.Claims:
                                    authType = DataModels.AuthenticationProviderType.Federation;
                                    break;

                                default:
                                    authType = DataModels.AuthenticationProviderType.OnlineFederation;
                                    break;
                            }

                            connectionDetail.IsFromSdkLoginCtrl = true;
                            connectionDetail.AuthType = authType;
                            connectionDetail.UseIfd = authType == DataModels.AuthenticationProviderType.Federation;
                            connectionDetail.Organization = connectionManager.ConnectedOrgUniqueName;
                            connectionDetail.OrganizationFriendlyName = connectionManager.ConnectedOrgFriendlyName;
                            connectionDetail.OrganizationDataServiceUrl =
                                connectionManager.ConnectedOrgPublishedEndpoints[EndpointType.OrganizationDataService];
                            connectionDetail.OrganizationServiceUrl =
                                connectionManager.ConnectedOrgPublishedEndpoints[EndpointType.OrganizationService];
                            connectionDetail.WebApplicationUrl =
                                connectionManager.ConnectedOrgPublishedEndpoints[EndpointType.WebApplication];
                            connectionDetail.OriginalUrl = connectionDetail.WebApplicationUrl;
                            connectionDetail.ServerName = new Uri(connectionDetail.WebApplicationUrl).Host;
                            connectionDetail.OrganizationVersion = connectionManager.CrmSvc.ConnectedOrgVersion.ToString();
                            if (!string.IsNullOrEmpty(connectionManager.ClientId))
                            {
                                connectionDetail.AzureAdAppId = new Guid(connectionManager.ClientId);
                                connectionDetail.ReplyUrl = connectionManager.RedirectUri.AbsoluteUri;
                            }
                            connectionDetail.UserName = connectionManager.CrmSvc.OAuthUserId;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        finally
                        {
                            signal.Release();
                        }
                    };

                    ctrl.ShowDialog();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                
                await signal.WaitAsync();
                thread.Abort();
                return connectionDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task<UpdaterServiceResponse<ConnectionResult>> ValidateConnectionAsync(ConnectionDetail connectionDetail)
        {
            return await Task.Factory.StartNew(() => ValidateConnection(connectionDetail));
        }

        public async Task<UpdaterServiceResponse<bool>> UploadWebresourceAsync(ConnectionDetail connectionDetail, WebResource webResource)
        {
            return await Task.Factory.StartNew(() => UploadWebresource(connectionDetail, webResource));
        }

        public async Task<UpdaterServiceResponse<bool>> CreateWebresourceAsync(ConnectionDetail connectionDetail, WebResource webResource, string solution)
        {
            return await Task.Factory.StartNew(() => CreateWebresource(connectionDetail, webResource, solution));
        }

        public async Task<UpdaterServiceResponse<List<SolutionDetail>>> GetSolutionsListAsync(ConnectionDetail connectionDetail)
        {
            return await Task.Factory.StartNew(() => GetSolutionsList(connectionDetail));
        }

        public async Task<UpdaterServiceResponse<IEnumerable<WebResource>>> RetrieveWebResourcesAsync(ConnectionDetail connectionDetail, List<string> webResourceNames)
        {
            return await Task.Factory.StartNew(() => RetrieveWebResources(connectionDetail, webResourceNames));
        }

        public async Task<UpdaterServiceResponse<bool>> PublishWebResourcesAsync(ConnectionDetail connectionDetail, IEnumerable<Guid> webResourcesIds)
        {
            return await Task.Factory.StartNew(() => PublishWebResources(connectionDetail, webResourcesIds));
        }

        public async Task<UpdaterServiceResponse<SolutionDetail>> RetrieveSolutionAsync(ConnectionDetail connectionDetail, Guid solutionId)
        {
            return await Task.Factory.StartNew(() => RetrieveSolution(connectionDetail, solutionId));
        }

        public async Task<UpdaterServiceResponse<bool>> IsWebResourceExistsAsync(ConnectionDetail connectionDetail, string webResourceName)
        {
            return await Task.Factory.StartNew(() => IsWebResourceExists(connectionDetail, webResourceName));
        }
    }
}
