using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Model;
using Cwru.Connection.Services;
using System.Threading.Tasks;

namespace Cwru.VsExtension.Commands.Base
{
    internal abstract class PublisherCommandBase : CommandBase
    {
        private readonly ConnectionService connectionService;

        protected PublisherCommandBase(Logger logger, ConnectionService connectionService) : base(logger)
        {
            this.connectionService = connectionService;
        }

        protected override async Task ExecuteInternalAsync()
        {
            await logger.WriteDebugAsync("PublisherCommandBase: Command started");

            await logger.WriteDebugAsync("PublisherCommandBase: Validating connection");
            var result = await connectionService.GetAndValidateConnectionAsync();
            await logger.WriteDebugAsync($"PublisherCommandBase: Validation result. Is Valid: {result.IsValid}, Is Just Configured: {result.IsJustConfigured}");

            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                await logger.WriteDebugAsync($"PublisherCommandBase: Validation Message: {result.Message}");
            }

            if (result.IsJustConfigured)
            {
                await logger.WriteDebugAsync("PublisherCommandBase: Command execution was skipped because new connection was just configured. Please try again.");
                return;
            }

            await logger.WriteDebugAsync($"PublisherCommandBase: ProjectInfo Root: {result.ProjectInfo?.Root}, Default environment Id: {result.ProjectConfig?.DafaultEnvironmentId}");

            if (result.IsValid)
            {
                await logger.WriteDebugAsync("PublisherCommandBase: Executing specific command logic");
                await ExecutePublisherLogicAsync(result.ProjectInfo, result.ProjectConfig);
                await logger.WriteDebugAsync("PublisherCommandBase: Command completed");
            }
        }

        protected abstract Task ExecutePublisherLogicAsync(ProjectInfo projectInfo, ProjectConfig projectConfig);
    }
}
