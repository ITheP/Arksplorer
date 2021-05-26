using Arksplorer.Controls;

namespace Arksplorer
{
    public static class Globals
    {
        public static string VersionNumber = "v1.4.03";
        public static string VersionName = "The Distant Roar";
        public static string Version = $"{VersionNumber} - {VersionName}";

        public static WebBrowser ArkpediaBrowser { get; set; }

        public static WebBrowser DododexBrowser { get; set; }
        public static WebBrowser ArkbuddyBrowser { get; set; }

        public static WebBrowser YouTubeBrowser { get; set; }

        /// <summary>
        /// Static reference to main app window that is decoupled from Application.Current.MainWindow (which e.g. can point to splash screen during start up and not an instance of the main window)
        /// </summary>
        public static MainWindow MainWindow { get; set; }

        /// <summary>
        /// Common across all datasets
        /// </summary>
        public static int GlobalIndexColumn { get; set; } = 0;
        /// <summary>
        /// Common across all datasets
        /// </summary>
        public static int MapColumn { get; set; } = 1;

        /// <summary>
        /// Global Index is a never repeating id for data, used for cross looking up rows across datasets.
        /// Ideally we can just check e.g. DataRow instances, but DataGrid fun and games appears to mean these aren't reliable,
        /// tests showed identical DataRow sets of data in the DataGrid + instance referenced elsewhere were not equal.
        /// </summary>
        public static int GlobalIndex { get; set; }

    }
}
