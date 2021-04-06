using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

namespace Arksplorer
{
    public class TameDino : IArkEntityWithCreature
    {
        [JsonIgnore]
        public string Map { get; set; }
        [JsonIgnore]
        public string Creature { get; set; }
        [JsonPropertyName("creature")]
        public string CreatureId { get; set; }
        public string Name { get; set; }
        public string Tribe { get; set; }
        [JsonConverter(typeof(SexConverter))]
        public BitmapImage Sex { get; set; }
        public int Base { get; set; }
        public int Lvl { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        [JsonPropertyName("cryo")]
        [JsonConverter(typeof(CryoConverter))]
        public BitmapImage Cryo { get; set; }
        [JsonPropertyName("hp-w")]
        public int HpW { get; set; }
        [JsonPropertyName("stam-w")]
        public int StamW { get; set; }
        [JsonPropertyName("melee-w")]
        public int MeleeW { get; set; }
        [JsonPropertyName("weight-w")]
        public int WeightW { get; set; }
        [JsonPropertyName("speed-w")]
        public int SpeedW { get; set; }
        [JsonPropertyName("food-w")]
        public int FoodW { get; set; }
        [JsonPropertyName("oxy-w")]
        public int OxyW { get; set; }
        [JsonPropertyName("craft-t")]
        public int CraftT { get; set; }
        public string Tamer { get; set; }
        public string Imprinter { get; set; }
        public object Imprint { get; set; }
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
        [JsonPropertyName("mut-f")]
        public int MutF { get; set; }
        [JsonPropertyName("mut-m")]
        public int MutM { get; set; }
        public bool Viv { get; set; }
        public string Ccc { get; set; }
        //public long Id { get; set; } <-- not bothered about seeing this
        //long TribeId { get; set; } <-- not bothered about seeing this
    }
}
