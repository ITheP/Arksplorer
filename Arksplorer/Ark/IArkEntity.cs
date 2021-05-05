//using System.Windows.Shapes;

using System.Text.Json.Serialization;

namespace Arksplorer
{
    public interface IArkEntity
    {
        /// <summary>
        /// A global unique reference to this entity. ALWAYS SET AS FIRST VALUE IN CLASS DEFINITION SO WILL BE INDEXED AS [0]
        /// </summary>
        public int GlobalIndex { get; set; }
        /// <summary>
        /// Map this instance appears on. ALWAYS SET AS FIRST VALUE IN CLASS DEFINITION SO WILL BE INDEXED AS [1]
        /// </summary>
        public string Map { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
    }
}
