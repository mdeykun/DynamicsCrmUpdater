using System;

namespace Cwru.Common.Model
{
    public class SolutionDetail : IEquatable<SolutionDetail>
    {
        public Guid SolutionId { get; set; }
        public Guid EnvironmentId { get; set; }
        public string FriendlyName { get; set; }
        public string UniqueName { get; set; }
        public string PublisherPrefix { get; set; }

        public bool Equals(SolutionDetail other)
        {
            return this.SolutionId == other.SolutionId && this.EnvironmentId == other.EnvironmentId;
        }

        public override string ToString()
        {
            return this.FriendlyName;
        }
    }
}
