using System;
using System.Collections.Generic;
using System.Linq;

namespace Cwru.Common.Config
{
    public class ProjectConfig : ICloneable
    {
        public ProjectConfig()
        {
            Environments = new List<EnvironmentConfig>();
        }

        public Guid ProjectId { get; set; }
        public string Version { get; set; }
        public bool PublishAfterUpload { get; set; }
        public bool IgnoreExtensions { get; set; }
        public bool ExtendedLog { get; set; }
        public Guid? SelectedEnvironmentId { get; set; }
        public List<EnvironmentConfig> Environments { get; set; }

        public object Clone()
        {
            return new ProjectConfig()
            {
                ProjectId = ProjectId,
                Version = Version,
                PublishAfterUpload = PublishAfterUpload,
                IgnoreExtensions = IgnoreExtensions,
                ExtendedLog = ExtendedLog,
                SelectedEnvironmentId = SelectedEnvironmentId,
                Environments = Environments?.Select(x => (EnvironmentConfig)x.Clone()).ToList()
            };
        }
    }
}
