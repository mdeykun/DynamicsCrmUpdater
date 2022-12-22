using Cwru.Common;
using Cwru.Connection.Services;
using Cwru.VsExtension.Commands.Base;
using System.Threading.Tasks;

namespace Cwru.VsExtension.Commands
{
    internal sealed class UpdaterOptionsCommand : CommandBase
    {
        private readonly ConnectionService connectionService;

        public UpdaterOptionsCommand(Logger logger, ConnectionService connectionService) : base(logger)
        {
            this.connectionService = connectionService;
        }

        protected override async Task ExecuteInternalAsync()
        {
            await logger.WriteDebugAsync("UpdaterOptionsCommand: Command started");
            await connectionService.ShowConfigurationDialogAsync();
            await logger.WriteDebugAsync("UpdaterOptionsCommand: Command completed");
        }
    }
}
