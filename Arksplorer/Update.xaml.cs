using Arksplorer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Arksplorer
{
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    public partial class Update : Window
    {
        public Update(LatestVersionInfo latestVersionInfo)
        {
            InitializeComponent();

            NewVersion.Text = $"{latestVersionInfo.LatestVersion} - {latestVersionInfo.Name}";
            UpdateUrl.NavigateUri = new Uri(latestVersionInfo.Url);
        }

        private void HandleLinkClick(object sender, RequestNavigateEventArgs e)
        {
            string url = e.Uri.AbsoluteUri;
            Web.OpenUrlInExternalBrowser(url);

            e.Handled = true;

            // We quick out of things if we are downloading the update itself
            if (url.EndsWith(".zip"))
                Application.Current.Shutdown();
        }
    }
}