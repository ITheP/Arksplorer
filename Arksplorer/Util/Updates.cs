using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Arksplorer.Util
{
    public class VersionInfo
    {
        [JsonPropertyName("latestVersion")]
        public string LatestVersion { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    /// <summary>
    /// Version checking
    /// </summary>
    public class Updates
    {
        private const string ServerVersionUrl = "https://ithep.github.io/Test/version.json";

        public async void CheckForUpdate()
        {
            try
            {
                VersionInfo versionInfo = await Web.HttpClient.GetFromJsonAsync<VersionInfo>(ServerVersionUrl);

                if (string.Compare(versionInfo.LatestVersion, Globals.VersionNumber) == 1)
                {
                    Debug.Print($"Newer version flagged on github: {versionInfo.LatestVersion} - {versionInfo.Name} ({versionInfo.Url})");
                    // Newer version available, we don't force anything here but give the user an option to upgrade
                }
                else
                {
                    Debug.Print("Github checked, no newer version available");
                }
            }
            catch (HttpRequestException ex)
            {
                Errors.ReportProblem(ex, $"Error retrieving latest version data from {ServerVersionUrl}: {ex.StatusCode}");
            }
            catch (NotSupportedException ex)
            {
                Errors.ReportProblem(ex, $"Invalid content type in JSON version data from {ServerVersionUrl}");
            }
            catch (JsonException ex)
            {
                Errors.ReportProblem(ex, $"Invalid JSON retrieving JSON version data from {ServerVersionUrl}");
            }
            catch (Exception ex)
            {
                Errors.ReportProblem(ex, $"Problem loading JSON version data from {ServerVersionUrl}");
            }
        }
    }
}
