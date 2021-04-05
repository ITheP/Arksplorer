//using System.Windows.Shapes;

using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Arksplorer
{
    public class WildDino : IArkEntityWithCreature
    {
        [JsonIgnore]
        public string Map { get; set; }
        [JsonIgnore]
        public string Creature { get; set; }
        [JsonPropertyName("creature")]
        public string CreatureId { get; set; }
        [JsonConverter(typeof(SexConverter))]
        public BitmapImage Sex { get; set; }
        public int Lvl { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public int Hp { get; set; }
        public int Stam { get; set; }
        public int Melee { get; set; }
        public int Weight { get; set; }
        public int Speed { get; set; }
        public int Food { get; set; }
        public int Oxy { get; set; }
        public int Craft { get; set; }
        [JsonConverter(typeof(ArkColorConverter))]
        public ArkColor C0 { get; set; }
        [JsonConverter(typeof(ArkColorConverter))]
        public ArkColor C1 { get; set; }
        [JsonConverter(typeof(ArkColorConverter))]
        public ArkColor C2 { get; set; }
        [JsonConverter(typeof(ArkColorConverter))]
        public ArkColor C3 { get; set; }
        [JsonConverter(typeof(ArkColorConverter))]
        public ArkColor C4 { get; set; }
        [JsonConverter(typeof(ArkColorConverter))]
        public ArkColor C5 { get; set; }
        //public string Ccc { get; set; } <-- not bothered about seeing this
        //public long Id { get; set; } <-- not bothered about seeing this
    }
}
