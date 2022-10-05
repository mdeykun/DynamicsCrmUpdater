using Cwru.Common.Config;
using System;
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

            return projectConfig.GetEnvironment(projectConfig.DafaultEnvironmentId.Value);
        }

        public static EnvironmentConfig GetEnvironment(this ProjectConfig projectConfig, Guid environmentId)
        {
            var result = projectConfig.Environments.Where(x => x.Id == environmentId).FirstOrDefault();
            return result;
        }

        public static string GetDefaultConnectionString(this ProjectConfig projectConfig)
        {
            return projectConfig.GetDefaultEnvironment()?.ConnectionString?.BuildConnectionString();
        }
    }
}
