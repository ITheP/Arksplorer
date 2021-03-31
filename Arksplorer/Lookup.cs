using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
    }
}
