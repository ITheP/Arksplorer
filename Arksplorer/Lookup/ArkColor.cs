﻿using System.Text.Json.Serialization;
using System.Windows.Media;

namespace Arksplorer
{
    public class ArkColor
    {
        public int Id { get; set; }
        public string Hex { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        [JsonIgnore]
        public Brush Color { get; set; }
        // Colour label should be in UI (contrast with color itself for visibility)
        public Brush LabelColor { get; set; }

        public void InitSuitableLabelColour()
        {
            Color color = ((SolidColorBrush)Color).Color;

            ColorHelper.RGBToHSL(color.R, color.G, color.B, out double h, out double s, out double l);
            if (Name == "Light Green")
                System.Diagnostics.Debug.Print("freese");

            if (l < 0.5)
                LabelColor = Brushes.White;
            else
                LabelColor = Brushes.Black;
        }
    }
}
