using Cwru.Common.Model;
using Cwru.CrmRequests.Common;
using Cwru.CrmRequests.Common.Contracts;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Cwru.CrmRequests.Service
{
    [ServiceBehavior(Name = "CrmWebResourceUpdaterServerSvc", InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class CrmRequestsService : ICrmRequests
    {
        public async Task<Response<ConnectionResult>> ValidateConnectionAsync(string crmConnectionString)
        {
            return await Task.Factory.StartNew(() => ValidateConnection(crmConnectionString), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
        public async Task<Response<bool>> UploadWebresourceAsync(string crmConnectionString, WebResource webResource)
        {
            return await Task.Factory.StartNew(() => UploadWebresource(crmConnectionString, webResource), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
        public async Task<Response<bool>> CreateWebresourceAsync(string crmConnectionString, WebResource webResource, string solution)
        {
            return await Task.Factory.StartNew(() => CreateWebresource(crmConnectionString, webResource, solution), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
        public async Task<Response<IEnumerable<SolutionDetail>>> GetSolutionsListAsync(string crmConnectionString)
        {
            return await Task.Factory.StartNew(() => GetSolutionsList(crmConnectionString), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
        public async Task<Response<IEnumerable<WebResource>>> RetrieveSolutionWebResourcesAsync(string crmConnectionString, Guid solutionId, IEnumerable<string> webResourceNames)
        {
            return await Task.Factory.StartNew(() => RetrieveWebResources(crmConnectionString, solutionId, webResourceNames), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
        public async Task<Response<IEnumerable<WebResource>>> RetrieveWebResourcesAsync(string crmConnectionString, IEnumerable<string> webResourceNames)
        {
            return await Task.Factory.StartNew(() => RetrieveWebResources(crmConnectionString, webResourceNames), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
        public async Task<Response<bool>> PublishWebResourcesAsync(string crmConnectionString, IEnumerable<Guid> webResourcesIds)
        {
            return await Task.Factory.StartNew(() => PublishWebResources(crmConnectionString, webResourcesIds), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
        public async Task<Response<bool>> IsWebResourceExistsAsync(string crmConnectionString, string webResourceName)
        {
            return await Task.Factory.StartNew(() => IsWebResourceExists(crmConnectionString, webResourceName), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
        private Response<ConnectionResult> ValidateConnection(string crmConnectionString)
        {
            try
            {
                Console.WriteLine("Connection validation requested");

                var client = CreateOrganizationService(crmConnectionString);
                return new Response<ConnectionResult>()
                {
                    IsSuccessful = client.IsReady,
                    Payload = new ConnectionResult()
                    {
                        IsReady = client.IsReady,
                        LastCrmError = client.LastCrmError,
                        OrganizationUniqueName = client.ConnectedOrgUniqueName,
                        OrganizationVersion = client.ConnectedOrgVersion?.ToString(),
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return GetFailedResponse(ex, new ConnectionResult()
                {
                    IsReady = false,
                    LastCrmError = ex.Message
                });
            }
        }
        private Response<bool> UploadWebresource(string crmConnectionString, WebResource webResource)
        {
            try
            {
                Console.WriteLine("Requesting WR update");
                var client = CreateOrganizationService(crmConnectionString);

                client.Update(new Entity("webresource", webResource.Id.Value)
                {
                    Attributes =
                    {
                        { "content", webResource.Content }
                    }
                });

                return new Response<bool>()
                {
                    IsSuccessful = true,
                    Payload = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return GetFailedResponse<bool>(ex);
            }
        }
        private Response<bool> CreateWebresource(string crmConnectionString, WebResource webResource, string solution)
        {
            try
            {
                Console.WriteLine("Requesting WR create");
                var client = CreateOrganizationService(crmConnectionString);

                client.Execute(new CreateRequest()
                {
                    Target = new Entity("webresource")
                    {
                        Attributes =
                        {
                            { "name", webResource.Name },
                            { "displayname", webResource.DisplayName },
                            { "description", webResource.Description },
                            { "content", webResource.Content },
                            { "webresourcetype", new OptionSetValue(webResource.Type) },
                        }
                    },
                    Parameters = new ParameterCollection
                    {
                        { "SolutionUniqueName", solution }
                    }
                });

                return new Response<bool>()
                {
                    IsSuccessful = true,
                    Payload = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return GetFailedResponse<bool>(ex);
            }
        }
        private Response<IEnumerable<SolutionDetail>> GetSolutionsList(string crmConnectionString)
        {
            try
            {
                Console.WriteLine("Solution list requested");
                var client = CreateOrganizationService(crmConnectionString);

                var response = client.RetrieveMultiple(new QueryExpression
                {
                    EntityName = "solution",
                    ColumnSet = new ColumnSet(new string[] { "friendlyname", "uniquename", "publisherid" }),
                    Criteria = new FilterExpression()
                    {
                        Conditions =
                        {
                            new ConditionExpression("isvisible", ConditionOperator.Equal, true)
                        }
                    },
                    LinkEntities =
                    {
                        new LinkEntity("solution", "publisher", "publisherid", "publisherid", JoinOperator.Inner)
                        {
                            Columns = new ColumnSet("customizationprefix"),
                            EntityAlias = "publisher"
                        }
                    },
                    Orders =
                    {
                        new OrderExpression("friendlyname", OrderType.Ascending)
                    }
                });

                return new Response<IEnumerable<SolutionDetail>>()
                {
                    Payload = response.Entities.Select(x => new SolutionDetail()
                    {
                        UniqueName = x.GetAttributeValue<string>("uniquename"),
                        FriendlyName = x.GetAttributeValue<string>("friendlyname"),
                        PublisherPrefix = x.GetAttributeValue<AliasedValue>("publisher.customizationprefix") == null ? null : x.GetAttributeValue<AliasedValue>("publisher.customizationprefix").Value.ToString(),
                        SolutionId = x.Id
                    }),
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return GetFailedResponse<IEnumerable<SolutionDetail>>(ex);
            }
        }
        private Response<IEnumerable<WebResource>> RetrieveWebResources(string crmConnectionString, Guid solutionId, IEnumerable<string> webResourceNames)
        {
            var response = new Response<IEnumerable<WebResource>>();
            try
            {
                Console.WriteLine("Requesting WR retrieve");

                var client = CreateOrganizationService(crmConnectionString);
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
                                    new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId)
                                }
                            }
                        }
                    }
                };

                if (webResourceNames != null && webResourceNames.Count() > 0)
                {
                    query.Criteria = new FilterExpression(LogicalOperator.Or);
                    query.Criteria.Conditions.AddRange(webResourceNames.Select(x => new ConditionExpression("name", ConditionOperator.Equal, x)));
                }

                var retrieveWebresourcesResponse = client.RetrieveMultiple(query);

                return new Response<IEnumerable<WebResource>>()
                {
                    IsSuccessful = true,
                    Payload = retrieveWebresourcesResponse.Entities.Select(x => new WebResource()
                    {
                        Id = x.Id,
                        Name = x.GetAttributeValue<string>("name"),
                        Content = x.GetAttributeValue<string>("content")
                    })
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return GetFailedResponse<IEnumerable<WebResource>>(ex);
            }
        }
        private Response<IEnumerable<WebResource>> RetrieveWebResources(string crmConnectionString, IEnumerable<string> webResourceNames)
        {
            var response = new Response<IEnumerable<WebResource>>();
            try
            {
                Console.WriteLine("Requesting WR retrieve");

                if (webResourceNames == null || webResourceNames.Count() == 0)
                {
                    return response;
                }

                var client = CreateOrganizationService(crmConnectionString);

                var query = new QueryExpression("webresource")
                {
                    ColumnSet = new ColumnSet("name", "content"),
                    Criteria = new FilterExpression(LogicalOperator.Or)
                };

                query.Criteria.Conditions.AddRange(webResourceNames.Select(x => new ConditionExpression("name", ConditionOperator.Equal, x)));

                var retrieveWebresourcesResponse = client.RetrieveMultiple(query);

                return new Response<IEnumerable<WebResource>>()
                {
                    IsSuccessful = true,
                    Payload = retrieveWebresourcesResponse.Entities.Select(x => new WebResource()
                    {
                        Id = x.Id,
                        Name = x.GetAttributeValue<string>("name"),
                        Content = x.GetAttributeValue<string>("content")
                    })
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return GetFailedResponse<IEnumerable<WebResource>>(ex);
            }
        }
        private Response<bool> PublishWebResources(string crmConnectionString, IEnumerable<Guid> webResourcesIds)
        {
            try
            {
                Console.WriteLine("Requesting WR publish");

                var client = CreateOrganizationService(crmConnectionString);
                if (webResourcesIds == null || !webResourcesIds.Any())
                {
                    throw new ArgumentNullException("webresourcesId");
                }

                var webResourcesIdsXml = string.Join("", webResourcesIds.Select(id => string.Format("<webresource>{0}</webresource>", id)));
                webResourcesIdsXml = $"<importexportxml><webresources>{webResourcesIdsXml}</webresources></importexportxml>";

                client.Execute(new OrganizationRequest
                {
                    RequestName = "PublishXml",
                    Parameters = new ParameterCollection()
                    {
                        { "ParameterXml", webResourcesIdsXml }
                    }
                });

                return new Response<bool>()
                {
                    Payload = true,
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return GetFailedResponse<bool>(ex);
            }
        }
        private Response<bool> IsWebResourceExists(string crmConnectionString, string webResourceName)
        {
            try
            {
                Console.WriteLine("Requesting IsWebResourceExists");

                var client = CreateOrganizationService(crmConnectionString);

                var response = client.RetrieveMultiple(new QueryExpression
                {
                    TopCount = 1,
                    EntityName = "webresource",
                    ColumnSet = new ColumnSet(new string[] { "name" }),
                    Criteria = new FilterExpression()
                    {
                        Conditions =
                        {
                            new ConditionExpression("name", ConditionOperator.Equal, webResourceName)
                        }
                    }
                });

                var webresource = response.Entities.FirstOrDefault();

                return new Response<bool>()
                {
                    Payload = webresource != null,
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return GetFailedResponse<bool>(ex);
            }
        }
        private Response<T> GetFailedResponse<T>(Exception ex, T payload = default(T))
        {
            return new Response<T>()
            {
                Error = ex.ToString(),
                IsSuccessful = false,
                Payload = payload
            };
        }
        private CrmServiceClient CreateOrganizationService(string connectionString)
        {
            var сrmServiceClient = new CrmServiceClient(connectionString);
            return сrmServiceClient;
        }
    }
}
