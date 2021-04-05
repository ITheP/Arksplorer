using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Arksplorer
{
    public static class Icons
    {
        public static BitmapImage Male { get; } = LoadImage("Images/Male.png");
        public static BitmapImage Female { get; } = LoadImage("Images/Female.png");
        public static BitmapImage Cryopod { get; } = LoadImage("Images/Cryopod.png");

        private static BitmapImage LoadImage(string filename)
        {
            // Direct references to images as part of a new BitmapImage is failing (though bizzarly not if checking during a debug session)
            // Most reliable way is by loading through the following
            BitmapImage image = new();
            
            image.BeginInit();
            image.UriSource = new Uri(filename, UriKind.Relative);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();

            return image;
        }
    }
}
