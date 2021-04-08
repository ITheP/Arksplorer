using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shell;

namespace Arksplorer
{

    public static class Lookup
    {
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

        public static void SetArkColorSortCode(IArkEntityWithCreature creature)
        {
            creature.C0_Sort = creature.C0?.Id ?? -1;
            creature.C1_Sort = creature.C1?.Id ?? -1;
            creature.C2_Sort = creature.C2?.Id ?? -1;
            creature.C3_Sort = creature.C3?.Id ?? -1;
            creature.C4_Sort = creature.C4?.Id ?? -1;
            creature.C5_Sort = creature.C5?.Id ?? -1;
        }
    }
}
