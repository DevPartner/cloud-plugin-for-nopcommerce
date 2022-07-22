using DevPartner.Nop.Plugin.CloudStorage.Domain;
using Nop.Data.Mapping;
using System;
using System.Collections.Generic;

namespace DevPartner.Nop.Plugin.CloudStorage.Migrations.Mapping
{
    public class CloudNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(MovingItem), "DP_Cloud_MovingItems" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}
