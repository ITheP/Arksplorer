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
    /// Interaction logic for InfoVisual.xaml
    /// </summary>
    public partial class InfoVisual : UserControl
    {
        private static DinoData CurrentDinoData { get; set; }

        private static MainWindow MainWindow { get; set; }

        public InfoVisual()
        {
            InitializeComponent();
            MainWindow = (MainWindow)Application.Current.MainWindow;
        }

        public void ShowInfo(Info info, bool ShowLinks = false)
        {
            if (info.Name == null)
            {
                Name.Visibility = Visibility.Collapsed;
            }
            else if (string.IsNullOrWhiteSpace(info.Name))
            {
                Name.Text = "Name not set";
                Name.FontStyle = FontStyles.Italic;
                Name.Visibility = Visibility.Visible;
            }
            else
            {
                Name.Text = info.Name;
                Name.FontStyle = FontStyles.Normal;
                Name.Visibility = Visibility.Visible;
            }

            Creature.Text = info.Creature;

            Lon.Text = $"{info.Lon:N2}";
            Lat.Text = $"{info.Lat:N2}";

            InfoList.ItemsSource = info.Items;
            IconList.ItemsSource = info.Icons;

            Arkpedia.Visibility = Visibility.Collapsed;
            Dododex.Visibility = Visibility.Collapsed;

            if (ShowLinks)
            {
                CurrentDinoData = Lookup.FindDino(info.CreatureId);
                if (CurrentDinoData != null)
                {
                    if (!string.IsNullOrWhiteSpace(CurrentDinoData.ArkpediaUrl))
                        Arkpedia.Visibility = Visibility.Visible;

                    if (!string.IsNullOrWhiteSpace(CurrentDinoData.DododexUrl))
                        Dododex.Visibility = Visibility.Visible;
                }
            }
        }

        private void Arkpedia_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(MainWindow.ArkpediaBrowser, $"https://ark.fandom.com/wiki/{CurrentDinoData.ArkpediaUrl}", MainWindow.ArkpediaTab);
        }

        private void Dododex_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(MainWindow.DododexBrowser, $"https://www.dododex.com/taming/{CurrentDinoData.DododexUrl}", MainWindow.DododexTab);
        }
    }
}
