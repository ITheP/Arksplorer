using System.Text.Json.Serialization;
using System.Windows.Media;

namespace Arksplorer
{
    public class ArkColor
    {
        public int Id { get; set; }
        public string Hex { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public Brush Color { get; set; }
    }
}
