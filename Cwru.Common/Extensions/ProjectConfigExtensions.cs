using Cwru.Common.Config;
using System.Linq;

namespace Cwru.Common.Extensions
{
    public static class ProjectConfigExtensions
    {
        public static EnvironmentConfig GetSelectedEnvironment(this ProjectConfig projectConfig)
        {
            if (projectConfig.SelectedEnvironmentId == null)
            {
                return null;
            }

            var result = projectConfig.Environments.Where(x => x.Id == projectConfig.SelectedEnvironmentId).FirstOrDefault();
            return result;
        }
    }
}
