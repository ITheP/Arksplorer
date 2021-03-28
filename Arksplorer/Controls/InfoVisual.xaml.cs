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
        public InfoVisual()
        {
            InitializeComponent();
        }

        public void ShowInfo(Info info)
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

            Lon.Text = $"{info.Lon:N2}";
            Lat.Text = $"{info.Lat:N2}";

            InfoList.ItemsSource = info.Items;
            IconList.ItemsSource = info.Icons;
        }
    }
}
