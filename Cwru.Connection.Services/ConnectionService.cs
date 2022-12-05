using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Extensions;
using Cwru.Common.Model;
using Cwru.Common.Services;
using Cwru.Connection.Services.Model;
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
        private readonly ILogger logger;
        private readonly VsDteService vsDteHelper;
        private readonly MappingService mappingHelper;
        private readonly ICrmRequests crmRequests;
        private readonly ToolConfigurationService toolConfigurationService;
        private readonly ProjectConfigurationService projectConfigurationService;
        private readonly SolutionsService solutionsService;

        public ConnectionService(
            ILogger logger,
            ICrmRequests crmRequests,
            SolutionsService solutionsService,
            VsDteService vsDteHelper,
            MappingService mappingHelper,
            ToolConfigurationService toolConfigurationService,
            ProjectConfigurationService projectConfigurationService)
        {
            this.logger = logger;
            this.crmRequests = crmRequests;
            this.solutionsService = solutionsService;
            this.vsDteHelper = vsDteHelper;
            this.mappingHelper = mappingHelper;
            this.toolConfigurationService = toolConfigurationService;
            this.projectConfigurationService = projectConfigurationService;
        }

        public async Task<ConnectionData> GetAndValidateConnectionAsync()
        {
            var projectInfo = await vsDteHelper.GetSelectedProjectInfoAsync();
            if (projectInfo == null)
            {
                return GetFailed("Project is not selected or selected project can't be identified");
            }

            var projectConfig = await projectConfigurationService.GetProjectConfigAsync(projectInfo.Guid);
            if (projectConfig == null)
            {
                return GetFailed("Failed to load project config.");
            }

            var toolConfig = toolConfigurationService.GetToolConfig();
            if (toolConfig == null)
            {
                return GetFailed("Failed to load tool config.");
            }

            var environmentConfig = projectConfig.GetDefaultEnvironment();
            if (environmentConfig == null || environmentConfig.ConnectionString == null)
            {
                if (ShowErrorDialog() == DialogResult.Yes)
                {
                    environmentConfig = await ShowConfigurationDialogAsync();
                    if (environmentConfig == null)
                    {
                        return GetFailed("Сonnection was not configured");
                    }
                    else
                    {
                        return new ConnectionData()
                        {
                            IsJustConfigured = true,
                            IsValid = true,
                            ProjectConfig = null,
                            ProjectInfo = null,
                        };
                    }
                }
                else
                {
                    return GetFailed();
                }
            }

            if (environmentConfig.ConnectionString.IntegratedSecurity != true &&
                environmentConfig.ConnectionString.AuthenticationType != AuthenticationType.Certificate &&
                environmentConfig.ConnectionString.UserName != null)
            {
                if (environmentConfig.ConnectionString.Password != null)
                {
                    return new ConnectionData()
                    {
                        IsValid = true,
                        ProjectConfig = projectConfig,
                        ProjectInfo = projectInfo,
                    };
                }
                else
                {
                    var result = ShowUpdatePasswordDialogAsync(projectConfig);
                    if (result == DialogResult.OK)
                    {
                        return new ConnectionData()
                        {
                            IsValid = true,
                            ProjectConfig = projectConfig,
                            ProjectInfo = projectInfo,
                        };
                    }
                }
            }

            if (environmentConfig.ConnectionString.AuthenticationType == AuthenticationType.ClientSecret &&
                environmentConfig.ConnectionString.ClientId != null)
            {
                if (environmentConfig.ConnectionString.ClientSecret != null)
                {
                    return new ConnectionData()
                    {
                        IsValid = true,
                        ProjectConfig = projectConfig,
                        ProjectInfo = projectInfo,
                    };
                }
                else
                {
                    var result = ShowUpdateClientSecretDialogAsync(projectConfig);
                    if (result == DialogResult.OK)
                    {
                        return new ConnectionData()
                        {
                            IsValid = true,
                            ProjectConfig = projectConfig,
                            ProjectInfo = projectInfo,
                        };
                    }
                    else
                    {
                        return GetFailed("Сonnection form was closed without saving");
                    }
                }
            }

            return new ConnectionData()
            {
                IsValid = true,
                ProjectConfig = projectConfig,
                ProjectInfo = projectInfo,
            };
        }

        public async Task<EnvironmentConfig> ShowConfigurationDialogAsync()
        {
            var project = await vsDteHelper.GetSelectedProjectInfoAsync();
            if (project == null)
            {
                await logger.WriteLineAsync("Project is not selected or selected project can't be identified");
                return null;
            }

            var toolConfig = toolConfigurationService.GetToolConfig();
            var projectConfig = await projectConfigurationService.GetProjectConfigAsync(project.Guid);

            var selector = new ConnectionSelector(
                logger,
                project,
                mappingHelper,
                crmRequests,
                solutionsService,
                ConvertToXrmConnectionDetail(projectConfig, toolConfig));

            selector.ShowDialog();

            if (selector.DialogResult == DialogResult.OK)
            {
                toolConfig.ExtendedLog = selector.ConnectionsList.ExtendedLog;
                toolConfigurationService.SaveToolConfig(toolConfig);

                projectConfig.Environments = ConvertFromXrmCrmConnections(selector.ConnectionsList);
                projectConfig.PublishAfterUpload = selector.ConnectionsList.PublishAfterUpload;
                projectConfig.IgnoreExtensions = selector.ConnectionsList.IgnoreExtensions;
                projectConfig.DafaultEnvironmentId = selector.ConnectionsList.SelectedConnectionId;
                projectConfigurationService.SaveProjectConfig(projectConfig);


                return projectConfig.GetDefaultEnvironment();
            }

            return null;
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

        private DialogResult ShowUpdatePasswordDialogAsync(ProjectConfig projectConfig)
        {
            var environmentConfig = projectConfig.GetDefaultEnvironment();

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
                    projectConfigurationService.SaveProjectConfig(projectConfig);
                }
            }
            return dialog.DialogResult;
        }

        private DialogResult ShowUpdateClientSecretDialogAsync(ProjectConfig projectConfig)
        {
            var environmentConfig = projectConfig.GetDefaultEnvironment();
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
                    projectConfigurationService.SaveProjectConfig(projectConfig);
                }
            }

            return dialog.DialogResult;
        }

        private ConnectionDetailsList ConvertToXrmConnectionDetail(ProjectConfig projectConfig, ToolConfig toolConfig)
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
                ExtendedLog = toolConfig.ExtendedLog == true,
                IgnoreExtensions = projectConfig.IgnoreExtensions,
                PublishAfterUpload = projectConfig.PublishAfterUpload,
                SelectedConnectionId = projectConfig.DafaultEnvironmentId
            };
        }

        private DialogResult ShowErrorDialog()
        {
            var title = "Configuration error";
            var text = "It seems that Publisher has not been configured yet or connection is not selected.\r\n\r\n" +
            "We can open configuration window for you now or you can do it later by clicking \"Publish options\" in the context menu of the project.\r\n\r\n" +
            "Do you want to open configuration window now?";
            return MessageBox.Show(text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private ConnectionData GetFailed(string message = null)
        {
            return new ConnectionData()
            {
                IsValid = false,
                Message = message,
            };
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
