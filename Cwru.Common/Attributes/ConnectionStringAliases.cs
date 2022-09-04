using System;

namespace Cwru.Common.Attributes
{
    internal class ConnectionStringAliases : Attribute
    {
        public string[] Aliases { get; private set; }

        public ConnectionStringAliases(params string[] aliases)
        {
            this.Aliases = aliases;
        }
    }
}
