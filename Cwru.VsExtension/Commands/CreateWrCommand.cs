using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Model;
using Cwru.Connection.Services;
using Cwru.Publisher.Services;
using Cwru.VsExtension.Commands.Base;
using System.Threading.Tasks;

namespace Cwru.VsExtension.Commands
{
    internal sealed class CreateWrCommand : PublisherCommandBase
    {
        private readonly CreateWrService createWrService;

        public CreateWrCommand(Logger logger, ConnectionService connectionService, CreateWrService createWrService) : base(logger, connectionService)
        {
            this.createWrService = createWrService;
        }

        protected override async Task ExecutePublisherLogicAsync(ProjectInfo projectInfo, ProjectConfig projectConfig)
        {
            await createWrService.CreateWrDefaultEnvironmentAsync(projectInfo, projectConfig);
        }
    }
}
