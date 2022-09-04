using Cwru.Common;
using Cwru.Connection.Services;
using Cwru.Publisher;
using Cwru.VsExtension.Commands.Base;
using System;
using System.Threading.Tasks;

namespace Cwru.VsExtension.Commands
{
    internal sealed class UpdateWebResourcesCommand : IBaseCommand
    {
        private readonly Logger logger;
        private readonly ConnectionService connectionService;
        private readonly PublishService publishService;

        public UpdateWebResourcesCommand(Logger logger, ConnectionService connectionService, PublishService publishService)
        {
            this.logger = logger;
            this.connectionService = connectionService;
            this.publishService = publishService;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var result = await connectionService.EnsurePasswordIsSet();
                if (result)
                {
                    await publishService.PublishWebResourcesAsync(false);
                }
            }
            catch (Exception ex)
            {
                await logger.WriteAsync(ex);
            }
        }
    }
}
