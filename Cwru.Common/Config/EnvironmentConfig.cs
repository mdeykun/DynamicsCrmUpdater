using Cwru.Common.Model;
using System;
using System.Diagnostics;

namespace Cwru.Common.Config
{
    [DebuggerDisplay("Name")]
    public class EnvironmentConfig : ICloneable
    {
        public Guid Id { get; set; }
        public bool SavePassword { get; set; }
        public long TimeoutTicks { get; set; }
        public Certificate Certificate { get; set; }
        public Guid SelectedSolutionId { get; set; }
        public bool IsUserProvidedConnectionString { get; set; }
        public CrmConnectionString ConnectionString { get; set; }

        #region Info Fields. Just to show in the grid
        public string Name { get; set; }
        public string Organization { get; set; }
        public string OrganizationVersion { get; set; }
        public string SolutionName { get; set; }
        #endregion

        public object Clone()
        {
            var cs = ConnectionString.BuildConnectionString();

            return new EnvironmentConfig()
            {
                Id = Id,
                Certificate = Certificate,
                ConnectionString = CrmConnectionString.Parse(cs),
                IsUserProvidedConnectionString = IsUserProvidedConnectionString,
                Name = Name,
                Organization = Organization,
                OrganizationVersion = OrganizationVersion,
                SolutionName = SolutionName,
                SavePassword = SavePassword,
                SelectedSolutionId = SelectedSolutionId,
                TimeoutTicks = TimeoutTicks
            };
        }
    }
}
