
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using Label = Microsoft.Xrm.Sdk.Label;

namespace McTools.Xrm.Connection.Utils
{
    public static class ConnectionDetailExtensions
    {
        public static CrmServiceClient crmSvc;
        public static MetadataCache _metadataCache;
        /// <summary>
        /// Returns a cached version of the metadata for this connection.
        /// </summary>
        /// <remarks>
        /// This cache is updated at the start of each connection, or by calling <see cref="UpdateMetadataCache(bool)"/>
        /// </remarks>
        public static EntityMetadata[] MetadataCache => _metadataCache.EntityMetadata;

        /// <summary>
        /// Returns a task that provides access to the <see cref="MetadataCache"/> once it has finished loading
        /// </summary>
        public static Task<MetadataCache> MetadataCacheLoader { get; private set; } = Task.FromResult<MetadataCache>(null);

        public static CrmServiceClient GetServiceClient(this ConnectionDetail connectionDetail)
        {
            return connectionDetail.GetCrmServiceClient();
        }
        public static void SetServiceClient(this ConnectionDetail connectionDetail, CrmServiceClient crmSvcValue)
        {
            crmSvc = crmSvcValue;
            connectionDetail.SetImpersonationCapability();
        }

        public static  List<SolutionDetail> GetSolutionsList(this ConnectionDetail connectionDetail, bool forceNewService = false)
        {
            var client = GetCrmServiceClient(connectionDetail, forceNewService);

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

            var solutions = client.RetrieveMultiple(query).Entities;
            var solutionDetails = solutions.Select(x => new SolutionDetail()
            {
                UniqueName = x.GetAttributeValue<string>("uniquename"),
                FriendlyName = x.GetAttributeValue<string>("friendlyname"),
                PublisherPrefix = x.GetAttributeValue<Microsoft.Xrm.Sdk.AliasedValue>("publisher.customizationprefix") == null ? null : x.GetAttributeValue<Microsoft.Xrm.Sdk.AliasedValue>("publisher.customizationprefix").Value.ToString(),
                SolutionId = x.Id
            }).ToList();
            return solutionDetails;
        }

        public static async Task<List<SolutionDetail>> GetSolutionsListAsync(this ConnectionDetail connectionDetail, bool forceNewService = false)
        {
            return await Task.Factory.StartNew(() => GetSolutionsList(connectionDetail, forceNewService));
        }

        public static CrmServiceClient GetCrmServiceClient(this ConnectionDetail connectionDetail, bool forceNewService = false)
        {
            if (forceNewService == false && crmSvc != null)
            {
                connectionDetail.SetImpersonationCapability();

                return crmSvc;
            }
            if (connectionDetail.Timeout.Ticks == 0)
            {
                connectionDetail.Timeout = new TimeSpan(0, 2, 0);
            }
            CrmServiceClient.MaxConnectionTimeout = connectionDetail.Timeout;

            if (connectionDetail.Certificate != null)
            {
                var cs = HandleConnectionString(connectionDetail, $"AuthType=Certificate;url={connectionDetail.OriginalUrl};thumbprint={connectionDetail.Certificate.Thumbprint};ClientId={connectionDetail.AzureAdAppId};RequireNewInstance={forceNewService}");
                crmSvc = new CrmServiceClient(cs);
            }
            else if (!string.IsNullOrEmpty(connectionDetail.ConnectionString))
            {
                var cs = HandleConnectionString(connectionDetail, connectionDetail.ConnectionString);
                crmSvc = new CrmServiceClient(cs);
            }
            else if (connectionDetail.NewAuthType == (CrmWebResourcesUpdater.DataModels.AuthenticationType)(int)AuthenticationType.ClientSecret)
            {
                var cs = HandleConnectionString(connectionDetail, $"AuthType=ClientSecret;url={connectionDetail.OriginalUrl};ClientId={connectionDetail.AzureAdAppId};ClientSecret={connectionDetail.GetClientSecret()};RequireNewInstance={forceNewService}");
                crmSvc = new CrmServiceClient(cs);
            }
            else if (connectionDetail.NewAuthType == (CrmWebResourcesUpdater.DataModels.AuthenticationType)(int)AuthenticationType.OAuth && connectionDetail.UseMfa)
            {
                var path = Path.Combine(Path.GetTempPath(), connectionDetail.ConnectionId.Value.ToString("B"));

                var cs = HandleConnectionString(connectionDetail, $"AuthType=OAuth;Username={connectionDetail.UserName};Url={connectionDetail.OriginalUrl};AppId={connectionDetail.AzureAdAppId};RedirectUri={connectionDetail.ReplyUrl};TokenCacheStorePath={path};LoginPrompt=Auto;RequireNewInstance={forceNewService}");
                crmSvc = new CrmServiceClient(cs);
            }
            else if (!string.IsNullOrEmpty(connectionDetail.GetClientSecret()))
            {
                ConnectOAuth(connectionDetail);
            }
            else if (connectionDetail.UseOnline)
            {
                ConnectOnline(connectionDetail);
            }
            else if (connectionDetail.UseIfd)
            {
                ConnectIfd(connectionDetail);
            }
            else
            {
                ConnectOnprem(connectionDetail);
            }

            if (!crmSvc.IsReady)
            {
                var error = crmSvc.LastCrmError;
                crmSvc = null;
                throw new Exception(error);
            }

            connectionDetail.SetImpersonationCapability();

            connectionDetail.OrganizationFriendlyName = crmSvc.ConnectedOrgFriendlyName;
            connectionDetail.OrganizationDataServiceUrl = crmSvc.ConnectedOrgPublishedEndpoints[EndpointType.OrganizationDataService];
            connectionDetail.OrganizationServiceUrl = crmSvc.ConnectedOrgPublishedEndpoints[EndpointType.OrganizationService];
            connectionDetail.WebApplicationUrl = crmSvc.ConnectedOrgPublishedEndpoints[EndpointType.WebApplication];
            connectionDetail.Organization = crmSvc.ConnectedOrgUniqueName;
            connectionDetail.OrganizationVersion = crmSvc.ConnectedOrgVersion.ToString();
            connectionDetail.TenantId = crmSvc.TenantId;
            connectionDetail.EnvironmentId = crmSvc.EnvironmentId;

            var webAppURi = new Uri(connectionDetail.WebApplicationUrl);
            connectionDetail.ServerName = webAppURi.Host;
            connectionDetail.ServerPort = webAppURi.Port;

            //UseIfd = crmSvc.ActiveAuthenticationType == AuthenticationType.IFD;

            switch (crmSvc.ActiveAuthenticationType)
            {
                case AuthenticationType.AD:
                case AuthenticationType.Claims:
                    connectionDetail.AuthType = (CrmWebResourcesUpdater.DataModels.AuthenticationProviderType)(int)AuthenticationProviderType.ActiveDirectory;
                    break;

                case AuthenticationType.IFD:
                    connectionDetail.AuthType = (CrmWebResourcesUpdater.DataModels.AuthenticationProviderType)(int)AuthenticationProviderType.Federation;
                    break;

                case AuthenticationType.Live:
                    connectionDetail.AuthType = (CrmWebResourcesUpdater.DataModels.AuthenticationProviderType)(int)AuthenticationProviderType.LiveId;
                    break;

                case AuthenticationType.OAuth:
                    // TODO add new property in ConnectionDetail class?
                    break;

                case AuthenticationType.Office365:
                    connectionDetail.AuthType = (CrmWebResourcesUpdater.DataModels.AuthenticationProviderType)(int)AuthenticationProviderType.OnlineFederation;
                    break;
            }

            return crmSvc;
        }

        public static async Task<CrmServiceClient> GetCrmServiceClientAsync(this ConnectionDetail connectionDetail, bool forceNewService = false)
        {
            return await Task.Factory.StartNew(() => GetCrmServiceClient(connectionDetail, forceNewService));
        }

        private static string HandleConnectionString(this ConnectionDetail connectionDetail, string connectionString)
        {
            var csb = new DbConnectionStringBuilder { ConnectionString = connectionString };
            if (csb.ContainsKey("timeout"))
            {
                var csTimeout = TimeSpan.Parse(csb["timeout"].ToString());
                csb.Remove("timeout");
                CrmServiceClient.MaxConnectionTimeout = csTimeout;
            }

            connectionDetail.OriginalUrl = csb["Url"].ToString();
            connectionDetail.UserName = csb.ContainsKey("username") ? csb["username"].ToString() :
                csb.ContainsKey("clientid") ? csb["clientid"].ToString() : null;

            if (csb.ContainsKey("Password"))
            {
                csb["Password"] = CryptoManager.Decrypt(csb["Password"].ToString(), ConnectionManager.CryptoPassPhrase,
                    ConnectionManager.CryptoSaltValue,
                    ConnectionManager.CryptoHashAlgorythm,
                    ConnectionManager.CryptoPasswordIterations,
                    ConnectionManager.CryptoInitVector,
                    ConnectionManager.CryptoKeySize);
            }
            if (csb.ContainsKey("ClientSecret"))
            {
                csb["ClientSecret"] = CryptoManager.Decrypt(csb["ClientSecret"].ToString(), ConnectionManager.CryptoPassPhrase,
                    ConnectionManager.CryptoSaltValue,
                    ConnectionManager.CryptoHashAlgorythm,
                    ConnectionManager.CryptoPasswordIterations,
                    ConnectionManager.CryptoInitVector,
                    ConnectionManager.CryptoKeySize);
            }

            var cs = csb.ToString();

            if (cs.IndexOf("RequireNewInstance=", StringComparison.Ordinal) < 0)
            {
                if (!cs.EndsWith(";"))
                {
                    cs += ";";
                }

                cs += "RequireNewInstance=True;";
            }

            return cs;
        }

        private static void ConnectIfd(this ConnectionDetail connectionDetail)
        {
            connectionDetail.AuthType = (CrmWebResourcesUpdater.DataModels.AuthenticationProviderType)(int)AuthenticationProviderType.Federation;

            if (!connectionDetail.IsCustomAuth)
            {
                crmSvc = new CrmServiceClient(CredentialCache.DefaultNetworkCredentials,
                    AuthenticationType.IFD,
                    connectionDetail.ServerName,
                    connectionDetail.ServerPort.ToString(),
                    connectionDetail.OrganizationUrlName,
                    true,
                    connectionDetail.UseSsl);
            }
            else
            {
                var password = CryptoManager.Decrypt(connectionDetail.GetUserPassword(), ConnectionManager.CryptoPassPhrase,
                    ConnectionManager.CryptoSaltValue,
                    ConnectionManager.CryptoHashAlgorythm,
                    ConnectionManager.CryptoPasswordIterations,
                    ConnectionManager.CryptoInitVector,
                    ConnectionManager.CryptoKeySize);

                crmSvc = new CrmServiceClient(connectionDetail.UserName, CrmServiceClient.MakeSecureString(password), connectionDetail.UserDomain,
                    connectionDetail.HomeRealmUrl,
                    connectionDetail.ServerName,
                    connectionDetail.ServerPort.ToString(),
                    connectionDetail.OrganizationUrlName,
                    true,
                    connectionDetail.UseSsl);
            }
        }

        private static void ConnectOAuth(this ConnectionDetail connectionDetail)
        {
            if (!string.IsNullOrEmpty(connectionDetail.RefreshToken))
            {
                CrmServiceClient.AuthOverrideHook = new RefreshTokenAuthOverride(connectionDetail);
                crmSvc = new CrmServiceClient(new Uri($"https://{connectionDetail.ServerName}:{connectionDetail.ServerPort}"), true);
                CrmServiceClient.AuthOverrideHook = null;
            }
            else
            {
                var secret = CryptoManager.Decrypt(connectionDetail.GetClientSecret(), ConnectionManager.CryptoPassPhrase,
                     ConnectionManager.CryptoSaltValue,
                     ConnectionManager.CryptoHashAlgorythm,
                     ConnectionManager.CryptoPasswordIterations,
                     ConnectionManager.CryptoInitVector,
                     ConnectionManager.CryptoKeySize);

                var path = Path.Combine(Path.GetTempPath(), connectionDetail.ConnectionId.Value.ToString("B"), "oauth-cache.txt");
                crmSvc = new CrmServiceClient(new Uri($"https://{connectionDetail.ServerName}:{connectionDetail.ServerPort}"), connectionDetail.AzureAdAppId.ToString(), CrmServiceClient.MakeSecureString(secret), true, path);
            }
        }

        private static void ConnectOnline(this ConnectionDetail connectionDetail)
        {
            connectionDetail.AuthType = (CrmWebResourcesUpdater.DataModels.AuthenticationProviderType)(int)AuthenticationProviderType.OnlineFederation;

            if (string.IsNullOrEmpty(connectionDetail.GetUserPassword()))
            {
                throw new Exception("Unable to read user password");
            }

            var password = CryptoManager.Decrypt(connectionDetail.GetUserPassword(), ConnectionManager.CryptoPassPhrase,
                 ConnectionManager.CryptoSaltValue,
                 ConnectionManager.CryptoHashAlgorythm,
                 ConnectionManager.CryptoPasswordIterations,
                 ConnectionManager.CryptoInitVector,
                 ConnectionManager.CryptoKeySize);

            Utilities.GetOrgnameAndOnlineRegionFromServiceUri(new Uri(connectionDetail.OriginalUrl), out var region, out var orgName, out _);

            //if (UseMfa)
            //{
            var path = Path.Combine(Path.GetTempPath(), connectionDetail.ConnectionId.Value.ToString("B"), "oauth-cache.txt");

            crmSvc = new CrmServiceClient(connectionDetail.UserName, CrmServiceClient.MakeSecureString(password),
                region,
                orgName,
                true,
                null,
                null,
                connectionDetail.AzureAdAppId != Guid.Empty ? connectionDetail.AzureAdAppId.ToString() : "51f81489-12ee-4a9e-aaae-a2591f45987d",
                new Uri(connectionDetail.ReplyUrl ?? "app://58145B91-0C36-4500-8554-080854F2AC97"),
                path,
                null);
            //}
            //else
            //{
            //    crmSvc = new CrmServiceClient(UserName, CrmServiceClient.MakeSecureString(password),
            //        region,
            //        orgName,
            //        true,
            //        true,
            //        null,
            //        true);
            //}
        }

        private static void ConnectOnprem(this ConnectionDetail connectionDetail)
        {
            connectionDetail.AuthType = (CrmWebResourcesUpdater.DataModels.AuthenticationProviderType)(int)AuthenticationProviderType.ActiveDirectory;

            NetworkCredential credential;
            if (!connectionDetail.IsCustomAuth)
            {
                credential = CredentialCache.DefaultNetworkCredentials;
            }
            else
            {
                var password = CryptoManager.Decrypt(connectionDetail.GetUserPassword(), ConnectionManager.CryptoPassPhrase,
                    ConnectionManager.CryptoSaltValue,
                    ConnectionManager.CryptoHashAlgorythm,
                    ConnectionManager.CryptoPasswordIterations,
                    ConnectionManager.CryptoInitVector,
                    ConnectionManager.CryptoKeySize);

                credential = new NetworkCredential(connectionDetail.UserName, password, connectionDetail.UserDomain);
            }

            crmSvc = new CrmServiceClient(credential,
                AuthenticationType.AD,
                connectionDetail.ServerName,
                connectionDetail.ServerPort.ToString(),
                connectionDetail.OrganizationUrlName,
                true,
                connectionDetail.UseSsl);
        }

        private static void SetImpersonationCapability(this ConnectionDetail connectionDetail)
        {
            var query = new QueryExpression("systemuserroles")
            {
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("systemuserid", ConditionOperator.EqualUserId)
                    }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = "systemuserroles",
                        LinkFromAttributeName = "roleid",
                        LinkToAttributeName = "roleid",
                        LinkToEntityName = "role",

                        LinkEntities =
                        {
                            new LinkEntity
                            {
                                LinkFromEntityName = "role",
                                LinkFromAttributeName = "roleid",
                                LinkToAttributeName = "roleid",
                                LinkToEntityName = "roleprivileges",
                                EntityAlias = "priv",
                                Columns = new ColumnSet("privilegedepthmask"),
                                LinkEntities =
                                {
                                    new LinkEntity
                                    {
                                        LinkFromEntityName = "roleprivileges",
                                        LinkFromAttributeName = "privilegeid",
                                        LinkToAttributeName = "privilegeid",
                                        LinkToEntityName = "privilege", LinkCriteria = new FilterExpression
                                        {
                                            Conditions =
                                            {
                                                new ConditionExpression("name", ConditionOperator.Equal, "prvActOnBehalfOfAnotherUser"),
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var privileges = crmSvc.RetrieveMultiple(query).Entities;

            connectionDetail.CanImpersonate = privileges.Any(p =>
                (int)p.GetAttributeValue<Microsoft.Xrm.Sdk.AliasedValue>("priv.privilegedepthmask").Value == 8);
        }

        #region Impersonation methods
        public static event EventHandler<ImpersonationEventArgs> OnImpersonate;

        public static void Impersonate(this ConnectionDetail connectionDetail, Guid userId, string username = null)
        {
            connectionDetail.ImpersonatedUserId = userId;
            connectionDetail.ImpersonatedUserName = username;

            connectionDetail.GetServiceClient().CallerId = userId;

            OnImpersonate?.Invoke(connectionDetail, new ImpersonationEventArgs(connectionDetail.ImpersonatedUserId, connectionDetail.ImpersonatedUserName));
        }

        public static void RemoveImpersonation(this ConnectionDetail connectionDetail)
        {
            connectionDetail.ImpersonatedUserId = Guid.Empty;
            connectionDetail.ImpersonatedUserName = null;

            connectionDetail.GetServiceClient().CallerId = Guid.Empty;

            OnImpersonate?.Invoke(connectionDetail, new ImpersonationEventArgs(connectionDetail.ImpersonatedUserId, connectionDetail.ImpersonatedUserName));
        }

        #endregion Impersonation methods

        #region Metadata Cache methods

        /// <summary>
        /// Updates the <see cref="MetadataCache"/>
        /// </summary>
        /// <param name="flush">Indicates if the existing cache should be flushed and a full new copy of the metadata should be retrieved</param>
        public static Task UpdateMetadataCache(this ConnectionDetail connectionDetail, bool flush)
        {
            if (connectionDetail.OrganizationMajorVersion < 8)
                throw new NotSupportedException("Metadata cache is only supported on Dynamics CRM 2016 or later");

            // If there's already an update in progress, don't start a new one
            if (!MetadataCacheLoader.IsCompleted)
                return MetadataCacheLoader;

            // Load the metadata in a background task
            var task = new Task<MetadataCache>(() =>
            {
                // Load & update metadata cache
                var metadataCachePath = Path.Combine(Path.GetDirectoryName(ConnectionsList.ConnectionsListFilePath), "..", "Metadata", connectionDetail.ConnectionId + ".xml.gz");
                metadataCachePath = Path.IsPathRooted(metadataCachePath) ? metadataCachePath : Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, metadataCachePath);

                var metadataSerializer = new DataContractSerializer(typeof(MetadataCache));
                var metadataCache = _metadataCache;

                if (metadataCache == null && File.Exists(metadataCachePath) && !flush)
                {
                    try
                    {
                        using (var stream = File.OpenRead(metadataCachePath))
                        using (var gz = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            metadataCache = (MetadataCache)metadataSerializer.ReadObject(gz);
                        }
                    }
                    catch
                    {
                        // If the cache file isn't readable for any reason, throw it away and download a new copy
                    }
                }

                // Get all the metadata that's changed since the last connection
                // If this query changes, increment the version number to ensure any previously cached versions are flushed
                const int queryVersion = 2;

                var metadataQuery = new RetrieveMetadataChangesRequest
                {
                    ClientVersionStamp = !flush && metadataCache?.MetadataQueryVersion == queryVersion ? metadataCache?.ClientVersionStamp : null,
                    Query = new EntityQueryExpression
                    {
                        Properties = new MetadataPropertiesExpression { AllProperties = true },
                        AttributeQuery = new AttributeQueryExpression
                        {
                            Properties = new MetadataPropertiesExpression { AllProperties = true }
                        },
                        RelationshipQuery = new RelationshipQueryExpression
                        {
                            Properties = new MetadataPropertiesExpression { AllProperties = true }
                        }
                    },
                    DeletedMetadataFilters = DeletedMetadataFilters.All
                };

                RetrieveMetadataChangesResponse metadataUpdate;

                try
                {
                    metadataUpdate = (RetrieveMetadataChangesResponse)connectionDetail.GetServiceClient().Execute(metadataQuery);
                }
                catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
                {
                    // If the last connection was too long ago, we need to request all the metadata, not just the changes
                    if (ex.Detail.ErrorCode == unchecked((int)0x80044352))
                    {
                        _metadataCache = null;
                        metadataQuery.ClientVersionStamp = null;
                        metadataUpdate = (RetrieveMetadataChangesResponse)connectionDetail.GetServiceClient().Execute(metadataQuery);
                    }
                    else
                    {
                        throw;
                    }
                }

                if (metadataCache == null || flush)
                {
                    // If we didn't have a previous cache, just start a fresh one
                    metadataCache = new MetadataCache();
                    metadataCache.EntityMetadata = metadataUpdate.EntityMetadata.ToArray();
                    metadataCache.MetadataQueryVersion = queryVersion;
                }
                else
                {
                    // We had a cached version of the metadata before, so now we need to merge in the changes
                    var deletedIds = metadataUpdate.DeletedMetadata == null ? new List<Guid>() : metadataUpdate.DeletedMetadata.SelectMany(kvp => kvp.Value).Distinct().ToList();

                    connectionDetail.CopyChanges(metadataCache, typeof(MetadataCache).GetProperty(nameof(Utils.MetadataCache.EntityMetadata)), metadataUpdate.EntityMetadata.ToArray(), deletedIds);
                }

                _metadataCache = metadataCache;

                // Save the latest metadata cache
                if (metadataCache.ClientVersionStamp != metadataUpdate.ServerVersionStamp ||
                    metadataCache.MetadataQueryVersion != queryVersion)
                {
                    metadataCache.ClientVersionStamp = metadataUpdate.ServerVersionStamp;
                    metadataCache.MetadataQueryVersion = queryVersion;

                    Directory.CreateDirectory(Path.GetDirectoryName(metadataCachePath));

                    using (var stream = File.Create(metadataCachePath))
                    using (var gz = new GZipStream(stream, CompressionLevel.Optimal))
                    {
                        metadataSerializer.WriteObject(gz, metadataCache);
                    }
                }

                return metadataCache;
            });
            task.ConfigureAwait(false);

            // Store the current metadata loading task and run it
            MetadataCacheLoader = task;
            task.Start();
            return task;
        }

        private static void CopyChanges(this ConnectionDetail connectionDetail, object source, PropertyInfo sourceProperty, MetadataBase[] newArray, List<Guid> deletedIds)
        {
            var existingArray = (MetadataBase[])sourceProperty.GetValue(source);
            var existingList = new List<MetadataBase>(existingArray);

            // Add any new items and update any modified ones
            foreach (var newItem in newArray)
            {
                var existingItem = existingList.SingleOrDefault(e => e.MetadataId == newItem.MetadataId);

                if (existingItem == null)
                    existingList.Add(newItem);
                else
                    connectionDetail.CopyChanges(existingItem, newItem, deletedIds);
            }

            // Store the new array
            var updatedArray = Array.CreateInstance(sourceProperty.PropertyType.GetElementType(), existingList.Count);

            for (var i = 0; i < existingList.Count; i++)
                updatedArray.SetValue(existingList[i], i);

            sourceProperty.SetValue(source, updatedArray);

            if (deletedIds.Count > 0)
                connectionDetail.RemoveDeletedItems(source, sourceProperty, deletedIds);
        }

        private static void CopyChanges(this ConnectionDetail connectionDetail, MetadataBase existingItem, MetadataBase newItem, List<Guid> deletedIds)
        {
            foreach (var prop in existingItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!Attribute.IsDefined(prop, typeof(DataMemberAttribute)))
                    continue;

                var newValue = prop.GetValue(newItem);
                var existingValue = prop.GetValue(existingItem) as MetadataBase;
                var type = prop.PropertyType;

                if (type.IsArray)
                    type = type.GetElementType();

                if (!typeof(MetadataBase).IsAssignableFrom(type) && !typeof(Label).IsAssignableFrom(type))
                {
                    if (newItem.HasChanged != false)
                        prop.SetValue(existingItem, newValue);
                }
                else if (typeof(Label).IsAssignableFrom(type))
                {
                    if (newItem.HasChanged != false)
                        connectionDetail.CopyChanges((Label)prop.GetValue(existingItem), (Label)newValue, deletedIds);
                }
                else if (newValue != null)
                {
                    if (prop.PropertyType.IsArray)
                    {
                        connectionDetail.CopyChanges(existingItem, prop, (MetadataBase[])newValue, deletedIds);
                    }
                    else
                    {
                        if (existingValue.MetadataId == ((MetadataBase)newValue).MetadataId)
                            connectionDetail.CopyChanges(existingValue, (MetadataBase)newValue, deletedIds);
                        else
                            prop.SetValue(existingItem, newValue);
                    }
                }
                else if (existingValue != null && deletedIds.Contains(existingValue.MetadataId.Value))
                {
                    prop.SetValue(existingItem, null);
                }
            }
        }

        private static void CopyChanges(this ConnectionDetail connectionDetail, Label existingLabel, Label newLabel, List<Guid> deletedIds)
        {
            if (newLabel == null)
                return;

            foreach (var localizedLabel in newLabel.LocalizedLabels)
            {
                var existingLocalizedLabel = existingLabel.LocalizedLabels.SingleOrDefault(ll => ll.MetadataId == localizedLabel.MetadataId);

                if (existingLocalizedLabel == null)
                    existingLabel.LocalizedLabels.Add(localizedLabel);
                else
                    connectionDetail.CopyChanges(existingLocalizedLabel, localizedLabel, deletedIds);
            }

            for (var i = existingLabel.LocalizedLabels.Count - 1; i >= 0; i--)
            {
                if (deletedIds.Contains(existingLabel.LocalizedLabels[i].MetadataId.Value))
                    existingLabel.LocalizedLabels.RemoveAt(i);
            }

            if (newLabel.UserLocalizedLabel != null)
                connectionDetail.CopyChanges(existingLabel.UserLocalizedLabel, newLabel.UserLocalizedLabel, deletedIds);
        }

        private static void RemoveDeletedItems(this ConnectionDetail connectionDetail, object source, PropertyInfo sourceProperty, List<Guid> deletedIds)
        {
            var existingArray = (MetadataBase[])sourceProperty.GetValue(source);
            var existingList = new List<MetadataBase>(existingArray);

            // Remove any deleted items
            existingList.RemoveAll(e => deletedIds.Contains(e.MetadataId.Value));

            // Recursively delete any sub-items
            foreach (var item in existingList)
                connectionDetail.RemoveDeletedItems(item, deletedIds);

            // Store the new array
            var updatedArray = Array.CreateInstance(sourceProperty.PropertyType.GetElementType(), existingList.Count);

            for (var i = 0; i < existingList.Count; i++)
                updatedArray.SetValue(existingList[i], i);

            sourceProperty.SetValue(source, updatedArray);
        }

        private static void RemoveDeletedItems(this ConnectionDetail connectionDetail, MetadataBase item, List<Guid> deletedIds)
        {
            foreach (var prop in item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!Attribute.IsDefined(prop, typeof(DataMemberAttribute)))
                    continue;

                var value = prop.GetValue(item);
                var childItem = value as MetadataBase;
                var type = prop.PropertyType;

                if (type.IsArray)
                    type = type.GetElementType();

                if (value is Label label)
                {
                    connectionDetail.RemoveDeletedItems(label, deletedIds);
                }
                else if (childItem != null && childItem.MetadataId != null && deletedIds.Contains(childItem.MetadataId.Value))
                {
                    prop.SetValue(item, null);
                }
                else if (childItem != null)
                {
                    connectionDetail.RemoveDeletedItems(childItem, deletedIds);
                }
                else if (value != null && prop.PropertyType.IsArray && typeof(MetadataBase).IsAssignableFrom(type))
                {
                    connectionDetail.RemoveDeletedItems(item, prop, deletedIds);
                }
            }
        }

        private static void RemoveDeletedItems(this ConnectionDetail connectionDetail, Label label, List<Guid> deletedIds)
        {
            if (label == null)
                return;

            for (var i = label.LocalizedLabels.Count - 1; i >= 0; i--)
            {
                if (deletedIds.Contains(label.LocalizedLabels[i].MetadataId.Value))
                    label.LocalizedLabels.RemoveAt(i);
            }

            if (label.UserLocalizedLabel != null)
                connectionDetail.RemoveDeletedItems(label.UserLocalizedLabel, deletedIds);
        }

        #endregion Metadata Cache methods
    }
}
