using System;
using System.Data;

namespace Arksplorer
{
    public class MapPackage
    {
        public DataTablePlus Data { get; set; }
        public string RawTimestamp { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime ApproxNextServerUpdateTimestamp { get; set; }
    }
}
