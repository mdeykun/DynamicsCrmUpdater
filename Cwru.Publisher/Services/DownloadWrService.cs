using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Extensions;
using Cwru.Common.Model;
using Cwru.Common.Services;
using Cwru.CrmRequests.Common.Contracts;
using Cwru.Publisher.Extensions;
using Cwru.Publisher.Forms;
using Cwru.Publisher.Helpers;
using Cwru.Publisher.Model;
using Cwru.Publisher.Services.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cwru.Publisher.Services
{
    public class DownloadWrService : PublisherBaseService
    {
        private readonly MappingService mappingService;
        private readonly ICrmRequests crmRequest;
        private readonly SolutionsService solutionsService;
        private readonly WebResourceTypesService webResourceTypesService;

        private readonly int openFilesLimit = 10;

        public DownloadWrService(
            ILogger logger,
            ICrmRequests crmWebResourcesUpdaterClient,
            MappingService mappingHelper,
            SolutionsService solutionsService,
            WebResourceTypesService webResourceTypesService,
            VsDteService vsDteService) : base(logger, vsDteService)
        {
            this.crmRequest = crmWebResourcesUpdaterClient;
            this.mappingService = mappingHelper;
            this.solutionsService = solutionsService;
            this.webResourceTypesService = webResourceTypesService;
        }

        public async Task DownloadSelectedWrAsync(ProjectInfo projectInfo, ProjectConfig projectConfig)
        {
            var environmentConfig = projectConfig.GetDefaultEnvironment();

            await OperationStartAsync("Downloading web resources...", "Downloading...", environmentConfig);

            var result = await DownloadWrByFilesAsync(projectConfig, environmentConfig, projectInfo);

            await OperationEndAsync(result,
                $"{result.Processed} web resource{result.Processed.Select(" was", "s were")} downloaded",
                $"Failed to download web resource{result.Total.Select("", "s")}",
                $"Operation was canceled");
        }
        public async Task DownloadWrsAsync(ProjectInfo projectInfo, ProjectConfig projectConfig)
        {
            await OperationStartAsync("Downloading web resources...", "Downloading...");

            var dialog = new SelectWebResourcesForm(projectConfig, crmRequest, solutionsService, webResourceTypesService);

            var result = dialog.ShowDialog() == DialogResult.OK ?
                await DownloadWrByWrNamesAsync(projectInfo, projectConfig, dialog.SelectedEnvironmentId, dialog.SelectedWebResources) :
                new Result() { ResultType = ResultType.Canceled };

            await OperationEndAsync(result,
                $"{result.Processed} web resource{result.Processed.Select(" was", "s were")} downloaded",
                $"Failed to download web resource{result.Total.Select("", "s")}",
                $"Operation was canceled");
        }
        private async Task<Result> DownloadWrByFilesAsync(ProjectConfig projectConfig, EnvironmentConfig environmentConfig, ProjectInfo projectInfo)
        {
            var downloaded = 0;
            var total = 0;
            try
            {
                var filesToDownload = await GetProjectFilesAsync(projectInfo, true);
                total = filesToDownload.Count();
                if (total == 0)
                {
                    return new Result(ResultType.Canceled, total: total, processed: 0, failed: 0, errorMessage: "Files to download were not found or not selected");
                }

                var updateOnDisk = filesToDownload.Count() > openFilesLimit;
                if (updateOnDisk && !ConfirmUpdatingOnDisk())
                {
                    return new Result(ResultType.Canceled, total: total, processed: 0, failed: 0);
                }

                var mappings = await mappingService.LoadMappingsAsync(projectInfo);

                await logger.WriteLineAsync("Starting downloading process");
                await logger.WriteLineAsync("--------------------------------------------------------------");

                var webResourceNames = GetWrNames(filesToDownload, mappings, projectConfig.IgnoreExtensions);
                var webResources = await RetrieveWrsAsync(environmentConfig.ConnectionString.BuildConnectionString(), webResourceNames);
                var fileToWrMapping = await GetFileToWrMappingAsync(projectInfo, filesToDownload, mappings, webResources, projectConfig.IgnoreExtensions);

                foreach (var filePath in fileToWrMapping.Keys)
                {
                    var webResource = fileToWrMapping[filePath];

                    if (await ReplaceWrContentAsync(projectInfo, webResource, filePath, !updateOnDisk))
                    {
                        downloaded += 1;
                    }
                }

                await logger.WriteLineAsync("--------------------------------------------------------------");
                await logger.WriteDebugAsync("Downloading process was completed");
                await logger.WriteLineWithTimeAsync(downloaded + " file" + (downloaded == 1 ? " was" : "s were") + " downloaded");

                return new Result(ResultType.Success, total: total, processed: downloaded, failed: 0);
            }
            catch (Exception ex)
            {
                await logger.WriteLineAsync($"Failed to download script{total.Select("", "s")}.");
                await logger.WriteDebugAsync(ex);

                return new Result(ResultType.Failure, total: total, processed: downloaded, failed: 0, exception: ex);
            }
        }
        private async Task<Result> DownloadWrByWrNamesAsync(ProjectInfo projectInfo, ProjectConfig projectConfig, Guid? selectedEnvironmentId, IEnumerable<WebResource> webResourcesToDownload)
        {
            var downloaded = 0;
            var total = webResourcesToDownload.Count();
            try
            {
                if (total == 0)
                {
                    await logger.WriteLineAsync("List of web resources to be downloaded is empty");
                    return new Result(ResultType.Failure, total: 0, processed: 0, failed: 0);
                }

                if (selectedEnvironmentId == null)
                {
                    await logger.WriteLineAsync("Environment was not selected");
                    return new Result(ResultType.Failure, total: 0, processed: 0, failed: 0);
                }

                var environment = GetEnvironment(projectConfig, selectedEnvironmentId);

                var updateOnDisk = total > openFilesLimit;
                if (updateOnDisk && !ConfirmUpdatingOnDisk())
                {
                    return new Result(ResultType.Canceled, total: 0, processed: 0, failed: 0);
                }

                var webResourcesNames = webResourcesToDownload.Select(x => x.Name).ToList();

                await logger.WriteEnvironmentInfoAsync(environment);

                var mappings = await mappingService.LoadMappingsAsync(projectInfo);

                await logger.WriteDebugAsync("Starting downloading process");
                await logger.WriteLineAsync("--------------------------------------------------------------");

                var webResources = await RetrieveWrsAsync(environment.ConnectionString.BuildConnectionString(), webResourcesNames);
                var fileToWrMapping = GetWrToFileMapping(projectInfo, webResources, mappings, projectConfig.IgnoreExtensions);

                foreach (var wrName in webResourcesNames)
                {
                    if (string.IsNullOrWhiteSpace(wrName))
                    {
                        await logger.WriteLineAsync($"Web resource name can't be null or empty: \"{wrName}\"");
                        continue;
                    }

                    var webResource = webResources.FirstOrDefault(x => wrName.IsEqualToLower(x.Name));
                    if (webResource == null)
                    {
                        await logger.WriteLineAsync($"Web resource was not retrived by name: {wrName}");
                        continue;

                    }

                    var createMapping = false;
                    var filePaths = fileToWrMapping.Where(x => wrName.IsEqualToLower(x.Value.Name)).Select(x => x.Key).ToList();
                    if (filePaths.Count == 0)
                    {
                        var filePath = GetWrFilePath(projectInfo, webResource, projectConfig.IgnoreExtensions);
                        if (string.IsNullOrWhiteSpace(filePath))
                        {
                            await logger.WriteLineAsync($"File path was not build for web resource: {webResource.Name}");
                            continue;
                        }

                        var mapping = mappingService.GetMappingByFilePath(mappings, filePath);
                        if (!string.IsNullOrWhiteSpace(mapping) && !mapping.IsEqualToLower(webResource.Name))
                        {
                            await logger.WriteLineAsync($"File ${filePath.RemoveRoot(projectInfo.Root)} is alredy mapped to another web resource. Please check {MappingService.MappingFileName}");
                            continue;
                        }

                        createMapping = mappingService.IsMappingRequired(mappings, filePath, webResource.Name);
                        filePaths.Add(filePath);
                    }

                    foreach (var filePath in filePaths)
                    {
                        if (string.IsNullOrWhiteSpace(filePath))
                        {
                            await logger.WriteLineAsync($"File path is empty for web resource: {webResource.Name}");
                            continue;
                        }

                        if (!File.Exists(filePath))
                        {
                            File.WriteAllText(filePath, "");
                        }

                        if (!projectInfo.ContainsFile(filePath))
                        {
                            //In case it is just created file or file exist but not added to project
                            await vsDteService.AddFromFileAsync(projectInfo.Guid, filePath);
                        }

                        if (await ReplaceWrContentAsync(projectInfo, webResource, filePath, !updateOnDisk))
                        {
                            downloaded += 1;
                        }

                        if (createMapping == true)
                        {
                            await mappingService.CreateMappingAsync(projectInfo, filePath, webResource.Name);
                        }
                    }
                }

                await logger.WriteLineAsync("--------------------------------------------------------------");
                await logger.WriteDebugAsync("Downloading process was completed");
                await logger.WriteLineWithTimeAsync(downloaded + " file" + (downloaded == 1 ? " was" : "s were") + " downloaded");

                return new Result(ResultType.Success, total: total, processed: downloaded, failed: 0);
            }
            catch (Exception ex)
            {
                await logger.WriteLineAsync($"Failed to download script{total.Select("", "s")}.");
                await logger.WriteDebugAsync(ex);

                return new Result(ResultType.Failure, total: total, processed: downloaded, failed: 0, ex);
            }
        }

        private EnvironmentConfig GetEnvironment(ProjectConfig projectConfig, Guid? selectedEnvironmentId)
        {
            if (selectedEnvironmentId == null)
            {
                throw new Exception("Environment is not selected");
            }

            var environment = projectConfig.GetEnvironment(selectedEnvironmentId.Value);
            if (environment == null)
            {
                throw new Exception("Environment configuration wasn't found");
            }

            return environment;
        }

        private async Task<IEnumerable<WebResource>> RetrieveWrsAsync(string connectionString, IEnumerable<string> webResourceNames)
        {
            await logger.WriteDebugAsync("Retrieving existing web resources");

            var retrieveWebResourceResponse = await crmRequest.RetrieveWebResourcesAsync(connectionString, webResourceNames);
            if (retrieveWebResourceResponse.IsSuccessful == false)
            {
                throw new Exception($"Failed to retrieve web resource: {retrieveWebResourceResponse.ErrorMessage}");
            }

            return retrieveWebResourceResponse.Payload;
        }
        private async Task<bool> ReplaceWrContentAsync(ProjectInfo projectInfo, WebResource webResource, string filePath, bool openDownloadedFiles = false)
        {
            var localContent = FilesHelper.GetEncodedFileContent(filePath);
            var remoteContent = webResource.Content;

            var relativePath = filePath.Replace(projectInfo.Root + "\\", "");

            if (string.IsNullOrEmpty(remoteContent))
            {
                await logger.WriteLineAsync($"{webResource.Name} is empty. {relativePath}");
                return false;
            }

            if (string.Compare(localContent, remoteContent) == 0)
            {
                await logger.WriteLineAsync($"{webResource.Name} has no changes. {relativePath}");
                return false;
            }

            if (openDownloadedFiles)
            {
                var weresourceContent = Encoding.UTF8.GetString(Convert.FromBase64String(remoteContent));
                await vsDteService.OpenFileAndPlaceContentAsync(projectInfo.Guid, filePath, weresourceContent);
            }
            else
            {
                File.WriteAllBytes(filePath, Convert.FromBase64String(remoteContent));
            }

            await logger.WriteLineAsync($"{webResource.Name} was downloaded. {relativePath}");
            return true;
        }
        private Dictionary<string, WebResource> GetWrToFileMapping(ProjectInfo projectInfo, IEnumerable<WebResource> webResources, Dictionary<string, string> mappings, bool ignoreExtensions)
        {
            var result = new Dictionary<string, WebResource>();

            foreach (var wr in webResources)
            {
                var selectedFolderPath = projectInfo.GetSelectedFolderPath() ?? projectInfo.Root;

                var filesToUpdate = projectInfo.
                    GetFilesPathsUnder(selectedFolderPath).
                    Where(x => wr.Name.IsEqualToLower(Path.GetFileName(x)) || (ignoreExtensions && wr.Name.IsEqualToLower(Path.GetFileNameWithoutExtension(x)))).
                    Select(x => x.ToLower()).
                    ToList();

                var filesFromMapping = mappings.Where(x => wr.Name.IsEqualToLower(x.Value)).Select(x => x.Key.ToLower());
                filesToUpdate.AddRange(filesFromMapping);

                filesToUpdate.
                    Distinct(StringComparer.OrdinalIgnoreCase).
                    ToList().
                    ForEach(x => result.Add(x.ToLower(), wr));
            }

            return result;
        }
        private string GetWrFilePath(ProjectInfo projectInfo, WebResource webResource, bool ignoreExtensions)
        {
            var fileName = Path.GetFileName(webResource.Name)?.RemoveIllegalFileNameSymbols();
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var extensions = webResource.Type != null ?
                webResourceTypesService.GetExtensions(webResource.Type.Value) :
                Enumerable.Empty<string>();

            if (extensions.Count() > 0 && extensions.All(x => !fileName.EndsWith(x)))
            {
                fileName = fileName + extensions.First();
            }

            var folderPath = projectInfo.GetSelectedFolderPath() ?? projectInfo.Root;
            var filePath = Path.Combine(folderPath, fileName);

            return filePath;
        }
        private bool ConfirmUpdatingOnDisk()
        {
            var message = $"You are downloading more then {openFilesLimit} web resources at once. Tool will update files content directly on disk. You will not be able to undo these changes. Please consider to backup existent web resource files or downlod less files at once.\r\n\r\nDo you want to proceed?";
            var limitExidDialogResult = MessageBox.Show(message, "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (limitExidDialogResult == DialogResult.OK)
            {
                return true;
            }

            return false;
        }

    }
}
