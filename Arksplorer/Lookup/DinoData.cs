using System.Text.Json.Serialization;

namespace Arksplorer
{
    public class DinoData
    {
        [JsonPropertyName("Arksplorer")]
        public string ArksplorerName { get; set; }
        [JsonPropertyName("Arkpedia")]
        public string ArkpediaName { get; set; }
        [JsonPropertyName("ArkpediaUrl")]
        public string ArkpediaUrl { get; set; }
        [JsonPropertyName("Dododex")]
        public string DododexName { get; set; }
        [JsonPropertyName("DododexUrl")]
        public string DododexUrl { get; set; }
        public string Maps { get; set; }
        public string Habitat { get; set; }
    }
}
