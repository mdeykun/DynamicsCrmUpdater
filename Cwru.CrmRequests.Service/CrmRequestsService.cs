﻿using Cwru.Common.Model;
using Cwru.CrmRequests.Common;
using Cwru.CrmRequests.Common.Contracts;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cwru.CrmRequests.Service
{
    [ServiceBehavior(Name = "CrmWebResourceUpdaterServerSvc", InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class CrmRequestsService : ICrmRequests
    {
        private List<string> useAlternateConnection = new List<string>();
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
        public async Task<Response<IEnumerable<WebResource>>> RetrieveAllSolutionWebResourcesAsync(string crmConnectionString, Guid solutionId)
        {
            return await Task.Factory.StartNew(() => RetrieveWebResources(crmConnectionString, solutionId, null, false), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
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

                var client = CreateOrganizationService(crmConnectionString, false, out var info);

                return new Response<ConnectionResult>()
                {
                    IsSuccessful = true,
                    ConnectionInfo = info,
                    Payload = new ConnectionResult()
                    {
                        IsReady = client.IsReady,
                        LastCrmError = client.LastCrmError,
                        LastCrmException = client.LastCrmException,
                        OrganizationUniqueName = client.ConnectedOrgUniqueName,
                        OrganizationVersion = client.ConnectedOrgVersion?.ToString(),
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return GetFailedResponse<ConnectionResult>(ex);
            }
        }
        private Response<bool> UploadWebresource(string crmConnectionString, WebResource webResource)
        {
            try
            {
                Console.WriteLine("Wr updating requested");
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
                Console.WriteLine("Wr creation requested");
                var client = CreateOrganizationService(crmConnectionString);

                var target = new Entity("webresource")
                {
                    Attributes =
                    {
                        { "name", webResource.Name },
                        { "displayname", webResource.DisplayName },
                        { "description", webResource.Description },
                        { "content", webResource.Content },
                        { "webresourcetype", new OptionSetValue((int)webResource.Type) },
                    }
                };

                var request = new CreateRequest()
                {
                    Parameters = new ParameterCollection
                    {
                        { "SolutionUniqueName", solution }
                    }
                };

                request.Target = target;

                client.Execute(request);

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
                Console.WriteLine("Solutions list requested");
                var client = CreateOrganizationService(crmConnectionString);

                var response = client.RetrieveMultiple(new QueryExpression
                {
                    EntityName = "solution",
                    ColumnSet = new ColumnSet("friendlyname", "uniquename", "publisherid"),
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
        private Response<IEnumerable<WebResource>> RetrieveWebResources(string crmConnectionString, Guid solutionId, IEnumerable<string> webResourceNames, bool downloadContent = true)
        {
            try
            {
                Console.WriteLine("Wr requested");

                var columnSet = new ColumnSet("name", "webresourcetype");
                if (downloadContent)
                {
                    columnSet.AddColumn("content");
                }

                var client = CreateOrganizationService(crmConnectionString);
                var query = new QueryExpression("webresource")
                {
                    ColumnSet = columnSet,
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

                var retrieveWebresourcesResponse = RetriveAll(client, query);

                return new Response<IEnumerable<WebResource>>()
                {
                    IsSuccessful = true,
                    Payload = retrieveWebresourcesResponse.Select(x => new WebResource()
                    {
                        Id = x.Id,
                        Name = x.GetAttributeValue<string>("name"),
                        Content = x.GetAttributeValue<string>("content"),
                        Type = (WebResourceType)x.GetAttributeValue<OptionSetValue>("webresourcetype")?.Value
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
            try
            {
                Console.WriteLine("Wr requested");

                if (webResourceNames == null || webResourceNames.Count() == 0)
                {
                    return new Response<IEnumerable<WebResource>>() { IsSuccessful = true, Payload = Enumerable.Empty<WebResource>() };
                }

                var client = CreateOrganizationService(crmConnectionString);

                var query = new QueryExpression("webresource")
                {
                    ColumnSet = new ColumnSet("name", "content", "webresourcetype"),
                    Criteria = new FilterExpression(LogicalOperator.Or)
                };

                query.Criteria.Conditions.AddRange(webResourceNames.Select(x => new ConditionExpression("name", ConditionOperator.Equal, x)));

                var retrieveWebresourcesResponse = RetriveAll(client, query);

                return new Response<IEnumerable<WebResource>>()
                {
                    IsSuccessful = true,
                    Payload = retrieveWebresourcesResponse.Select(x => new WebResource()
                    {
                        Id = x.Id,
                        Name = x.GetAttributeValue<string>("name"),
                        Content = x.GetAttributeValue<string>("content"),
                        Type = (WebResourceType)x.GetAttributeValue<OptionSetValue>("webresourcetype")?.Value
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
                Console.WriteLine("Wr publishing requested");

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
                Console.WriteLine("IsWebResourceExists requested");

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
        private CrmServiceClient CreateOrganizationService(string crmConnectionString)
        {
            return CreateOrganizationService(crmConnectionString, true, out _);
        }
        private CrmServiceClient CreateOrganizationService(string crmConnectionString, bool throwEx, out string connectionInfo)
        {
            connectionInfo = string.Empty;

            CrmServiceClient client;
            if (useAlternateConnection.Contains(crmConnectionString))
            {
                client = CreateOrganizationServiceAlternate(crmConnectionString, out connectionInfo);
            }
            else
            {
                client = new CrmServiceClient(crmConnectionString);
                if (client.IsReady != true)
                {
                    var alternateClient = CreateOrganizationServiceAlternate(crmConnectionString, out connectionInfo);
                    if (alternateClient?.IsReady == true)
                    {
                        useAlternateConnection.Add(crmConnectionString);
                        client = alternateClient;
                    }
                }
            }

            if (client?.IsReady != true)
            {
                if (useAlternateConnection.Contains(crmConnectionString))
                {
                    useAlternateConnection.Remove(crmConnectionString);
                }

                if (throwEx)
                {
                    throw client?.LastCrmException != null ? throw new Exception(client.LastCrmError, client.LastCrmException) : new Exception("Crm connection is not ready");
                }
            }

            return client;
        }
        private CrmServiceClient CreateOrganizationServiceAlternate(string crmConnectionString, out string alternateClientInfo)
        {
            CrmServiceClient alternateClient = null;
            alternateClientInfo = string.Empty;

            try
            {
                var cs = CrmConnectionString.Parse(crmConnectionString);

                if (cs.AuthenticationType != Cwru.Common.Model.AuthenticationType.AD &&
                    cs.AuthenticationType != Cwru.Common.Model.AuthenticationType.IFD)
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(cs.ServiceUri))
                {
                    throw new ArgumentNullException(nameof(cs.ServiceUri));
                }

                var serviceUri = cs.ServiceUri.Trim();
                var uri = new Uri(serviceUri);
                var orgName = GetOrganizationName(serviceUri);
                var useSsl = string.Compare(uri.Scheme, "https", true) == 0;

                alternateClientInfo = "Creating alternate client=>";

                if (cs.AuthenticationType == Cwru.Common.Model.AuthenticationType.AD)
                {
                    alternateClient = new CrmServiceClient(
                        credential: cs.IntegratedSecurity != true ? new NetworkCredential(cs.UserName, cs.Password, cs.Domain) : CredentialCache.DefaultNetworkCredentials,
                        hostName: uri.Host,
                        port: uri.Port.ToString(),
                        orgName: orgName,
                        useUniqueInstance: cs.RequireNewInstance == true,
                        useSsl: useSsl);
                }

                if (cs.AuthenticationType == Cwru.Common.Model.AuthenticationType.IFD)
                {
                    if (cs.IntegratedSecurity == true)
                    {
                        alternateClient = new CrmServiceClient(
                            credential: CredentialCache.DefaultNetworkCredentials,
                            authType: Microsoft.Xrm.Tooling.Connector.AuthenticationType.IFD,
                            hostName: uri.Host,
                            port: uri.Port.ToString(),
                            orgName: orgName,
                            useUniqueInstance: cs.RequireNewInstance == true,
                            useSsl: useSsl);
                    }
                    else
                    {
                        alternateClient = new CrmServiceClient(
                            userId: cs.UserName,
                            password: cs.Password,
                            domain: cs.Domain,
                            homeRealm: cs.HomeRealmUri,
                            hostName: uri.Host,
                            port: uri.Port.ToString(),
                            orgName: orgName,
                            useUniqueInstance: cs.RequireNewInstance == true,
                            useSsl: useSsl);
                    }
                }

                if (alternateClient?.IsReady == true)
                {
                    alternateClientInfo += "Created";
                }
                else
                {
                    alternateClientInfo += $"Failed\r\n{alternateClient?.LastCrmError}";
                }
            }
            catch (Exception ex)
            {
                alternateClientInfo += $"Failed\r\n{ex}";
                Console.WriteLine(ex.ToString());
            }

            return alternateClient;
        }
        private List<Entity> RetriveAll(IOrganizationService organizationService, QueryExpression query)
        {
            var result = new List<Entity>();

            var response = organizationService.RetrieveMultiple(query);
            result.AddRange(response.Entities);

            for (var pageNumber = 2; pageNumber <= 10; pageNumber++)
            {
                if (!response.MoreRecords)
                {
                    break;
                }

                query.PageInfo.PagingCookie = response.PagingCookie;
                query.PageInfo.PageNumber = pageNumber;

                response = organizationService.RetrieveMultiple(query);
                result.AddRange(response.Entities);
            }

            return result;
        }

        private Response<T> GetFailedResponse<T>(Exception ex, T payload = default(T), string connectionInfo = null)
        {
            return new Response<T>()
            {
                ErrorMessage = ex.Message,
                IsSuccessful = false,
                Payload = payload,
                Exception = ex,
                ConnectionInfo = connectionInfo
            };
        }

        private string GetOrganizationName(string serviceUri)
        {
            if (string.IsNullOrEmpty(serviceUri))
            {
                throw new ArgumentNullException(nameof(serviceUri));
            }

            var match = Regex.Match(serviceUri?.Trim(), @"^(?:https?):\/\/\S*?\/(\S*?)(?:\/\S*\s*|\s*)$", RegexOptions.IgnoreCase);

            if (match.Success == false || match.Groups.Count <= 1)
            {
                throw new Exception("Can't retrieve orgName from Service Uri");
            }

            return match.Groups[1].Value;
        }
    }
}
