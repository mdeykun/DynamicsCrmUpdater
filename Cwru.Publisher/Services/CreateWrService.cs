using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Extensions;
using Cwru.Common.Model;
using Cwru.Common.Services;
using Cwru.CrmRequests.Common.Contracts;
using Cwru.Publisher.Forms;
using Cwru.Publisher.Helpers;
using Cwru.Publisher.Model;
using Cwru.Publisher.Services.Base;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cwru.Publisher.Services
{
    public class CreateWrService : PublisherBaseService
    {
        private readonly MappingService mappingService;
        private readonly ICrmRequests crmRequest;
        private readonly SolutionsService solutionsService;
        private readonly WebResourceTypesService webResourceTypesService;

        public CreateWrService(
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

        public async Task CreateWrDefaultEnvironmentAsync(ProjectInfo projectInfo, ProjectConfig projectConfig)
        {
            var environment = projectConfig.GetDefaultEnvironment();

            await vsDteService.SaveAllAsync();
            await OperationStartAsync("Creating web resource...", "Creating...", environment);

            var result = await CreateWrAsync(projectInfo, projectConfig, environment);

            await OperationEndAsync(result,
                "Web resource was created",
                "Failed to create web resource",
                "Web resource creation was canceled");
        }

        private async Task<Result> CreateWrAsync(ProjectInfo projectInfo, ProjectConfig projectConfig, EnvironmentConfig environmentConfig)
        {
            try
            {
                var defaultSolution = await solutionsService.GetDefaultSolutionDetailsAsync(environmentConfig);
                await logger.WriteLineAsync($"Solution: {defaultSolution?.FriendlyName}");

                var filePath = projectInfo.GetSelectedFilePath();
                await logger.WriteLineAsync($"File path: {filePath?.RemoveRoot(projectInfo.Root)}");

                var dialog = new CreateWebResourceForm(filePath, defaultSolution, webResourceTypesService);
                var dialogResult = dialog.ShowDialog();

                var result = new Result() { ResultType = ResultType.Canceled };
                if (dialogResult == DialogResult.OK)
                {
                    await logger.WriteLineAsync($"Web resource: {dialog.WebResource?.Name}");
                    result = await CreateWrAsync(projectInfo, projectConfig.GetDefaultEnvironment(), dialog.WebResource, filePath);
                }

                return result;
            }
            catch (Exception ex)
            {
                return new Result(ResultType.Failure, total: 1, processed: 0, failed: 1, exception: ex);
            }
        }

        private async Task<Result> CreateWrAsync(ProjectInfo projectInfo, EnvironmentConfig environment, WebResource webResource, string filePath)
        {
            webResource.Content = FilesHelper.GetEncodedFileContent(filePath);

            var isWebResourceExistsResponse = await crmRequest.IsWebResourceExistsAsync(environment.ConnectionString.BuildConnectionString(), webResource.Name);
            if (isWebResourceExistsResponse.IsSuccessful == false)
            {
                return GetFailedResult($"Failed to validate web resource existence: {isWebResourceExistsResponse.ErrorMessage}");
            }

            if (isWebResourceExistsResponse.Payload)
            {
                return GetFailedResult("Web resource '" + webResource.Name + "' already exists in CRM");
            }

            var mappings = await mappingService.LoadMappingsAsync(projectInfo);
            var mapping = mappingService.GetMappingByFilePath(mappings, filePath);
            if (!string.IsNullOrWhiteSpace(mapping) && !mapping.IsEqualToLower(webResource.Name))
            {
                return GetFailedResult($"File ${filePath.RemoveRoot(projectInfo.Root)} is alredy mapped to another web resource. Please check {MappingService.MappingFileName}");
            }

            if (mappingService.IsMappingRequired(mappings, filePath, webResource.Name))
            {
                await mappingService.CreateMappingAsync(projectInfo, filePath, webResource.Name);
            }

            var defaultSolution = await solutionsService.GetDefaultSolutionDetailsAsync(environment);
            var createWebResourceResponse = await crmRequest.CreateWebresourceAsync(environment.ConnectionString.BuildConnectionString(), webResource, defaultSolution.UniqueName);
            createWebResourceResponse.EnsureSuccess();

            return new Result(ResultType.Success, total: 1, processed: 1, failed: 0);
        }

        public Result GetFailedResult(string errorMessage)
        {
            return new Result(ResultType.Failure, total: 1, processed: 0, failed: 1, errorMessage: errorMessage);
        }
    }
}
