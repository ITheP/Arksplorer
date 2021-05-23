using System.Windows;

namespace Arksplorer
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            Version.Text = Globals.Version;
        }
    }
}