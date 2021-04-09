using System.Net.Http;

namespace Arksplorer
{
    public static class Web
    {
        public static HttpClient HttpClient { get; } = new();
    }
}
