//using System.Windows.Shapes;

namespace Arksplorer
{
    public class QueueDataItem
    {
        public string MapName { get; set; }
        public MetaData MetaData { get; set; }
        public string DataUri { get; set; }
        public string TimestampUri { get; set; }
        public bool ForceRefresh { get; set; }
    }
}
