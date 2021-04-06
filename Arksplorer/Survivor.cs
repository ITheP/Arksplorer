﻿//using System.Windows.Shapes;

using System.Text.Json.Serialization;

namespace Arksplorer
{
    public class Survivor : IArkEntity
    {
        [JsonIgnore]
        public string Map { get; set; }
        public long PlayerId { get; set; }
        public string Steam { get; set; }
        public string Name { get; set; }
        public object TribeId { get; set; }
        public string Tribe { get; set; }
        [JsonConverter(typeof(SexConverter))]
        public string Sex { get; set; }
        public int Lvl { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public int Hp { get; set; }
        public int Stam { get; set; }
        public int Melee { get; set; }
        public int Weight { get; set; }
        public int Speed { get; set; }
        public int Food { get; set; }
        public int Water { get; set; }
        public int Oxy { get; set; }
        public int Craft { get; set; }
        public int Fort { get; set; }
        public string Active { get; set; }
        public string Ccc { get; set; }
    }
}
