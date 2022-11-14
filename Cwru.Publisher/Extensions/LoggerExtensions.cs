using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Model;
using System.Threading.Tasks;

namespace Cwru.Publisher.Extensions
{
    public static class LoggerExtensions
    {
        public static async Task WriteEnvironmentInfoAsync(this ILogger logger, EnvironmentConfig environmentConfig)
        {
            await logger.WriteLineAsync($"Environment: {environmentConfig.Name} ({environmentConfig.ConnectionString.ServiceUri})");
        }

        public static async Task WriteSolutionInfoAsync(this ILogger logger, SolutionDetail solutionDetail)
        {
            await logger.WriteLineAsync("Solution: " + solutionDetail.FriendlyName);
        }
    }
}
