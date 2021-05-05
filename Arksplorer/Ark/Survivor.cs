//using System.Windows.Shapes;

using System;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Arksplorer
{
    public class Survivor : IArkEntity
    {
        [JsonIgnore]
        public int GlobalIndex { get; set; }    // Make sure first property so indexed at [0]
        [JsonIgnore]
        public string Map { get; set; }         // Make sure second property so indexed at [1]
        public long PlayerId { get; set; }
        public string Steam { get; set; }
        public string Name { get; set; }
        public object TribeId { get; set; }
        public string Tribe { get; set; }
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
        public int Water { get; set; }
        public int Oxy { get; set; }
        public int Craft { get; set; }
        public int Fort { get; set; }
        public DateTime Active { get; set; }
        public string Ccc { get; set; }
    }
}
