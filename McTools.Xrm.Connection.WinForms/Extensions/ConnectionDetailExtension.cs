using Cwru.Common.Model;
using McTools.Xrm.Connection.WinForms.Model;
using System;
using System.IO;

namespace McTools.Xrm.Connection.WinForms.Extensions
{
    public static class ConnectionDetailExtensions
    {
        //TODO: Review from AuthenticationTypes perspective
        public static CrmConnectionString ToCrmConnectionString(this ConnectionDetail connectionDetail, bool? forceNewService = null)
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
                return CrmConnectionString.Parse(connectionDetail.ConnectionString);
            }

            if (connectionDetail.AuthType == AuthenticationType.ClientSecret)
            {
                return new CrmConnectionString()
                {
                    AuthenticationType = AuthenticationType.ClientSecret,
                    ServiceUri = connectionDetail.OriginalUrl,
                    ClientId = connectionDetail.AzureAdAppId,
                    ClientSecret = connectionDetail.ClientSecret,
                    RequireNewInstance = forceNewService,
                    LoginPrompt = "None"
                };
            }

            if (connectionDetail.AuthType == AuthenticationType.OAuth && connectionDetail.UseMfa)
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
                    Password = connectionDetail.UserPassword,
                    ClientId = connectionDetail.AzureAdAppId != Guid.Empty ? connectionDetail.AzureAdAppId : new Guid("51f81489-12ee-4a9e-aaae-a2591f45987d"),
                    RedirectUri = connectionDetail.ReplyUrl ?? "app://58145B91-0C36-4500-8554-080854F2AC97",
                    TokenCacheStorePath = path,
                    RequireNewInstance = forceNewService,
                    LoginPrompt = "None"
                };
            }

            if (connectionDetail.UseIfd)
            {
                if (connectionDetail.IntegratedSecurity == true)
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
                        Password = connectionDetail.UserPassword,
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

            if (connectionDetail.IntegratedSecurity != true)
            {
                cs.Domain = connectionDetail.UserDomain;
                cs.UserName = connectionDetail.UserName;
                cs.Password = connectionDetail.UserPassword;
                cs.IntegratedSecurity = null;
            }

            return cs;
        }

        public static string GetUserName(this ConnectionDetail detail)
        {
            if (!string.IsNullOrWhiteSpace(detail.UserDomain) && !string.IsNullOrWhiteSpace(detail.UserName))
            {
                return $"{detail.UserDomain}\\{detail.UserName}";
            }

            if (!string.IsNullOrWhiteSpace(detail.UserName))
            {
                return detail.UserName;
            }

            if (detail.AzureAdAppId != Guid.Empty)
            {
                return detail.AzureAdAppId.ToString("B");
            }

            return null;
        }

        public static int GetImageIndex(this ConnectionDetail detail)
        {
            if (detail.UseOnline)
            {
                return 2;
            }

            if (detail.UseIfd)
            {
                return 1;
            }

            return 0;
        }
    }
}
