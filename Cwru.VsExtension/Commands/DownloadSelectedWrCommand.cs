using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Model;
using Cwru.Connection.Services;
using Cwru.Publisher.Services;
using Cwru.VsExtension.Commands.Base;
using System.Threading.Tasks;

namespace Cwru.VsExtension.Commands
{
    internal class DownloadSelectedWrCommand : PublisherCommandBase
    {
        private readonly DownloadWrService downloadWrService;

        public DownloadSelectedWrCommand(Logger logger, ConnectionService connectionService, DownloadWrService downloadWrService) : base(logger, connectionService)
        {
            this.downloadWrService = downloadWrService;
        }

        protected override async Task ExecutePublisherLogicAsync(ProjectInfo projectInfo, ProjectConfig projectConfig)
        {
            await downloadWrService.DownloadSelectedWrAsync(projectInfo, projectConfig);
        }
    }
}
