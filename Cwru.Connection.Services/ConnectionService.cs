using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Extensions;
using Cwru.Common.Model;
using Cwru.Common.Services;
using Cwru.CrmRequests.Common.Contracts;
using McTools.Xrm.Connection.WinForms;
using McTools.Xrm.Connection.WinForms.Extensions;
using McTools.Xrm.Connection.WinForms.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cwru.Connection.Services
{
    public class ConnectionService
    {
        private readonly Logger logger;
        private readonly VsDteService vsDteHelper;
        private readonly MappingService mappingHelper;
        private readonly ICrmRequests crmRequestsClient;
        private readonly ConfigurationService configurationService;

        public ConnectionService(
            Logger logger,
            ICrmRequests crmRequestsClient,
            VsDteService vsDteHelper,
            MappingService mappingHelper,
            ConfigurationService configurationService)
        {
            this.logger = logger;
            this.crmRequestsClient = crmRequestsClient;
            this.vsDteHelper = vsDteHelper;
            this.mappingHelper = mappingHelper;
            this.configurationService = configurationService;
        }

        public async Task<DialogResult> ShowConfigurationDialogAsync()
        {
            var projectConfig = await configurationService.GetProjectConfigAsync();
            var project = await vsDteHelper.GetSelectedProjectInfoAsync();

            var selector = new ConnectionSelector(
                crmRequestsClient,
                ConvertToXrmConnectionDetail(projectConfig));

            selector.OnCreateMappingFile = async () =>
            {
                await mappingHelper.CreateMappingFileAsync(project.Guid, project.Root, project.Files);
                MessageBox.Show("UploaderMapping.config successfully created", "Microsoft Dynamics CRM Web Resources Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            selector.ShowDialog();

            if (selector.DialogResult == DialogResult.OK || selector.DialogResult == DialogResult.Yes)
            {
                projectConfig.Environments = ConvertFromXrmCrmConnections(selector.connectionsList);
                projectConfig.ExtendedLog = selector.connectionsList.ExtendedLog;
                projectConfig.PublishAfterUpload = selector.connectionsList.PublishAfterUpload;
                projectConfig.IgnoreExtensions = selector.connectionsList.IgnoreExtensions;
                projectConfig.SelectedEnvironmentId = selector.connectionsList.SelectedConnectionId;

                configurationService.Save(projectConfig);
            }

            return selector.DialogResult;
        }

        private List<EnvironmentConfig> ConvertFromXrmCrmConnections(ConnectionDetailsList connectionList)
        {
            var environmentConfigs = new List<EnvironmentConfig>();
            foreach (var connection in connectionList.Connections)
            {
                var config = new EnvironmentConfig()
                {
                    ConnectionString = connection.UseConnectionString ?
                        CrmConnectionString.Parse(connection.ConnectionString) :
                        connection.ToCrmConnectionString(),
                    IsUserProvidedConnectionString = connection.UseConnectionString,
                    Id = connection.ConnectionId.Value,
                    SelectedSolutionId = connection.SelectedSolutionId != null ? connection.SelectedSolutionId.Value : throw new Exception("Selected solution Id is null"),
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
                    SolutionName = connection.SolutionName,
                };

                environmentConfigs.Add(config);
            }

            return environmentConfigs;
        }

        public async Task<bool> EnsurePasswordIsSet()
        {
            var projectConfig = await configurationService.GetProjectConfigAsync();
            var environmentConfig = projectConfig.GetSelectedEnvironment();

            if (environmentConfig == null || environmentConfig.ConnectionString == null)
            {
                if (ShowErrorDialog() == DialogResult.Yes)
                {
                    var result = await ShowConfigurationDialogAsync();
                    if (result != DialogResult.OK)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (projectConfig.SelectedEnvironmentId == null)
            {
                await logger.WriteLineAsync("Error: Connection is not selected");
                return false;
            }

            environmentConfig = projectConfig.GetSelectedEnvironment();

            if (environmentConfig.ConnectionString.IntegratedSecurity != true &&
                environmentConfig.ConnectionString.AuthenticationType != AuthenticationType.Certificate &&
                environmentConfig.ConnectionString.UserName != null)
            {
                if (environmentConfig.ConnectionString.Password != null)
                {
                    return true;
                }
                else
                {
                    var result = await ShowUpdatePasswordDialogAsync();
                    if (result == DialogResult.OK)
                    {
                        return true;
                    }
                }
            }

            if (environmentConfig.ConnectionString.AuthenticationType == AuthenticationType.ClientSecret &&
                environmentConfig.ConnectionString.ClientId != null)
            {
                if (environmentConfig.ConnectionString.ClientSecret != null)
                {
                    return true;
                }
                else
                {
                    var result = await ShowUpdateClientSecretDialogAsync();
                    if (result == DialogResult.OK)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task<DialogResult> ShowUpdatePasswordDialogAsync()
        {
            var projectConfig = await configurationService.GetProjectConfigAsync();
            var environmentConfig = projectConfig.GetSelectedEnvironment();

            var dialog = new PasswordForm(environmentConfig.Name)
            {
                UserLogin = environmentConfig.ConnectionString.UserName,
                UserDomain = environmentConfig.ConnectionString.Domain,
            };

            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                environmentConfig.ConnectionString.Password = dialog.UserPassword;
                if (dialog.SavePassword)
                {
                    environmentConfig.SavePassword = true;
                    configurationService.Save(projectConfig);
                }
            }
            return dialog.DialogResult;
        }

        public async Task<DialogResult> ShowUpdateClientSecretDialogAsync()
        {
            var projectConfig = await configurationService.GetProjectConfigAsync();
            var environmentConfig = projectConfig.GetSelectedEnvironment();
            var dialog = new SecretForm(environmentConfig.Name)
            {
                ClientId = environmentConfig.ConnectionString.ClientId?.ToString("B")
            };

            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                environmentConfig.ConnectionString.ClientSecret = dialog.ClientSecret;
                if (dialog.SaveSecret)
                {
                    environmentConfig.SavePassword = true;
                    configurationService.Save(projectConfig);
                }
            }

            return dialog.DialogResult;
        }

        private ConnectionDetailsList ConvertToXrmConnectionDetail(ProjectConfig projectConfig)
        {
            var connections = new List<ConnectionDetail>();
            if (projectConfig != null && projectConfig.Environments != null)
            {
                foreach (var environmentConfig in projectConfig.Environments)
                {
                    var connection = new ConnectionDetail()
                    {
                        AzureAdAppId = environmentConfig.ConnectionString.ClientId ?? Guid.Empty,
                        Certificate = environmentConfig.Certificate != null ? new CertificateInfo()
                        {
                            Thumbprint = environmentConfig.ConnectionString.Thumbprint,
                            Issuer = environmentConfig.Certificate.Issuer,
                            Name = environmentConfig.Certificate.Name
                        } : null,
                        ClientSecret = environmentConfig.ConnectionString.ClientSecret,
                        ConnectionId = environmentConfig.Id,
                        ConnectionName = environmentConfig.Name,
                        ConnectionString = environmentConfig.IsUserProvidedConnectionString ? environmentConfig.ConnectionString.ToString() : null,
                        HomeRealmUrl = string.Empty,
                        IntegratedSecurity = environmentConfig.ConnectionString.IntegratedSecurity == true,
                        AuthType = environmentConfig.ConnectionString.AuthenticationType ?? AuthenticationType.AD,
                        Organization = environmentConfig.Organization,
                        OrganizationVersion = environmentConfig.OrganizationVersion,
                        ServerName = environmentConfig.ConnectionString.ServiceUri,
                        OriginalUrl = environmentConfig.ConnectionString.ServiceUri,
                        UserName = environmentConfig.ConnectionString.UserName,
                        UserDomain = environmentConfig.ConnectionString.Domain,
                        UseIfd = environmentConfig.ConnectionString.AuthenticationType == AuthenticationType.IFD,
                        UseMfa = false,
                        SelectedSolutionId = environmentConfig.SelectedSolutionId,
                        SolutionName = environmentConfig.SolutionName,
                        SavePassword = environmentConfig.SavePassword,
                        UserPassword = environmentConfig.ConnectionString.Password
                    };

                    connections.Add(connection);
                }
            }

            return new ConnectionDetailsList(connections)
            {
                ExtendedLog = projectConfig.ExtendedLog,
                IgnoreExtensions = projectConfig.IgnoreExtensions,
                PublishAfterUpload = projectConfig.PublishAfterUpload,
                SelectedConnectionId = projectConfig.SelectedEnvironmentId
            };
        }

        public DialogResult ShowErrorDialog()
        {
            var title = "Configuration error";
            var text = "It seems that Publisher has not been configured yet or connection is not selected.\r\n\r\n" +
            "We can open configuration window for you now or you can do it later by clicking \"Publish options\" in the context menu of the project.\r\n\r\n" +
            "Do you want to open configuration window now?";
            return MessageBox.Show(text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
    }
}

//public McTools.Xrm.Connection.CrmConnections ConvertToXrmCrmConnections(CrmConnections crmConnections)
//{
//    if (crmConnections == null)
//    {
//        return null;
//    }

//    var result = new McTools.Xrm.Connection.CrmConnections()
//    {
//        ByPassProxyOnLocal = crmConnections.ByPassProxyOnLocal,
//        ExtendedLog = crmConnections.ExtendedLog,
//        IgnoreExtensions = crmConnections.IgnoreExtensions,
//        Name = crmConnections.Name,
//        Password = crmConnections.Password,
//        ProxyAddress = crmConnections.ProxyAddress,
//        PublishAfterUpload = crmConnections.PublishAfterUpload,
//        UseCustomProxy = crmConnections.UseCustomProxy,
//        UseDefaultCredentials = crmConnections.UseDefaultCredentials,
//        UseInternetExplorerProxy = crmConnections.UseInternetExplorerProxy,
//        UseMruDisplay = crmConnections.UseMruDisplay,
//        UserName = crmConnections.UserName,
//        Connections = crmConnections.Connections.Select(x => ConvertToXrmConnectionDetail(x)).ToList()
//    };

//    return result;
//}

//public McTools.Xrm.Connection.ConnectionDetail ConvertToXrmConnectionDetail(ConnectionDetail connectionDetail)
//{
//    if (connectionDetail == null)
//    {
//        return null;
//    }

//    return new McTools.Xrm.Connection.ConnectionDetail()
//    {
//        AzureAdAppId = connectionDetail.AzureAdAppId,
//        Certificate = connectionDetail.Certificate != null ?
//                new McTools.Xrm.Connection.CertificateInfo()
//                {
//                    Issuer = connectionDetail.Certificate.Issuer,
//                    Name = connectionDetail.Certificate.Name,
//                    Thumbprint = connectionDetail.Certificate.Thumbprint,
//                } : null,
//        ClientSecret = connectionDetail.ClientSecretEncrypted,
//        ConnectionId = connectionDetail.ConnectionId,
//        ConnectionName = connectionDetail.ConnectionName,
//        ConnectionString = connectionDetail.ConnectionString,
//        HomeRealmUrl = connectionDetail.HomeRealmUrl,
//        IsCustomAuth = connectionDetail.IsCustomAuth,
//        NewAuthType = connectionDetail.NewAuthType,
//        Organization = connectionDetail.Organization,
//        OriginalUrl = connectionDetail.OriginalUrl,
//        RefreshToken = connectionDetail.RefreshToken,
//        ReplyUrl = connectionDetail.ReplyUrl,
//        SavePassword = connectionDetail.SavePassword,
//        SelectedSolution = connectionDetail.SelectedSolution,
//        ServerName = connectionDetail.ServerName,
//        ServerPortString = connectionDetail.ServerPortString,
//        Solutions = connectionDetail.Solutions,
//        Timeout = connectionDetail.Timeout,
//        UseIfd = connectionDetail.UseIfd,
//        UseMfa = connectionDetail.UseMfa,
//        UserDomain = connectionDetail.UserDomain,
//        UserName = connectionDetail.UserName,
//        UserPasswordEncrypted = connectionDetail.UserPasswordEncrypted,
//    };
//}

//public CrmConnections ConvertFromXrmCrmConnections(McTools.Xrm.Connection.CrmConnections crmConnections)
//{
//    if (crmConnections == null)
//    {
//        return null;
//    }

//    var result = new CrmConnections()
//    {
//        ByPassProxyOnLocal = crmConnections.ByPassProxyOnLocal,
//        ExtendedLog = crmConnections.ExtendedLog,
//        IgnoreExtensions = crmConnections.IgnoreExtensions,
//        IsReadOnly = crmConnections.IsReadOnly,
//        Name = crmConnections.Name,
//        Password = crmConnections.Password,
//        ProxyAddress = crmConnections.ProxyAddress,
//        PublishAfterUpload = crmConnections.PublishAfterUpload,
//        UseCustomProxy = crmConnections.UseCustomProxy,
//        UseDefaultCredentials = crmConnections.UseDefaultCredentials,
//        UseInternetExplorerProxy = crmConnections.UseInternetExplorerProxy,
//        UseMruDisplay = crmConnections.UseMruDisplay,
//        UserName = crmConnections.UserName,
//        Connections = crmConnections.Connections.Select(x => ConvertFromXrmConnectionDetail(x)).ToList()
//    };

//    return result;
//}

//public ConnectionDetail ConvertFromXrmConnectionDetail(McTools.Xrm.Connection.ConnectionDetail connectionDetail)
//{
//    if (connectionDetail == null)
//    {
//        return null;
//    }

//    return new ConnectionDetail()
//    {
//        AzureAdAppId = connectionDetail.AzureAdAppId,
//        Certificate = connectionDetail.Certificate != null ?
//                new CertificateInfo()
//                {
//                    Issuer = connectionDetail.Certificate.Issuer,
//                    Name = connectionDetail.Certificate.Name,
//                    Thumbprint = connectionDetail.Certificate.Thumbprint,
//                } : null,
//        ClientSecretEncrypted = connectionDetail.ClientSecretEncrypted,
//        ConnectionId = connectionDetail.ConnectionId,
//        ConnectionName = connectionDetail.ConnectionName,
//        ConnectionString = connectionDetail.ConnectionString,
//        HomeRealmUrl = connectionDetail.HomeRealmUrl,
//        IsCustomAuth = connectionDetail.IsCustomAuth,
//        NewAuthType = connectionDetail.NewAuthType,
//        Organization = connectionDetail.Organization,
//        OriginalUrl = connectionDetail.OriginalUrl,
//        RefreshToken = connectionDetail.RefreshToken,
//        ReplyUrl = connectionDetail.ReplyUrl,
//        SavePassword = connectionDetail.SavePassword,
//        SelectedSolution = connectionDetail.SelectedSolution,
//        ServerName = connectionDetail.ServerName,
//        ServerPortString = connectionDetail.ServerPortString,
//        Solutions = connectionDetail.Solutions,
//        Timeout = connectionDetail.Timeout,
//        UseIfd = connectionDetail.UseIfd,
//        UseMfa = connectionDetail.UseMfa,
//        UserDomain = connectionDetail.UserDomain,
//        UserName = connectionDetail.UserName,
//        UserPasswordEncrypted = connectionDetail.UserPasswordEncrypted,
//    };
//}

//private string GetAuthType(AuthenticationType authenticationType)
//{
//    switch (authenticationType)
//    {
//        case AuthenticationType.AD: return "AD";
//        case AuthenticationType.IFD: return "IFD";
//        case AuthenticationType.OAuth: return "OAuth";
//        case AuthenticationType.Office365: return "Office365";
//        case AuthenticationType.Certificate: return "Certificate";
//        case AuthenticationType.ClientSecret: return "ClientSecret";

//        default: throw new ArgumentOutOfRangeException(nameof(authenticationType));
//    }
//}
