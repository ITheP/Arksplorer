using System;
using System.Data;
//using System.Windows.Shapes;

namespace Arksplorer
{
    public class MapPackage
    {
        public DataTable Data { get; set; }
        public string RawTimestamp { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime ApproxNextServerUpdateTimestamp { get; set; }
    }
}
