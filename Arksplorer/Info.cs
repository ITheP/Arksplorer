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
        public List<ListInfoItem> Items { get; } = new();
        public List<BitmapImage> Icons { get; } = new();

        // Handy for extra processing
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string Creature { get; set; }
        public string CreatureId { get; set; }
        public int Level { get; set; } = -1;
        public string Sex { get; set; }
        public bool Cryoed { get; set; }
        public ArkColor C0 { get; set; }
        public ArkColor C1 { get; set; }
        public ArkColor C2 { get; set; }
        public ArkColor C3 { get; set; }
        public ArkColor C4 { get; set; }
        public ArkColor C5 { get; set; }

        public void Add(string description, string value = null)
        {
            Items.Add(new ListInfoItem() { Description = description, Value = value });
        }

        public void AddIcon(BitmapImage image)
        {
            Icons.Add(image);
        }
    }
}
