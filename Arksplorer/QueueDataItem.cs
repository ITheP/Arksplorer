//using System.Windows.Shapes;

using System;

namespace Arksplorer
{
    public class QueueDataItem
    {
        public string MapName { get; set; }
        public MetaData MetaData { get; set; }
        public Uri DataUri { get; set; }
        public Uri TimestampUri { get; set; }
        public bool ForceRefresh { get; set; }
    }
}
