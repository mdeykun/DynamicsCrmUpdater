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
            var result = await connectionService.GetAndValidateConnectionAsync();
            if (result.IsJustConfigured)
            {
                return;
            }

            if (result.IsValid)
            {
                await ExecutePublisherLogicAsync(result.ProjectInfo, result.ProjectConfig);
            }
            else
            {
                if (!string.IsNullOrEmpty(result.Message))
                {
                    await logger.WriteLineAsync(result.Message);
                }
            }
        }

        protected abstract Task ExecutePublisherLogicAsync(ProjectInfo projectInfo, ProjectConfig projectConfig);
    }
}
