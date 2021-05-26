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
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Arksplorer.Util
{

    /// <summary>
    /// Version checking
    /// </summary>
    public class Updates
    {
        private const string ServerVersionUrl = "https://ithep.github.io/Arksplorer/version.json";

        public async void CheckForUpdate()
        {
            try
            {
                LatestVersionInfo latestVersionInfo = await Web.HttpClient.GetFromJsonAsync<LatestVersionInfo>(ServerVersionUrl);

                if (string.Compare(latestVersionInfo.LatestVersion, Globals.VersionNumber) == 1)
                {
                    Debug.Print($"Newer version flagged on github: {latestVersionInfo.LatestVersion} - {latestVersionInfo.Name} ({latestVersionInfo.Url})");
                    // Newer version available, we don't force anything here but give the user an option to upgrade

                    Globals.MainWindow.Dispatcher.Invoke(() =>
                    {
                        Update update = new(latestVersionInfo);
                        update.ShowDialog();
                    });
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
