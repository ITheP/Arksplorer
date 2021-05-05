using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Arksplorer
{
    public static class Globals
    {

        public const string Version = "v1.3.0 - The Flappy Wing";

        public static WebBrowser ArkpediaBrowser { get; set; }

        public static WebBrowser DododexBrowser { get; set; }

        public static WebBrowser YouTubeBrowser { get; set; }

        /// <summary>
        /// Static reference to main app window that is decoupled from Application.Current.MainWindow (which e.g. can point to splash screen during start up and not an instance of the main window)
        /// </summary>
        public static MainWindow MainWindow { get; set; }

        public static string CrashFile { get; } = "./crash.txt";

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

        public static void AddToCrashFile(string content)
        {
            try
            {
                content = $"===[Arksplorer {Version} @ {DateTime.Now}]================================{Environment.NewLine}{content}{Environment.NewLine}";
                File.AppendAllText(CrashFile, content);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to write log to {CrashFile}. Not going so well is it!{Environment.NewLine}{ex.Message}", "Error logging an error!");
            }
        }
    }
}
