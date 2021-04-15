using System.Diagnostics;
using System.Net.Http;

namespace Arksplorer
{
    public static class Web
    {
        public static HttpClient HttpClient { get; } = new();

        public static void OpenUrlInExternalBrowser(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
