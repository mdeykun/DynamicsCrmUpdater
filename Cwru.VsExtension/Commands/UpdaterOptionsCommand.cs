using Cwru.Common;
using Cwru.Connection.Services;
using Cwru.VsExtension.Commands.Base;
using System;
using System.Threading.Tasks;

namespace Cwru.VsExtension.Commands
{
    internal sealed class UpdaterOptionsCommand : IBaseCommand
    {
        private readonly Logger logger;
        private readonly ConnectionService connectionService;

        public UpdaterOptionsCommand(Logger logger, ConnectionService connectionService)
        {
            this.logger = logger;
            this.connectionService = connectionService;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                await connectionService.ShowConfigurationDialogAsync();
            }
            catch (Exception ex)
            {
                await logger.WriteAsync(ex);
            }
        }
    }
}
