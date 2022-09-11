﻿using Cwru.Common;
using Cwru.Connection.Services;
using Cwru.Publisher;
using Cwru.VsExtension.Commands.Base;
using System;
using System.Threading.Tasks;

namespace Cwru.VsExtension.Commands
{
    internal sealed class UpdateWrCommand : IBaseCommand
    {
        private readonly Logger logger;
        private readonly ConnectionService connectionService;
        private readonly PublishService publishService;

        public UpdateWrCommand(Logger logger, ConnectionService connectionService, PublishService publishService)
        {
            this.logger = logger;
            this.connectionService = connectionService;
            this.publishService = publishService;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var result = await connectionService.GetAndValidateConnectionAsync();
                if (result.IsValid)
                {
                    await publishService.UploadWrToDefaultEnvironmentAsync(result.ProjectInfo, result.ProjectConfig, false);
                }
                else
                {
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        await logger.WriteLineAsync(result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.WriteAsync(ex);
            }
        }
    }
}