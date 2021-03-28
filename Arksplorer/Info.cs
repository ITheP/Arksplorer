using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Arksplorer
{
    /// <summary>
    /// General purpose info for showing basic description/value data plus any icons
    /// </summary>
    public class Info
    {
        public List<InfoEntry> Items { get; } = new();
        public List<BitmapImage> Icons { get; } = new();

        // Handy for extra processing
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string CreatureId { get; set; }
        public int Level { get; set; } = -1;
        public string Sex { get; set; }
        public bool Cryoed { get; set; }

        public void Add(string description, string value = null)
        {
            Items.Add(new InfoEntry() { Description = description, Value = value });
        }

        public void AddIcon(BitmapImage image)
        {
            Icons.Add(image);
        }
    }

    public class InfoEntry
    {
        public string Description { get; set; }
        public string Value { get; set; }
    }
}
