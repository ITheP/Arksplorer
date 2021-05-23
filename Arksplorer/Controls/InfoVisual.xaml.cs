using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Arksplorer.Controls
{
    /// <summary>
    /// Interaction logic for InfoVisual.xaml
    /// </summary>
    public partial class InfoVisual : UserControl
    {
        private DinoData CurrentDinoData { get; set; }

        private Info CurrentDino { get; set; }

        public InfoVisual()
        {
            InitializeComponent();
        }

        public void ShowInfo(Info info, bool ShowLinks, bool showDetails = false)
        {
            CurrentDino = info;

            if (info.Name == null)
            {
                CreatureName.Visibility = Visibility.Collapsed;
            }
            else if (string.IsNullOrWhiteSpace(info.Name))
            {
                CreatureName.Text = "Name not set";
                CreatureName.FontStyle = FontStyles.Italic;
                CreatureName.Visibility = Visibility.Visible;
            }
            else
            {
                CreatureName.Text = info.Name;
                CreatureName.FontStyle = FontStyles.Normal;
                CreatureName.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrWhiteSpace(info.Creature))
            {
                Creature.Visibility = Visibility.Collapsed;
            }
            else
            {
                Creature.Text = info.Creature;
                Creature.Visibility = Visibility.Visible;
            }

            Lon.Text = $"{info.Lon:N2}";
            Lat.Text = $"{info.Lat:N2}";

            Level.Text = $"{info.Level}";
            if (info.BaseLevel > 0)
            {
                BaseLevel.Text = $"{info.BaseLevel}";
                //BaseLevel.Visibility = Visibility.Visible;
                LevelArrow.Visibility = Visibility.Visible;
            }
            else
            {
                BaseLevel.Text = "Level ";
                //BaseLevel.Visibility = Visibility.Collapsed;
                LevelArrow.Visibility = Visibility.Collapsed;
            }

            // ToDo: Make this proper binding for live update on option changes. Remember to always have a bound list if this is the case
            if (showDetails)
            {
                InfoList.Visibility = Visibility.Visible;
                InfoList.ItemsSource = info.Items;
            }
            else
            {
                InfoList.Visibility = Visibility.Collapsed;
                InfoList.ItemsSource = null;
            }

            IconList.ItemsSource = info.Icons;

            Arkpedia.Visibility = Visibility.Collapsed;
            Dododex.Visibility = Visibility.Collapsed;

            if (ShowLinks && info.CreatureId != null)
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

            if (info.C0 == null && info.C1 == null && info.C2 == null && info.C3 == null && info.C4 == null && info.C5 == null)
                ColorList.Visibility = Visibility.Collapsed;
            else
            {
                ShowColor(C0, B0, info.C0);
                ShowColor(C1, B1, info.C1);
                ShowColor(C2, B2, info.C2);
                ShowColor(C3, B3, info.C3);
                ShowColor(C4, B4, info.C4);
                ShowColor(C5, B5, info.C5);

                ColorList.Visibility = Visibility.Visible;
            }
        }

        private static void ShowColor(TextBlock control, Border border, ArkColor color)
        {
            if (color == null)
            {
                control.Text = string.Empty;
                control.Background = Brushes.White;
                control.Tag = null;
                border.BorderBrush = Brushes.LightGray;
                border.BorderThickness = ThinBorder;
            }
            else
            {
                control.Text = color.Name;
                control.Background = color.Color;
                control.Tag = color;
                border.BorderBrush = Brushes.Black;
                border.BorderThickness = ThickBorder;
            }
        }

        private static Thickness ThickBorder = new(1.0);
        private static Thickness ThinBorder = new(1.0);

        private void Arkpedia_Click(object sender, RoutedEventArgs e)
        {
            string url = $"https://ark.fandom.com/wiki/{CurrentDinoData.ArkpediaUrl}";
            Globals.ArkpediaBrowser.Navigate(url, Globals.MainWindow.ArkpediaTab);
            Globals.ArkpediaBrowser.UpdateRotatingShortcuts(CurrentDino.Creature, $"Load info for {CurrentDino.Creature}", url);
        }

        private void Dododex_Click(object sender, RoutedEventArgs e)
        {
            var serverConfig = Globals.MainWindow.ServerConfig;
            string url = $"https://www.dododex.com/taming/{CurrentDinoData.DododexUrl}#level={CurrentDino.Level}&taming={serverConfig.TamingSpeedMultiplier}&consumption={serverConfig.FoodDrainMultiplier}";
            Globals.DododexBrowser.Navigate( url, Globals.MainWindow.DododexTab);
            Globals.DododexBrowser.UpdateRotatingShortcuts(CurrentDino.Creature, $"Load info for {CurrentDino.Creature}", url);
        }
    }
}
