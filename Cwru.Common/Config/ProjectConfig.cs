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
            SelectedEnvironments = new List<Guid>();
        }

        public Guid ProjectId { get; set; }
        public string Version { get; set; }
        public bool PublishAfterUpload { get; set; }
        public bool IgnoreExtensions { get; set; }
        //public bool ExtendedLog { get; set; }
        public Guid? DafaultEnvironmentId { get; set; }
        public List<Guid> SelectedEnvironments { get; set; }
        public List<EnvironmentConfig> Environments { get; set; }

        public object Clone()
        {
            return new ProjectConfig()
            {
                ProjectId = ProjectId,
                Version = Version,
                PublishAfterUpload = PublishAfterUpload,
                IgnoreExtensions = IgnoreExtensions,
                DafaultEnvironmentId = DafaultEnvironmentId,
                Environments = Environments?.Select(x => (EnvironmentConfig)x.Clone()).ToList()
            };
        }
    }
}
