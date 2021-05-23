using System;

namespace Arksplorer
{
    public class MetaData
    {
        public Type JsonClassType { get; set; }
        public string ArkEntityType { get; set; }
        public string Description { get; set; }
        public string NormalSearch { get; set; }
        public string WildcardSearch { get; set; }
        public bool IncludesCreatures { get; set; }
        public bool IncludesLevel { get; set; }
        public bool IncludesColors { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
