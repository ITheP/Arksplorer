//using System.Windows.Shapes;

namespace Arksplorer
{
    public interface IArkEntityWithCreature : IArkEntity
    {
        public string Creature { get; set; }
        public string CreatureId { get; set; }
        public ArkColor C0 { get; set; }
        public int C0_Sort { get; set; }
        public ArkColor C1 { get; set; }
        public int C1_Sort { get; set; }
        public ArkColor C2 { get; set; }
        public int C2_Sort { get; set; }
        public ArkColor C3 { get; set; }
        public int C3_Sort { get; set; }
        public ArkColor C4 { get; set; }
        public int C4_Sort { get; set; }
        public ArkColor C5 { get; set; }
        public int C5_Sort { get; set; }
    }
}
