using System.Text.Json.Serialization;
//using System.Windows.Shapes;

namespace Arksplorer
{
    public class TameDino : IArkEntity
    {
        public string Creature { get; set; }
        public string Name { get; set; }
        public string Tribe { get; set; }
        public int Sex { get; set; }
        public int Base { get; set; }
        public int Lvl { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public bool Cryo { get; set; }
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
        public int C0 { get; set; }
        public int C1 { get; set; }
        public int C2 { get; set; }
        public int C3 { get; set; }
        public int C4 { get; set; }
        public int C5 { get; set; }
        [JsonPropertyName("mut-f")]
        public int MutF { get; set; }
        [JsonPropertyName("mut-m")]
        public int MutM { get; set; }
        public bool Viv { get; set; }
        public string Ccc { get; set; }
        public long Id { get; set; }
        public long TribeId { get; set; }
    }
}
