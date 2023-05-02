using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Extensions;
using Cwru.Common.Model;
using Cwru.Common.Services;
using Cwru.Publisher.Extensions;
using Cwru.Publisher.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cwru.Publisher.Services.Base
{
    public class PublisherBaseService
    {
        protected readonly ILogger logger;
        protected readonly VsDteService vsDteService;
        protected readonly bool showDonation = false;

        public PublisherBaseService(ILogger logger, VsDteService vsDteService, bool showDonation = false)
        {
            this.logger = logger;
            this.vsDteService = vsDteService;
            this.showDonation = showDonation;
        }

        protected async Task OperationStartAsync(string message, string statusBarMessage = null, EnvironmentConfig environmentConfig = null)
        {
            if (showDonation)
            {
                await ShowSupportUsMessageAsync();
                await logger.WriteLineAsync();
            }

            if (!string.IsNullOrEmpty(message))
            {
                await logger.WriteLineWithTimeAsync(message);
                await logger.WriteLineAsync();
            }

            if (!string.IsNullOrEmpty(statusBarMessage))
            {
                await vsDteService.SetStatusBarAsync(statusBarMessage);
            }

            if (environmentConfig != null)
            {
                await logger.WriteEnvironmentInfoAsync(environmentConfig);
            }
        }
        protected async Task OperationEndAsync(Result result, string successMessage, string errorMessage, string cancelMessage)
        {
            var error = result.GetErrorMessage();
            if (!string.IsNullOrEmpty(error))
            {
                await logger.WriteLineAsync(error);
            }

            var message = string.Empty;
            switch (result.ResultType)
            {
                case ResultType.Success:
                    message = successMessage;
                    break;
                case ResultType.Failure:
                    message = errorMessage;
                    break;
                case ResultType.Canceled:
                    message = cancelMessage;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Result.ResultType));
            }

            if (!string.IsNullOrEmpty(message))
            {
                await logger.WriteLineWithTimeAsync(message);
            }

            var statusBarMessage = string.Empty;
            switch (result.ResultType)
            {
                case ResultType.Success:
                    statusBarMessage = "Done.";
                    break;
                case ResultType.Failure:
                    statusBarMessage = "Failed.";
                    break;
                case ResultType.Canceled:
                    statusBarMessage = "Canceled.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Result.ResultType));
            }

            if (!string.IsNullOrEmpty(statusBarMessage))
            {
                await vsDteService.SetStatusBarAsync(statusBarMessage);
            }
        }
        protected async Task ShowSupportUsMessageAsync()
        {
            await logger.WriteLineAsync("Please help us to support and extend the tool:");
            await logger.WriteLineAsync();
            await logger.WriteLineAsync("USDT (ERC20): " + Info.UsdtErc20);
            await logger.WriteLineAsync("USDT (TRC20): " + Info.UsdtTrc20);
            await logger.WriteLineAsync("Ethereum: " + Info.EthErc20);
            await logger.WriteLineAsync("Bitcoin: " + Info.Btc);
        }
        protected async Task<IEnumerable<string>> GetProjectFilesAsync(ProjectInfo projectInfo, bool selectedItemsOnly)
        {
            await logger.WriteDebugAsync(selectedItemsOnly ? "Loading selected files' paths" : "Loading all files' paths");
            var files = selectedItemsOnly ? projectInfo.GetSelectedFilesPaths() : projectInfo.GetFilesPaths();

            if (files == null || files.Count() == 0)
            {
                await logger.WriteDebugAsync("Failed to load files' paths");
                return Enumerable.Empty<string>();
            }

            await logger.WriteDebugAsync(files.Count() + " path" + (files.Count() == 1 ? " was" : "s were") + " loaded");

            files = files.ExcludeFile(MappingService.MappingFileName);
            return files;
        }
        protected async Task<Dictionary<string, WebResource>> GetFileToWrMappingAsync(ProjectInfo projectInfo, IEnumerable<string> files, Dictionary<string, string> mappings, IEnumerable<WebResource> webResources, bool ignoreExtensions)
        {
            var result = new Dictionary<string, WebResource>();

            foreach (var filePath in files)
            {
                var webResourceName = Path.GetFileName(filePath);
                if (mappings != null && mappings.ContainsKey(filePath))
                {
                    webResourceName = mappings[filePath];

                    var relativePath = filePath.Replace(projectInfo.Root + "\\", "");
                    await logger.WriteDebugAsync($"Mapping found: {relativePath} to {webResourceName}");
                }

                var webResource = webResources.FirstOrDefault(x => x.Name.IsEqualToLower(webResourceName));
                if (webResource == null && ignoreExtensions)
                {
                    await logger.WriteDebugAsync(webResourceName + " does not exist or not added to selected solution");
                    webResourceName = Path.GetFileNameWithoutExtension(filePath);
                    await logger.WriteDebugAsync("Searching for " + webResourceName);
                    webResource = webResources.FirstOrDefault(x => x.Name.IsEqualToLower(webResourceName));
                }
                if (webResource == null)
                {
                    await logger.WriteLineAsync(webResourceName + " does not exist or not added to selected solution");
                    continue;
                }

                if (!File.Exists(filePath))
                {
                    await logger.WriteLineAsync("Warning: File not found: " + filePath);
                    continue;
                }

                result.Add(filePath, webResource);
            }

            return result;
        }
        protected static IEnumerable<string> GetWrNames(IEnumerable<string> filesPathes, Dictionary<string, string> mappings, bool ignoreExtensions)
        {
            var webResourceNames = new List<string>();
            foreach (var filePath in filesPathes)
            {
                var webResourceName = Path.GetFileName(filePath);
                var lowerFilePath = filePath.ToLower();

                if (mappings != null && mappings.ContainsKey(lowerFilePath))
                {
                    webResourceName = mappings[lowerFilePath];
                }
                else if (ignoreExtensions)
                {
                    webResourceNames.Add(Path.GetFileNameWithoutExtension(filePath));
                }
                webResourceNames.Add(webResourceName);
            }

            return webResourceNames;
        }
    }
}
