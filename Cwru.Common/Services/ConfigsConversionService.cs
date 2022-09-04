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
        const string ExtendedLogPropertyName = "ExtendedLog";
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
                        SelectedEnvironmentId = settingsStore.GetGuid(collectionPath, SelectedConnectionIdPropertyName),
                        PublishAfterUpload = settingsStore.GetBoolOrDefault(collectionPath, AutoPublishPropertyName),
                        IgnoreExtensions = settingsStore.GetBoolean(collectionPath, IgnoreExtensionsPropertyName),
                        ExtendedLog = settingsStore.GetBoolean(collectionPath, ExtendedLogPropertyName),
                        Version = "2"
                    };

                    settingsStore.DeleteCollection(collectionPath);

                    return projectConfig;
                }
                catch (Exception ex)
                {
                    await logger.WriteAsync("Error occured during connections conversion", ex);
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
                        await logger.WriteAsync("Failed to cleanup settings store after unsuccessful conversion", ex2);
                    }
                }
            }

            return null;
        }

        //DEPLOY: not for production use:
        internal void UploadOldConfig(Guid projectGuid)
        {
            var collectionPath = GetCollectionPath(projectGuid);
            if (settingsStore.CollectionExists(collectionPath))
            {
                settingsStore.DeleteCollection(collectionPath);
                settingsStore.CreateCollection(collectionPath);
            }

            settingsStore.SetString(collectionPath, SettingsVersionPropertyName, "1");
            settingsStore.SetString(collectionPath, ConnectionsPropertyName, configXml);
            settingsStore.SetString(collectionPath, SelectedConnectionIdPropertyName, "bc68cbb7-8f5b-4976-8f24-f1a905ede08f");
            settingsStore.SetBoolean(collectionPath, AutoPublishPropertyName, true);
            settingsStore.SetBoolean(collectionPath, IgnoreExtensionsPropertyName, true);
            settingsStore.SetBoolean(collectionPath, ExtendedLogPropertyName, false);
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
                    await logger.WriteAsync("Error occured during connection detail conversion:");
                    await logger.WriteAsync(ex);
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
                connectionString = DecriptConnectionString(connectionString);
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

            //TODO: test IFD and AD with Integrated security and without
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
                await logger.WriteAsync(ex);
            }

            return null;
        }

        private string DecriptConnectionString(string connectionString)
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
                return connectionString;
            }
        }

        private string GetCollectionPath(Guid projectId)
        {
            return CollectionBasePath + "_" + projectId.ToString();
        }

        private const string configXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfConnectionDetail xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <ConnectionDetail>
    <AuthType>None</AuthType>
    <AzureAdAppId>00000000-0000-0000-0000-000000000000</AzureAdAppId>
    <ConnectionId>bc68cbb7-8f5b-4976-8f24-f1a905ede08f</ConnectionId>
    <ConnectionName>notal-us-dev</ConnectionName>
    <ConnectionString>authtype=ClientSecret;url=https://notaldev1.crm.dynamics.com/;clientid=707f5e78-cdf6-4e04-a678-36d16cee861a;clientsecret=eIslNhtXhjsyeyj5gSsNucR6HixPhwJ+tQt2CuYfk77I6fe6xj1rZ92Qvz7AkorI</ConnectionString>
    <EnvironmentId>d4b38a4d-e688-4e9b-abcc-12292549265d</EnvironmentId>
    <IsCustomAuth>false</IsCustomAuth>
    <IsFromSdkLoginCtrl>false</IsFromSdkLoginCtrl>
    <LastUsedOn>0001-01-01 00:00:00</LastUsedOn>
    <NewAuthType>AD</NewAuthType>
    <Organization>orgb5f49dde</Organization>
    <OrganizationDataServiceUrl>https://notaldev1.api.crm.dynamics.com/XRMServices/2011/OrganizationData.svc</OrganizationDataServiceUrl>
    <OrganizationFriendlyName>NotalDev1</OrganizationFriendlyName>
    <OrganizationServiceUrl>https://notaldev1.api.crm.dynamics.com/XRMServices/2011/Organization.svc</OrganizationServiceUrl>
    <OrganizationVersion>9.2.22073.190</OrganizationVersion>
    <OriginalUrl>https://notaldev1.crm.dynamics.com/</OriginalUrl>
    <SavePassword>false</SavePassword>
    <ServerName>notaldev1.crm.dynamics.com</ServerName>
    <ServerPort>443</ServerPort>
    <TenantId>29014319-16b0-40a0-bcd4-fcfcbd8c1a3a</TenantId>
    <Timeout />
    <TimeoutTicks>0</TimeoutTicks>
    <UseIfd>false</UseIfd>
    <UseMfa>false</UseMfa>
    <UserName>707f5e78-cdf6-4e04-a678-36d16cee861a</UserName>
    <WebApplicationUrl>https://notaldev1.crm.dynamics.com/</WebApplicationUrl>
    <SelectedSolution>
      <SolutionId>fd140aaf-4df4-11dd-bd17-0019b9312238</SolutionId>
      <FriendlyName>Default Solution</FriendlyName>
      <UniqueName>Default</UniqueName>
      <PublisherPrefix>new</PublisherPrefix>
    </SelectedSolution>
  </ConnectionDetail>
  <ConnectionDetail>
    <AuthType>None</AuthType>
    <AzureAdAppId>00000000-0000-0000-0000-000000000000</AzureAdAppId>
    <ConnectionId>f02d59e2-08b4-4738-8f63-bfb122bd6b4f</ConnectionId>
    <ConnectionName>notal-us-uat</ConnectionName>
    <ConnectionString>authtype=ClientSecret;url=https://notaluat.crm.dynamics.com/;clientid=307bc0a5-ec04-4eb4-ae8b-18accfb1fc99;clientsecret=EbbcG09FSonApFY95GNNyTDg494Ahjv9+lDYcrXP65O5B5lgCgDKyrELPBNWieGb</ConnectionString>
    <EnvironmentId>8df75b46-8998-4022-a3f7-fd254265a3ed</EnvironmentId>
    <IsCustomAuth>false</IsCustomAuth>
    <IsFromSdkLoginCtrl>false</IsFromSdkLoginCtrl>
    <LastUsedOn>0001-01-01 00:00:00</LastUsedOn>
    <NewAuthType>AD</NewAuthType>
    <Organization>org1cdbb188</Organization>
    <OrganizationDataServiceUrl>https://notaluat.api.crm.dynamics.com/XRMServices/2011/OrganizationData.svc</OrganizationDataServiceUrl>
    <OrganizationFriendlyName>Notal UAT</OrganizationFriendlyName>
    <OrganizationServiceUrl>https://notaluat.api.crm.dynamics.com/XRMServices/2011/Organization.svc</OrganizationServiceUrl>
    <OrganizationVersion>9.2.22073.190</OrganizationVersion>
    <OriginalUrl>https://notaluat.crm.dynamics.com/</OriginalUrl>
    <SavePassword>false</SavePassword>
    <ServerName>notaluat.crm.dynamics.com</ServerName>
    <ServerPort>443</ServerPort>
    <TenantId>29014319-16b0-40a0-bcd4-fcfcbd8c1a3a</TenantId>
    <Timeout />
    <TimeoutTicks>0</TimeoutTicks>
    <UseIfd>false</UseIfd>
    <UseMfa>false</UseMfa>
    <UserName>307bc0a5-ec04-4eb4-ae8b-18accfb1fc99</UserName>
    <WebApplicationUrl>https://notaluat.crm.dynamics.com/</WebApplicationUrl>
    <SelectedSolution>
      <SolutionId>fd140aaf-4df4-11dd-bd17-0019b9312238</SolutionId>
      <FriendlyName>Default Solution</FriendlyName>
      <UniqueName>Default</UniqueName>
      <PublisherPrefix>new</PublisherPrefix>
    </SelectedSolution>
  </ConnectionDetail>
  <ConnectionDetail>
    <AuthType>None</AuthType>
    <AzureAdAppId>00000000-0000-0000-0000-000000000000</AzureAdAppId>
    <ConnectionId>9901d5a1-4ca3-447a-84c8-d69bae851674</ConnectionId>
    <ConnectionName>notal-us-prod</ConnectionName>
    <IsCustomAuth>true</IsCustomAuth>
    <IsFromSdkLoginCtrl>false</IsFromSdkLoginCtrl>
    <LastUsedOn>0001-01-01 00:00:00</LastUsedOn>
    <NewAuthType>AD</NewAuthType>
    <Organization>org8f7ae095</Organization>
    <OrganizationDataServiceUrl>https://notal.api.crm.dynamics.com/api/data/v9.2/</OrganizationDataServiceUrl>
    <OrganizationFriendlyName>NotalVisionProd</OrganizationFriendlyName>
    <OrganizationServiceUrl>https://notal.api.crm.dynamics.com/XRMServices/2011/Organization.svc</OrganizationServiceUrl>
    <OrganizationUrlName>notal</OrganizationUrlName>
    <OrganizationVersion>9.2.22073.190</OrganizationVersion>
    <OriginalUrl>https://notal.crm.dynamics.com</OriginalUrl>
    <SavePassword>false</SavePassword>
    <ServerName>notal.crm.dynamics.com</ServerName>
    <ServerPort>443</ServerPort>
    <TenantId>00000000-0000-0000-0000-000000000000</TenantId>
    <Timeout />
    <TimeoutTicks>1200000000</TimeoutTicks>
    <UseIfd>false</UseIfd>
    <UseMfa>false</UseMfa>
    <UserDomain />
    <UserName>MDeykun@notalvision.com</UserName>
    <WebApplicationUrl>https://notal.crm.dynamics.com</WebApplicationUrl>
    <SelectedSolution>
      <SolutionId>fd140aaf-4df4-11dd-bd17-0019b9312238</SolutionId>
      <FriendlyName>Default Solution</FriendlyName>
      <UniqueName>Default</UniqueName>
      <PublisherPrefix>new</PublisherPrefix>
    </SelectedSolution>
  </ConnectionDetail>
</ArrayOfConnectionDetail>";
    }
}
