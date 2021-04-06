using Microsoft.Web.WebView2.Core;
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
        private DinoData CurrentDinoData { get; set; }

        public MainWindow MainWindow { get; set; }
        private Info CurrentDino { get; set; }

        public InfoVisual()
        {
            InitializeComponent();
            MainWindow = (MainWindow)Application.Current.MainWindow;
        }

        public void ShowInfo(Info info, bool ShowLinks = false)
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

            Creature.Text = info.Creature;

            Lon.Text = $"{info.Lon:N2}";
            Lat.Text = $"{info.Lat:N2}";

            Level.Text = $"{info.Level}";
            if (info.BaseLevel > 0)
            {
                BaseLevel.Text = $"{info.BaseLevel}";
                BaseLevel.Visibility = Visibility.Visible;
                LevelArrow.Visibility = Visibility.Visible;
            }
            else
            {
                BaseLevel.Visibility = Visibility.Collapsed;
                LevelArrow.Visibility = Visibility.Collapsed;
            }

            // ToDo: Make this proper binding for live update on option changes. Remember to always have a bound list if this is the case
            if (MainWindow.DetailInPopUps)
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

            this.InvalidateVisual();
            this.InvalidateArrange();
            this.InvalidateMeasure();
        }

        private static void ShowColor(TextBlock control, Border border, ArkColor color)
        {
            if (color == null)
            {
                control.Text = string.Empty;
                control.Background = Brushes.White;
                border.BorderBrush = Brushes.LightGray;
                border.BorderThickness = ThinBorder;
            }
            else
            {
                control.Text = color.Name;
                control.Background = color.Color;
                border.BorderBrush = Brushes.Black;
                border.BorderThickness = ThickBorder;
            }
        }

        private static Thickness ThickBorder = new(1.0);
        private static Thickness ThinBorder = new(1.0);

        private void Arkpedia_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(MainWindow.ArkpediaWebTab, $"https://ark.fandom.com/wiki/{CurrentDinoData.ArkpediaUrl}");
        }

        private void Dododex_Click(object sender, RoutedEventArgs e)
        {
            var serverConfig = MainWindow.ServerConfig;
            MainWindow.Navigate(MainWindow.DododexWebTab, $"https://www.dododex.com/taming/{CurrentDinoData.DododexUrl}#level={CurrentDino.Level}&taming={serverConfig.TamingSpeedMultiplier}&consumption={serverConfig.FoodDrainMultiplier}");
        }
    }
}
