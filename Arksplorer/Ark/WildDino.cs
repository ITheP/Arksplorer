using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Arksplorer
{
    public class WildDino : IArkEntityWithCreature
    {
        [JsonIgnore]
        public int GlobalIndex { get; set; }    // Make sure first property so indexed at [0]
        [JsonIgnore]
        public string Map { get; set; }         // Make sure second property so indexed at [1]
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
        [JsonIgnore]
        public int C0_Sort { get; set; }
        [JsonConverter(typeof(ArkColorConverter))]
        public ArkColor C1 { get; set; }
        [JsonIgnore]
        public int C1_Sort { get; set; }
        [JsonConverter(typeof(ArkColorConverter))]
        public ArkColor C2 { get; set; }
        [JsonIgnore]
        public int C2_Sort { get; set; }
        [JsonConverter(typeof(ArkColorConverter))]
        public ArkColor C3 { get; set; }
        [JsonIgnore]
        public int C3_Sort { get; set; }
        [JsonConverter(typeof(ArkColorConverter))]
        public ArkColor C4 { get; set; }
        [JsonIgnore]
        public int C4_Sort { get; set; }
        [JsonConverter(typeof(ArkColorConverter))]
        public ArkColor C5 { get; set; }
        [JsonIgnore]
        public int C5_Sort { get; set; }
        public string Ccc { get; set; }
        //public long Id { get; set; } <-- not bothered about seeing this
    }
}
