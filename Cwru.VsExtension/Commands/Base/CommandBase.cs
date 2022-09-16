using Cwru.Common;
using System;
using System.Threading.Tasks;

namespace Cwru.VsExtension.Commands.Base
{
    internal abstract class CommandBase : IBaseCommand
    {
        protected readonly Logger logger;

        public CommandBase(Logger logger)
        {
            this.logger = logger;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                await ExecuteInternalAsync();
            }
            catch (Exception ex)
            {
                await logger.WriteAsync(ex);
            }
        }

        protected abstract Task ExecuteInternalAsync();
    }
}
