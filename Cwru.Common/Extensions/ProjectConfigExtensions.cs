using Cwru.Common.Config;
using System.Linq;

namespace Cwru.Common.Extensions
{
    public static class ProjectConfigExtensions
    {
        public static EnvironmentConfig GetDefaultEnvironment(this ProjectConfig projectConfig)
        {
            if (projectConfig.DafaultEnvironmentId == null)
            {
                return null;
            }

            var result = projectConfig.Environments.Where(x => x.Id == projectConfig.DafaultEnvironmentId).FirstOrDefault();
            return result;
        }

        public static string GetDefaultConnectionString(this ProjectConfig projectConfig)
        {
            return projectConfig.GetDefaultEnvironment()?.ConnectionString?.BuildConnectionString();
        }
    }
}
