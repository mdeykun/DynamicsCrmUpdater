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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cwru.Publisher.Services
{
    public class UpdateWrService : PublisherBaseService
    {
        private readonly MappingService mappingService;
        private readonly ICrmRequests crmRequest;
        private readonly SolutionsService solutionsService;
        private readonly ConfigurationService configurationService;

        public UpdateWrService(
            Logger logger,
            ICrmRequests crmWebResourcesUpdaterClient,
            MappingService mappingHelper,
            SolutionsService solutionsService,
            VsDteService vsDteService,
            ConfigurationService configurationService) : base(logger, vsDteService)
        {
            this.crmRequest = crmWebResourcesUpdaterClient;
            this.mappingService = mappingHelper;
            this.solutionsService = solutionsService;
            this.configurationService = configurationService;
        }

        public async Task UploadWrEnvironmentsAsync(ProjectInfo projectInfo, ProjectConfig projectConfig, bool selectedItemsOnly)
        {
            await vsDteService.SaveAllAsync();
            await OperationStartAsync("Uploading web resources...", "Uploading...");

            var dialog = new SelectEnvironmentsForm(projectConfig);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                projectConfig.SelectedEnvironments = dialog.SelectedEnvironments.Select(x => x.Id).ToList();
                configurationService.Save(projectConfig);

                for (var i = 0; i < dialog.SelectedEnvironments.Count; i++)
                {
                    var environment = dialog.SelectedEnvironments[i];

                    if (i != 0)
                    {
                        await logger.WriteLineAsync();
                    }
                    await logger.WriteEnvironmentInfoAsync(environment);

                    var result = await UploadWrAsync(projectConfig, environment, projectInfo, selectedItemsOnly);

                    await OperationEndAsync(result);
                }
            }
            else
            {
                var result = new Result() { ResultType = ResultType.Canceled };
                await OperationEndAsync(result);
            }
        }

        public async Task UploadWrDefaultEnvironmentAsync(ProjectInfo projectInfo, ProjectConfig projectConfig, bool selectedItemsOnly)
        {
            var environment = projectConfig.GetDefaultEnvironment();

            await vsDteService.SaveAllAsync();
            await OperationStartAsync("Uploading web resources...", "Uploading...", environment);

            var result = await UploadWrAsync(projectConfig, environment, projectInfo, selectedItemsOnly);

            await OperationEndAsync(result);
        }

        private async Task<Result> UploadWrAsync(ProjectConfig projectConfig, EnvironmentConfig environmentConfig, ProjectInfo projectInfo, bool selectedItemsOnly)
        {
            var updatedWrs = new List<WebResource>();
            var total = 0;

            try
            {
                var filesToUpload = await GetProjectFilesAsync(projectInfo, selectedItemsOnly, projectConfig.ExtendedLog);
                total = filesToUpload.Count();
                if (total <= 0)
                {
                    return new Result(ResultType.Failure, 0, 0, 0, errorMessage: "No files found to publish");
                }

                var selectedSolution = await solutionsService.GetDefaultSolutionDetailsAsync(environmentConfig);
                await logger.WriteSolutionInfoAsync(selectedSolution);

                await logger.WriteLineAsync("Starting uploading process", projectConfig.ExtendedLog);
                await logger.WriteLineAsync("--------------------------------------------------------------");

                var mappings = mappingService.LoadMappings(projectInfo);

                var webResourceNames = GetWrNames(filesToUpload, mappings, projectConfig.IgnoreExtensions);
                var webResources = await RetrieveSolutionWrsAsync(environmentConfig.ConnectionString.BuildConnectionString(), environmentConfig.SelectedSolutionId, webResourceNames, projectConfig.IgnoreExtensions, projectConfig.ExtendedLog);
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

                return new Result(ResultType.Success, total: filesToUpload.Count(), processed: updatedWrs.Count, failed: 0);
            }
            catch (Exception ex)
            {
                await logger.WriteLineAsync($"Failed to upload script{total.Select("", "s")}.");
                await logger.WriteLineAsync(ex, projectConfig.ExtendedLog);

                return new Result(ResultType.Failure, total: total, processed: updatedWrs.Count, failed: 0, ex);
            }
        }

        private async Task<bool> UpdateWrByFileAsync(EnvironmentConfig environmentConfig, WebResource webResource, string filePath, string relativePath, bool extendedLog)
        {
            var webResourceName = Path.GetFileName(filePath);
            await logger.WriteLineAsync("Uploading " + webResourceName, extendedLog);

            var localContent = FilesHelper.GetEncodedFileContent(filePath);
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

        private async Task<IEnumerable<WebResource>> RetrieveSolutionWrsAsync(string connectionString, Guid solutionId, IEnumerable<string> webResourceNames, bool ignoreExtensions, bool extendedLog)
        {
            await logger.WriteLineAsync("Retrieving existing web resources", extendedLog);

            var retrieveWebResourceResponse = await crmRequest.RetrieveSolutionWebResourcesAsync(connectionString, solutionId, webResourceNames);
            if (retrieveWebResourceResponse.IsSuccessful == false)
            {
                throw new Exception($"Failed to retrieve web resource: {retrieveWebResourceResponse.ErrorMessage}");
            }

            return retrieveWebResourceResponse.Payload;
        }

        private async Task OperationEndAsync(Result result)
        {
            await OperationEndAsync(result,
                $"{result.Processed} web resource{result.Processed.Select(" was", "s were")} uploaded",
                $"Failed to upload web resource{result.Total.Select("", "s")}",
                $"Operation was canceled");
        }
    }
}
