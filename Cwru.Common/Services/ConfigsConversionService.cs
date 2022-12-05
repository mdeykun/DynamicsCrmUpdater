using Cwru.Common.Config;
using Cwru.Common.Config.Depricated;
using Cwru.Common.Extensions;
using Cwru.Common.Model;
using Microsoft.VisualStudio.Settings;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Cwru.Common.Services
{
    public class ConfigsConversionService
    {
        private readonly CryptoService cryptoService;
        private readonly XmlSerializerService xmlSerializerService;
        private readonly WritableSettingsStore settingsStore;
        private readonly ILogger logger;

        const string CollectionBasePath = "CrmPublisherSettings";
        const string ConnectionsPropertyName = "Connections";
        const string SelectedConnectionIdPropertyName = "SelectedConnectionId";
        const string AutoPublishPropertyName = "AutoPublishEnabled";
        const string IgnoreExtensionsPropertyName = "IgnoreExtensions";
        const string SettingsVersionPropertyName = "SettingsVersion";

        public ConfigsConversionService(
            ILogger logger,
            CryptoService cryptoService,
            XmlSerializerService xmlSerializerService,
            WritableSettingsStore settingsStore)
        {
            this.cryptoService = cryptoService;
            this.xmlSerializerService = xmlSerializerService;
            this.settingsStore = settingsStore;
            this.logger = logger;
        }

        public async Task<ProjectConfig> ConvertSettingsAsync(Guid projectId)
        {
            var collectionPath = GetCollectionPath(projectId);
            if (!settingsStore.CollectionExists(collectionPath))
            {
                return null;
            }

            var configVersion = settingsStore.GetStringOrDefault(collectionPath, SettingsVersionPropertyName);
            if (configVersion == null)
            {
                await logger.WriteLineAsync("Conversion of configuration (version 0) to configuration (version 2) is not supported.");
                settingsStore.DeleteCollection(collectionPath);
                return null;
            }
            else if (configVersion == "2")
            {
                return null;
            }
            else if (configVersion == "1")
            {
                try
                {
                    var connectionsXml = settingsStore.GetString(collectionPath, ConnectionsPropertyName);

                    var environmentsConfigs = await ConvertFromXrmCrmConnectionsAsync(connectionsXml);
                    if (environmentsConfigs.Count() == 0)
                    {
                        settingsStore.DeleteCollection(collectionPath);
                        return null;
                    }

                    var projectConfig = new ProjectConfig()
                    {
                        ProjectId = projectId,
                        Environments = environmentsConfigs.ToList(),
                        DafaultEnvironmentId = settingsStore.GetGuid(collectionPath, SelectedConnectionIdPropertyName),
                        PublishAfterUpload = settingsStore.GetBoolOrDefault(collectionPath, AutoPublishPropertyName),
                        IgnoreExtensions = settingsStore.GetBoolean(collectionPath, IgnoreExtensionsPropertyName),
                        Version = "2"
                    };

                    settingsStore.DeleteCollection(collectionPath);

                    return projectConfig;
                }
                catch (Exception ex)
                {
                    await logger.WriteLineAsync("Error occured during connections conversion", ex);
                    return null;
                }
                finally
                {
                    try
                    {
                        settingsStore.DeleteCollection(collectionPath);
                    }
                    catch (Exception ex2)
                    {
                        await logger.WriteLineAsync("Failed to cleanup settings store after unsuccessful conversion", ex2);
                    }
                }
            }

            return null;
        }

        public async Task<List<EnvironmentConfig>> ConvertFromXrmCrmConnectionsAsync(string connectionsXml)
        {
            var deprecatedConnections = (List<ConnectionDetail>)xmlSerializerService.Deserialize(connectionsXml, typeof(List<ConnectionDetail>));
            return await ConvertFromXrmCrmConnectionsAsync(deprecatedConnections);
        }

        public async Task<List<EnvironmentConfig>> ConvertFromXrmCrmConnectionsAsync(List<ConnectionDetail> connectionDetails)
        {
            var environmentConfigs = new List<EnvironmentConfig>();

            foreach (var connection in connectionDetails)
            {
                try
                {
                    var config = new EnvironmentConfig()
                    {
                        ConnectionString = await ToCrmConnectionStringAsync(connection),
                        IsUserProvidedConnectionString = connection.ConnectionString != null,
                        Id = connection.ConnectionId.Value,
                        SelectedSolutionId = connection.SelectedSolution?.SolutionId != null ? connection.SelectedSolution.SolutionId : throw new Exception("Selected solution Id is null"),
                        Certificate = connection.Certificate != null ? new Certificate()
                        {
                            Issuer = connection.Certificate.Issuer,
                            Name = connection.Certificate.Name
                        } : null,
                        SavePassword = connection.SavePassword,
                        TimeoutTicks = connection.Timeout.Ticks,
                        Name = connection.ConnectionName,
                        Organization = connection.Organization,
                        OrganizationVersion = connection.OrganizationVersion,
                        SolutionName = connection.SelectedSolution?.FriendlyName,
                    };

                    environmentConfigs.Add(config);
                }
                catch (Exception ex)
                {
                    await logger.WriteLineAsync("Error occured during connection detail conversion:");
                    await logger.WriteLineAsync(ex);
                }
            }

            return environmentConfigs;
        }

        private async Task<CrmConnectionString> ToCrmConnectionStringAsync(ConnectionDetail connectionDetail, bool? forceNewService = null)
        {
            if (connectionDetail.Certificate != null)
            {
                return new CrmConnectionString()
                {
                    AuthenticationType = AuthenticationType.Certificate,
                    ServiceUri = connectionDetail.OriginalUrl,
                    Thumbprint = connectionDetail.Certificate.Thumbprint,
                    ClientId = connectionDetail.AzureAdAppId,
                    RequireNewInstance = forceNewService,
                    LoginPrompt = "None"
                };
            }

            if (connectionDetail.ConnectionString != null)
            {
                var connectionString = connectionDetail.ConnectionString;
                connectionString = await DecriptConnectionStringAsync(connectionString);
                return CrmConnectionString.Parse(connectionString);
            }

            if (connectionDetail.NewAuthType == AuthenticationType.ClientSecret)
            {
                return new CrmConnectionString()
                {
                    AuthenticationType = AuthenticationType.ClientSecret,
                    ServiceUri = connectionDetail.OriginalUrl,
                    ClientId = connectionDetail.AzureAdAppId,
                    ClientSecret = await DecryptOrNullAsync(connectionDetail.ClientSecretEncrypted),
                    RequireNewInstance = forceNewService,
                    LoginPrompt = "None"
                };
            }

            if (connectionDetail.NewAuthType == AuthenticationType.OAuth && connectionDetail.UseMfa)
            {
                var path = Path.Combine(Path.GetTempPath(), connectionDetail.ConnectionId.Value.ToString("B"));

                return new CrmConnectionString()
                {
                    AuthenticationType = AuthenticationType.OAuth,
                    ServiceUri = connectionDetail.OriginalUrl,
                    UserName = connectionDetail.UserName,
                    ClientId = connectionDetail.AzureAdAppId,
                    RedirectUri = connectionDetail.ReplyUrl,
                    TokenCacheStorePath = path,
                    RequireNewInstance = forceNewService,
                    LoginPrompt = "None"
                };
            }

            if (connectionDetail.UseOnline)
            {
                var path = Path.Combine(Path.GetTempPath(), connectionDetail.ConnectionId.Value.ToString("B"), "oauth-cache.txt");

                return new CrmConnectionString()
                {
                    AuthenticationType = AuthenticationType.OAuth,
                    ServiceUri = connectionDetail.OriginalUrl,
                    UserName = connectionDetail.UserName,
                    Password = await DecryptOrNullAsync(connectionDetail.UserPasswordEncrypted),
                    ClientId = connectionDetail.AzureAdAppId != Guid.Empty ? connectionDetail.AzureAdAppId : new Guid("51f81489-12ee-4a9e-aaae-a2591f45987d"),
                    RedirectUri = connectionDetail.ReplyUrl ?? "app://58145B91-0C36-4500-8554-080854F2AC97",
                    TokenCacheStorePath = path,
                    RequireNewInstance = forceNewService,
                    LoginPrompt = "None"
                };
            }

            if (connectionDetail.UseIfd)
            {
                if (connectionDetail.IsCustomAuth == false)
                {
                    return new CrmConnectionString()
                    {
                        AuthenticationType = AuthenticationType.IFD,
                        ServiceUri = connectionDetail.OriginalUrl,
                        RequireNewInstance = forceNewService,
                        LoginPrompt = "None"
                    };
                }
                else
                {
                    return new CrmConnectionString()
                    {
                        AuthenticationType = AuthenticationType.IFD,
                        ServiceUri = connectionDetail.OriginalUrl,
                        UserName = connectionDetail.UserName,
                        Domain = connectionDetail.UserDomain,
                        Password = await DecryptOrNullAsync(connectionDetail.UserPasswordEncrypted),
                        HomeRealmUri = connectionDetail.HomeRealmUrl,
                        RequireNewInstance = forceNewService,
                        LoginPrompt = "None"
                    };
                }
            }

            var cs = new CrmConnectionString()
            {
                AuthenticationType = AuthenticationType.AD,
                ServiceUri = connectionDetail.OriginalUrl,
                IntegratedSecurity = true,
                LoginPrompt = "None",

            };

            if (connectionDetail.IsCustomAuth == true)
            {
                cs.Domain = connectionDetail.UserDomain;
                cs.UserName = connectionDetail.UserName;
                cs.Password = await DecryptOrNullAsync(connectionDetail.UserPasswordEncrypted);
            }

            return cs;
        }

        private async Task<SecureString> DecryptOrNullAsync(string encrypted)
        {
            try
            {
                var encrypted2 = encrypted != null ? cryptoService.DecryptString(encrypted) : null;
                return encrypted2 != null ? cryptoService.Decrypt(encrypted2, "1").ToSecureString() : null;
            }
            catch (Exception ex)
            {
                await logger.WriteLineAsync(ex);
            }

            return null;
        }

        private async Task<string> DecriptConnectionStringAsync(string connectionString)
        {
            try
            {
                var csb = new DbConnectionStringBuilder { ConnectionString = connectionString };

                if (csb.ContainsKey("Password"))
                {
                    csb["Password"] = cryptoService.Decrypt(csb["Password"].ToString(), "1");
                }

                if (csb.ContainsKey("ClientSecret"))
                {
                    csb["ClientSecret"] = cryptoService.Decrypt(csb["ClientSecret"].ToString(), "1");
                }

                return csb.ToString();
            }
            catch (Exception ex)
            {
                await logger.WriteDebugAsync(ex);
                return connectionString;
            }
        }

        private string GetCollectionPath(Guid projectId)
        {
            return CollectionBasePath + "_" + projectId.ToString();
        }
    }
}
