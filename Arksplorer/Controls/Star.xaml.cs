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

namespace Arksplorer.Controls
{
    /// <summary>
    /// Interaction logic for Star.xaml
    /// </summary>
    public partial class Star : UserControl
    {
        public double MaxSize { get; set; }
        public Duration SpinSpeed { get; set; }
        public Duration ZoomSpeed { get; set; }
        public double FromAngle { get; set; }
        public double ToAngle { get; set; }

        public Star()
        {
            Random rnd = new();
            MaxSize = (rnd.NextDouble() * 3.0d) + 1.0d;

            FromAngle = rnd.NextDouble() * 360.0;
            ToAngle = FromAngle + 360.0;
            if (rnd.NextDouble() < 0.5)
                ToAngle = -ToAngle;

            SpinSpeed = new Duration(TimeSpan.FromMilliseconds((rnd.NextDouble() * 3000.0d) + 1000.0d));
            // Zoom speed 0.5 to 2 zoom speed
            ZoomSpeed = new Duration(TimeSpan.FromMilliseconds((rnd.NextDouble() * 1500.0d) + 500.0d));

            InitializeComponent();
        }
    }
}
