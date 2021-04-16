using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arksplorer
{
    public static class Globals
    {

        public const string Version = "v1.2.1 - The Snaggled Tooth";

        public static WebBrowser ArkpediaBrowser { get; set; }
        public static WebBrowser DododexBrowser { get; set; }

        /// <summary>
        /// Static reference to main app window that is decoupled from Application.Current.MainWindow (which e.g. can point to splash screen during start up and not an instance of the main window)
        /// </summary>
        public static MainWindow MainWindow { get; set; }
    }
}
