//using System.Windows.Shapes;

using System.Text.Json.Serialization;

namespace Arksplorer
{
    public interface IArkEntity
    {
        /// <summary>
        /// Map this instance appears on
        /// </summary>
        public string Map { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
    }
}
