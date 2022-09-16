using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Model;
using Cwru.Connection.Services;
using Cwru.Publisher;
using Cwru.VsExtension.Commands.Base;
using System.Threading.Tasks;

namespace Cwru.VsExtension.Commands
{
    internal class DownloadWrsCommand : PublisherCommandBase
    {
        private readonly PublishService publishService;

        public DownloadWrsCommand(Logger logger, ConnectionService connectionService, PublishService publishService) : base(logger, connectionService)
        {
            this.publishService = publishService;
        }

        protected override async Task ExecutePublisherLogicAsync(ProjectInfo projectInfo, ProjectConfig projectConfig)
        {
            await publishService.DownloadWrsAsync(projectInfo, projectConfig);
        }
    }
}
