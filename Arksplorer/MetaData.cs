//using System.Windows.Shapes;

using System;

namespace Arksplorer
{
    public class MetaData
    {
        /// <summary>
        /// Class type is needed for JSON decoding, i.w. what to map the JSON data too
        /// </summary>
        public Type JsonClassType { get; set; }
        public string ArkEntityType { get; set; }
        public string Description { get; set; }
        public string NormalSearch { get; set; }
        public string WildcardSearch { get; set; }
        public bool IncludesCreatures { get; set; }
        public bool IncludesLevel { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
