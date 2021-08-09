using System.Text.Json.Serialization;

namespace Arksplorer.Util
{
    public class LatestVersionInfo
    {
        [JsonPropertyName("latestVersion")]
        public string LatestVersion { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
