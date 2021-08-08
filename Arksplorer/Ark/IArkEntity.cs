using System.IO.Compression;

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
        /// <summary>
        /// Reference back to complete source data this row comes from (to get column Positioning references etc. later)
        /// </summary>
        public DataTablePlus DataParent { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
    }
}
