using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Extensions;
using Cwru.Common.Model;
using Cwru.Common.Services;
using Cwru.CrmRequests.Common.Contracts;
using Cwru.Publisher.Extensions;
using Cwru.Publisher.Forms;
using Cwru.Publisher.Model;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace Cwru.Publisher
{
    public class PublishService
    {
        private readonly MappingService mappingService;
        private readonly ICrmRequests crmRequest;
        private readonly SolutionsService solutionsService;
        private readonly Logger logger;
        private readonly VsDteService vsDteService;

        public PublishService(
            Logger logger,
            ICrmRequests crmWebResourcesUpdaterClient,
            MappingService mappingHelper,
            SolutionsService solutionsService,
            VsDteService vsDteService)
        {
            this.crmRequest = crmWebResourcesUpdaterClient;
            this.mappingService = mappingHelper;
            this.solutionsService = solutionsService;
            this.logger = logger;
            this.vsDteService = vsDteService;
        }

        public Task UploadWrToEnvironmentsAsync(ProjectInfo projectInfo, ProjectConfig projectConfig, bool selectedItemsOnly)
        {
            throw new NotImplementedException();
        }

        public async Task UploadWrToDefaultEnvironmentAsync(ProjectInfo projectInfo, ProjectConfig projectConfig, bool selectedItemsOnly)
        {
            await OperationStart("Uploading web resources...", "Uploading...");

            var filesToPublish = await GetProjectFilesAsync(projectInfo, selectedItemsOnly, projectConfig.ExtendedLog);
            if (filesToPublish.Count() == 0)
            {
                return;
            }

            var result = await UploadWrAsync(projectConfig, projectConfig.GetDefaultEnvironment(), projectInfo, filesToPublish);

            if (result.Exception == null)
            {
                await OperationEnd("Done.", $"{result.Processed} web resource{result.Processed.Select(" was", "s were")} uploaded");
            }
            else
            {
                await OperationEnd("Done.", $"Failed to upload script{filesToPublish.Count().Select("", "s")}");
            }
        }

        public async Task CreateWrAsync(ProjectInfo projectInfo, ProjectConfig projectConfig)
        {
            var environmentConfig = projectConfig.GetDefaultEnvironment();

            var selectedSolution = await solutionsService.GetSolutionDetailsAsync(environmentConfig, true);
            if (selectedSolution == null)
            {
                await logger.WriteLineAsync("Solution is not selected. Please check parameters");
                return;
            }

            var selectedFile = projectInfo.SelectedFile;
            if (selectedFile == null)
            {
                throw new Exception("File was not selected");
            }

            var dialog = new CreateWebResourceForm(logger, selectedFile, selectedSolution.PublisherPrefix);

            dialog.OnCreate = async (WebResource webResource) =>
            {
                webResource.Content = GetEncodedFileContent(selectedFile);

                var webresourceName = webResource.Name;

                var isWebResourceExistsResponse = await crmRequest.IsWebResourceExistsAsync(environmentConfig.ConnectionString.BuildConnectionString(), webresourceName);
                if (isWebResourceExistsResponse.IsSuccessful == false)
                {
                    MessageBox.Show($"Failed to validate webresource existance: {isWebResourceExistsResponse.ErrorMessage}", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (isWebResourceExistsResponse.Payload)
                {
                    MessageBox.Show("Webresource with name '" + webresourceName + "' already exist in CRM.", "Webresource already exists.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var isMappingRequired = mappingService.IsMappingRequired(
                        projectInfo.Root,
                        projectInfo.Files,
                        selectedFile,
                        webresourceName,
                        projectConfig.IgnoreExtensions);

                    var isMappingFileReadOnly = mappingService.IsMappingFileReadOnly(projectInfo.Files);
                    if (isMappingRequired && isMappingFileReadOnly)
                    {
                        Cursor.Current = Cursors.Arrow;
                        var message = "Mapping record can't be created. File \"UploaderMapping.config\" is read-only. Do you want to proceed? \r\n\r\n" +
                                        "Schema name of the web resource you are creating is differ from the file name. " +
                                        "Because of that new mapping record has to be created in the file \"UploaderMapping.config\". " +
                                        "Unfortunately the file \"UploaderMapping.config\" is read-only (file might be under a source control), so mapping record cant be created. \r\n\r\n" +
                                        "Press OK to proceed without mapping record creation (You have to do that manually later). Press Cancel to fix problem and try later.";
                        var result = MessageBox.Show(message, "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                        if (result == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    Cursor.Current = Cursors.WaitCursor;
                    if (isMappingRequired && !isMappingFileReadOnly)
                    {
                        await mappingService.CreateMappingAsync(
                            projectInfo.Guid,
                            projectInfo.Root,
                            projectInfo.Files,
                            selectedFile,
                            webresourceName);
                    }

                    var createWebResourceResponse = await crmRequest.CreateWebresourceAsync(environmentConfig.ConnectionString.BuildConnectionString(), webResource, selectedSolution.UniqueName);
                    createWebResourceResponse.EnsureSuccess();

                    await logger.WriteLineAsync("Webresource '" + webresourceName + "' was successfully created");
                    dialog.Close();
                }
                catch (Exception ex)
                {
                    Cursor.Current = Cursors.Arrow;
                    MessageBox.Show("An error occured during web resource creation: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    await logger.WriteLineAsync($"An error occured during web resource creation: {ex}", true);
                }
            };
            dialog.ShowDialog();
        }

        public async Task DownloadSelectedWrAsync(ProjectInfo projectInfo, ProjectConfig projectConfig)
        {
            await OperationStart("Downloading web resources...", "Downloading...");

            var filesToDownload = await GetProjectFilesAsync(projectInfo, true, projectConfig.ExtendedLog);
            if (filesToDownload.Count() == 0)
            {
                return;
            }

            var result = await DownloadWrAsync(projectConfig, projectConfig.GetDefaultEnvironment(), projectInfo, filesToDownload);

            if (result.Exception == null)
            {
                await OperationEnd("Done.", $"{result.Processed} web resource{result.Processed.Select(" was", "s were")} downloaded");
            }
            else
            {
                await OperationEnd("Done.", $"Failed to download script{filesToDownload.Count().Select("", "s")}");
            }
        }

        public async Task DownloadWrsAsync(ProjectInfo projectInfo, ProjectConfig projectConfig)
        {
            await OperationStart("Downloading web resources...", "Downloading...");

            var dialog = new SelectWebResourcesForm(logger, projectConfig, crmRequest, solutionsService);
            dialog.ShowDialog();

            return;

            //TODO
            var filesToDownload = Enumerable.Empty<string>(); //await (projectInfo, true, projectConfig.ExtendedLog);
            if (filesToDownload.Count() == 0)
            {
                return;
            }

            var result = await DownloadWrAsync(projectConfig, projectConfig.GetDefaultEnvironment(), projectInfo, filesToDownload);

            if (result.Exception == null)
            {
                await OperationEnd("Done.", $"{result.Processed} web resource{result.Processed.Select(" was", "s were")} downloaded");
            }
            else
            {
                await OperationEnd("Done.", $"Failed to download script{filesToDownload.Count().Select("", "s")}");
            }
        }

        private async Task<Result> DownloadWrAsync(ProjectConfig projectConfig, EnvironmentConfig environmentConfig, ProjectInfo projectInfo, IEnumerable<string> filesToDownload)
        {
            var downloaded = 0;

            try
            {
                await logger.WriteEnvironmentInfoAsync(environmentConfig);

                var mappings = mappingService.LoadMappings(projectInfo.Root, projectInfo.Files);

                await logger.WriteLineAsync("Starting downloading process", projectConfig.ExtendedLog);
                await logger.WriteLineAsync("--------------------------------------------------------------");

                var webResources = await RetrieveWrsAsync(environmentConfig, filesToDownload, mappings, projectConfig.IgnoreExtensions, projectConfig.ExtendedLog);
                var fileToWrMapping = await GetFileToWrMappingAsync(projectInfo, filesToDownload, mappings, webResources, projectConfig.IgnoreExtensions, projectConfig.ExtendedLog);

                foreach (var filePath in fileToWrMapping.Keys)
                {
                    var webResource = fileToWrMapping[filePath];
                    var localContent = GetEncodedFileContent(filePath);
                    var remoteContent = webResource.Content;

                    if (string.IsNullOrEmpty(remoteContent))
                    {
                        await logger.WriteLineAsync(webResource.Name + " is empty");
                    }
                    else if (string.Compare(localContent, remoteContent) != 0)
                    {
                        var weresourceContent = Encoding.UTF8.GetString(Convert.FromBase64String(remoteContent));
                        await vsDteService.OpenFileAndPlaceContentAsync(projectInfo.Guid, filePath, weresourceContent);

                        var relativePath = filePath.Replace(projectInfo.Root + "\\", "");
                        await logger.WriteLineAsync($"{webResource.Name} was downloaded to " + relativePath);
                        downloaded += 1;
                    }
                    else
                    {
                        await logger.WriteLineAsync(webResource.Name + " has no changes");
                    }
                }

                await logger.WriteLineAsync("--------------------------------------------------------------");
                await logger.WriteLineAsync("Downloading process was completed", projectConfig.ExtendedLog);

                return new Result(total: filesToDownload.Count(), processed: downloaded, failed: 0);
            }
            catch (Exception ex)
            {
                await logger.WriteLineAsync($"Failed to download script{filesToDownload.Count().Select("", "s")}.");
                await logger.WriteAsync(ex, projectConfig.ExtendedLog);

                return new Result(total: filesToDownload.Count(), processed: downloaded, failed: 0, ex);
            }
        }

        private async Task<Result> UploadWrAsync(ProjectConfig projectConfig, EnvironmentConfig environmentConfig, ProjectInfo projectInfo, IEnumerable<string> filesToUpload)
        {
            var updatedWrs = new List<WebResource>();

            try
            {
                await logger.WriteEnvironmentInfoAsync(environmentConfig);

                var selectedSolution = await solutionsService.GetSolutionDetailsAsync(environmentConfig);
                await logger.WriteSolutionInfoAsync(selectedSolution);

                await logger.WriteLineAsync("Starting uploading process", projectConfig.ExtendedLog);
                await logger.WriteLineAsync("--------------------------------------------------------------");

                var mappings = mappingService.LoadMappings(projectInfo.Root, projectInfo.Files);

                var webResources = await RetrieveSelectedSolutionWrsAsync(environmentConfig, filesToUpload, mappings, projectConfig.IgnoreExtensions, projectConfig.ExtendedLog);
                var fileToWrMapping = await GetFileToWrMappingAsync(projectInfo, filesToUpload, mappings, webResources, projectConfig.IgnoreExtensions, projectConfig.ExtendedLog);

                foreach (var filePath in fileToWrMapping.Keys)
                {
                    var webResource = fileToWrMapping[filePath];

                    var relativePath = filePath.Replace(projectInfo.Root + "\\", "");
                    var isUpdated = await UpdateWrByFileAsync(environmentConfig, webResource, filePath, relativePath, projectConfig.ExtendedLog);
                    if (isUpdated)
                    {
                        updatedWrs.Add(webResource);
                    }
                }

                await logger.WriteLineAsync("--------------------------------------------------------------");
                await logger.WriteLineAsync("Uploading process was completed", projectConfig.ExtendedLog);

                await logger.WriteLineWithTimeAsync(updatedWrs.Count + " file" + (updatedWrs.Count == 1 ? " was" : "s were") + " uploaded");


                if (updatedWrs.Count > 0 && projectConfig.PublishAfterUpload)
                {
                    await PublishWrAsync(environmentConfig, updatedWrs.Select(x => x.Id.Value).ToList());
                }

                return new Result(total: filesToUpload.Count(), processed: updatedWrs.Count, failed: 0);
            }
            catch (Exception ex)
            {
                await logger.WriteLineAsync($"Failed to upload script{filesToUpload.Count().Select("", "s")}.");
                await logger.WriteAsync(ex, projectConfig.ExtendedLog);

                return new Result(total: filesToUpload.Count(), processed: updatedWrs.Count, failed: 0, ex);
            }
        }

        private async Task<IEnumerable<string>> GetProjectFilesAsync(ProjectInfo projectInfo, bool selectedItemsOnly, bool extendedLog)
        {
            await logger.WriteLineAsync(selectedItemsOnly ? "Loading selected files' paths" : "Loading all files' paths", extendedLog);
            var files = selectedItemsOnly ? projectInfo.SelectedFiles : projectInfo.Files;

            if (files == null || files.Count() == 0)
            {
                await logger.WriteLineAsync("Failed to load files' paths", extendedLog);
                return Enumerable.Empty<string>();
            }

            await logger.WriteLineAsync(files.Count() + " path" + (files.Count() == 1 ? " was" : "s were") + " loaded", extendedLog);

            files = files.ExcludeFile(MappingService.MappingFileName);
            return files;
        }

        private async Task<Dictionary<string, WebResource>> GetFileToWrMappingAsync(ProjectInfo projectInfo, IEnumerable<string> files, Dictionary<string, string> mappings, IEnumerable<WebResource> webResources, bool ignoreExtensions, bool extendedLog)
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

                var webResource = webResources.FirstOrDefault(x => x.Name == webResourceName);
                if (webResource == null && ignoreExtensions)
                {
                    await logger.WriteLineAsync(webResourceName + " does not exist or not added to selected solution", extendedLog);
                    webResourceName = Path.GetFileNameWithoutExtension(filePath);
                    await logger.WriteLineAsync("Searching for " + webResourceName, extendedLog);
                    webResource = webResources.FirstOrDefault(x => x.Name == webResourceName);
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

        private async Task<bool> UpdateWrByFileAsync(EnvironmentConfig environmentConfig, WebResource webResource, string filePath, string relativePath, bool extendedLog)
        {
            var webResourceName = Path.GetFileName(filePath);
            await logger.WriteLineAsync("Uploading " + webResourceName, extendedLog);

            var localContent = GetEncodedFileContent(filePath);
            var remoteContent = webResource.Content;
            if (remoteContent.Length != localContent.Length || remoteContent != localContent)
            {
                webResource.Content = localContent;

                var result = await crmRequest.UploadWebresourceAsync(environmentConfig.ConnectionString.BuildConnectionString(), webResource);
                result.EnsureSuccess();

                await logger.WriteLineAsync($"{webResource.Name} uploaded from " + relativePath);
                return true;
            }
            else
            {
                await logger.WriteLineAsync(webResourceName + " has no changes");
                return false;
            }
        }

        private async Task<IEnumerable<WebResource>> RetrieveSelectedSolutionWrsAsync(EnvironmentConfig environmentConfig, IEnumerable<string> selectedFiles, Dictionary<string, string> mappings, bool ignoreExtensions, bool extendedLog)
        {
            await logger.WriteLineAsync("Retrieving existing web resources", extendedLog);

            var webResourceNames = new List<string>();

            foreach (var filePath in selectedFiles)
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

            var retrieveWebResourceResponse = await crmRequest.RetrieveSolutionWebResourcesAsync(environmentConfig.ConnectionString.BuildConnectionString(), environmentConfig.SelectedSolutionId, webResourceNames);
            if (retrieveWebResourceResponse.IsSuccessful == false)
            {
                throw new Exception($"Failed to retrieve web resource: {retrieveWebResourceResponse.ErrorMessage}");
            }

            return retrieveWebResourceResponse.Payload;
        }

        private async Task<IEnumerable<WebResource>> RetrieveWrsAsync(EnvironmentConfig environmentConfig, IEnumerable<string> selectedFiles, Dictionary<string, string> mappings, bool ignoreExtensions, bool extendedLog)
        {
            await logger.WriteLineAsync("Retrieving existing web resources", extendedLog);

            var webResourceNames = GetWrNames(selectedFiles, mappings, ignoreExtensions);

            var retrieveWebResourceResponse = await crmRequest.RetrieveWebResourcesAsync(environmentConfig.ConnectionString.BuildConnectionString(), webResourceNames);
            if (retrieveWebResourceResponse.IsSuccessful == false)
            {
                throw new Exception($"Failed to retrieve web resource: {retrieveWebResourceResponse.ErrorMessage}");
            }

            return retrieveWebResourceResponse.Payload;
        }

        private IEnumerable<string> GetWrNames(IEnumerable<string> filesPathes, Dictionary<string, string> mappings, bool ignoreExtensions)
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

        private async Task PublishWrAsync(EnvironmentConfig environmentConfig, IEnumerable<Guid> webresourcesIds)
        {
            await logger.WriteLineWithTimeAsync("Publishing...");
            await vsDteService.SetStatusBarAsync("Publishing...");

            if (webresourcesIds == null || !webresourcesIds.Any())
            {
                throw new ArgumentNullException("webresourcesId");
            }
            if (webresourcesIds.Any())
            {
                var result = await crmRequest.PublishWebResourcesAsync(environmentConfig.ConnectionString.BuildConnectionString(), webresourcesIds);
                result.EnsureSuccess();
            }
            var count = webresourcesIds.Count();
            await logger.WriteLineWithTimeAsync(count + " file" + (count == 1 ? " was" : "s were") + " published");
        }

        private string GetEncodedFileContent(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] binaryData = new byte[fs.Length];
                fs.Read(binaryData, 0, (int)fs.Length);
                fs.Close();

                return Convert.ToBase64String(binaryData, 0, binaryData.Length);
            }
        }

        private string GetFileContent(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var binaryData = new byte[fs.Length];
                fs.Read(binaryData, 0, (int)fs.Length);
                fs.Close();

                return Encoding.UTF8.GetString(binaryData);
            }
        }

        private async Task OperationStart(string message, string statusBarMessage = null)
        {
            await vsDteService.SaveAllAsync();
            await logger.ClearAsync();

            if (!string.IsNullOrEmpty(message))
            {
                await logger.WriteLineWithTimeAsync(message);
            }

            if (!string.IsNullOrEmpty(statusBarMessage))
            {
                await vsDteService.SetStatusBarAsync(statusBarMessage);
            }
        }

        private async Task OperationEnd(string message, string statusBarMessage = null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                await logger.WriteLineWithTimeAsync(message);
            }

            if (!string.IsNullOrEmpty(statusBarMessage))
            {
                await vsDteService.SetStatusBarAsync(statusBarMessage);
            }
        }
    }

    //public async Task PublishWrToEnvironmentsAsync(ProjectInfo projectInfo, ProjectConfig projectConfig, IEnumerable<EnvironmentConfig> environmentsConfigs, bool selectedItemsOnly)
    //{

    //}
}

