using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Media;

namespace Arksplorer
{
    public static class Lookup
    {
        public static void LoadDataFromLookupFiles()
        {
            try
            {
                LoadDinoData("./Data/Lookup-Dinos.json");
                LoadColorData("./Data/Lookup-Colors.json");
            }
            catch
            {
                throw;
            }
        }

        public static void LoadDinoData(string filename)
        {
            Dinos = JsonSerializer.Deserialize<Dictionary<string, DinoData>>(File.ReadAllText(filename));
        }

        public static DinoData FindDino(string creatureId)
        {
            if (Dinos.ContainsKey(creatureId))
                return Dinos[creatureId];

            return null;
        }

        public static Dictionary<string, DinoData> Dinos { get; set; }

        public static void LoadColorData(string filename)
        {
            // Want a dictionary based on int - cant define json as an int key'ed hash set so
            // we read in raw data then generate a dictionary after

            List<ArkColor> colors = JsonSerializer.Deserialize<List<ArkColor>>(File.ReadAllText(filename));

            ArkColors = new();

            foreach (var arkColor in colors)
            {
                var color = (SolidColorBrush)(new BrushConverter().ConvertFrom(arkColor.Hex));
                color.Freeze();
                arkColor.Color = color;
                arkColor.InitSuitableLabelColour();
                ArkColors.Add(arkColor.Id, arkColor);
            }
        }

        public static ArkColor FindColor(int id)
        {
            if (ArkColors.ContainsKey(id))
                return ArkColors[id];

            return null;
        }

        public static Dictionary<int, ArkColor> ArkColors { get; set; }
    }
}
