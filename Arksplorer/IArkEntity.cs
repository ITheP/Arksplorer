//using System.Windows.Shapes;

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

    public interface IArkEntityWithCreature : IArkEntity
    {
        public string Creature { get; set; }
        public string CreatureId { get; set; }
    }
}
