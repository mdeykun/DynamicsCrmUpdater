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
        protected readonly Logger logger;
        protected readonly VsDteService vsDteService;

        public PublisherBaseService(Logger logger, VsDteService vsDteService)
        {
            this.logger = logger;
            this.vsDteService = vsDteService;
        }

        protected async Task OperationStartAsync(string message, string statusBarMessage = null, EnvironmentConfig environmentConfig = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                await logger.WriteLineWithTimeAsync(message);
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
        protected async Task<IEnumerable<string>> GetProjectFilesAsync(ProjectInfo projectInfo, bool selectedItemsOnly, bool extendedLog)
        {
            await logger.WriteLineAsync(selectedItemsOnly ? "Loading selected files' paths" : "Loading all files' paths", extendedLog);
            var files = selectedItemsOnly ? projectInfo.GetSelectedFilesPaths() : projectInfo.GetFilesPaths();

            if (files == null || files.Count() == 0)
            {
                await logger.WriteLineAsync("Failed to load files' paths", extendedLog);
                return Enumerable.Empty<string>();
            }

            await logger.WriteLineAsync(files.Count() + " path" + (files.Count() == 1 ? " was" : "s were") + " loaded", extendedLog);

            files = files.ExcludeFile(MappingService.MappingFileName);
            return files;
        }
        protected async Task<Dictionary<string, WebResource>> GetFileToWrMappingAsync(ProjectInfo projectInfo, IEnumerable<string> files, Dictionary<string, string> mappings, IEnumerable<WebResource> webResources, bool ignoreExtensions, bool extendedLog)
        {
            var result = new Dictionary<string, WebResource>();

            foreach (var filePath in files)
            {
                var webResourceName = Path.GetFileName(filePath);
                if (mappings != null && mappings.ContainsKey(filePath))
                {
                    webResourceName = mappings[filePath];

                    var relativePath = filePath.Replace(projectInfo.Root + "\\", "");
                    await logger.WriteLineAsync($"Mapping found: {relativePath} to {webResourceName}", extendedLog);
                }

                var webResource = webResources.FirstOrDefault(x => x.Name.IsEqualToLower(webResourceName));
                if (webResource == null && ignoreExtensions)
                {
                    await logger.WriteLineAsync(webResourceName + " does not exist or not added to selected solution", extendedLog);
                    webResourceName = Path.GetFileNameWithoutExtension(filePath);
                    await logger.WriteLineAsync("Searching for " + webResourceName, extendedLog);
                    webResource = webResources.FirstOrDefault(x => x.Name.IsEqualToLower(webResourceName));
                }
                if (webResource == null)
                {
                    await logger.WriteLineAsync("Uploading of " + webResourceName + " was skipped: web resource does not exist or not added to selected solution", extendedLog);
                    await logger.WriteLineAsync(webResourceName + " does not exist or not added to selected solution", !extendedLog);
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
