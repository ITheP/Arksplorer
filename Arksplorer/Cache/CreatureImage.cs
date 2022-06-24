using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Arksplorer.Caches
{

    public static class CreatureImages
    {
        private const string Folder = "./Cache/Images/Creatures/";
        private const string BaseUrl = "https://www.dododex.com/media/creature/";

        private static Dictionary<string, BitmapImage> Images { get; set; } = new();

        public static void EnsureFoldersExist()
        {
            if (!File.Exists("./Cache/"))
                Directory.CreateDirectory("./Cache/");

            if (!File.Exists("./Cache/Images/"))
                Directory.CreateDirectory("./Cache/Images/");

            if (!File.Exists("./Cache/"))
                Directory.CreateDirectory("./Cache/Images/Creatures/");
        }

        /// <summary>
        /// Load creature image from cache. Doesn't exist? Load from disk! Doesn't exist? Try and download from internet! Anything downloaded will then be cached for later use.
        /// </summary>
        /// <param name="creatureName"></param>
        /// <returns></returns>
        public static BitmapImage RequestImage(string creatureName)
        {
            // File/internet need to load async, and populate at a later point when loading is complete.
            // NOTE: Setting image source to a URL should auto-load the file from the internet

            // Right now we are only using this with Dododex, so is slightly specific to this at this time.

            // dododex wants lower case creature names + no spaces
            creatureName = creatureName.Replace(" ", "").ToLower();

            // < img src = "/media/creature/equus.png" itemprop = "image" alt = "Equus" id = "mainImage" >
            if (Images.ContainsKey(creatureName))
                return Images[creatureName];

            Uri uri;
            BitmapImage bitmap;
            string path = string.Empty;

            // See if we can snag the image from our local cache
            try
            {
                path = System.IO.Path.Combine(Folder, $"{creatureName}.png");
                Debug.Print(path);

                if (!File.Exists(path))
                {
                    // No file yet, try and snag it!
                    string url = $"{BaseUrl}{creatureName}.png";

                    if (!SnagImageFromUrl(url, path))
                    {
                        // We only want to load this once, so stick an empty value in the dictionary so there is something to look up
                        Images.Add(creatureName, null);

                        Debug.Print($"Tried to load '{url} and failed.");

                        return null;
                    }
                }

                // If file didn't exist before, it should now! Could download to and load from memory stream etc. but saving locally then loading will do for now
                uri = new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, path));
                bitmap = new BitmapImage(uri);

                Images.Add(creatureName, bitmap);

                return bitmap;
            }
            catch (Exception ex)
            {
                Errors.ReportProblem(ex, $"There was a problem trying to load the image '{path}'");
                return null;
            }
        }

        public static bool SnagImageFromUrl(string url, string destinationFile)
        {
            Uri uri = new Uri(url);

            try
            {
                using (WebClient client = new())
                {
                    client.Headers["User-Agent"] = "Arksplorer";
                    client.DownloadFile(url, destinationFile);
                }
            }
            catch (Exception ex)
            {
                if (!Globals.HaveDoneWarning($"Image.{uri.Host}"))
                {

                    Errors.ReportProblem(ex, $"There was a problem trying to download the image '{url}' to '{destinationFile}'. Further download warnings for this site will not be shown during this session.");
                }

                Debug.Print(ex.Message);
                if (ex.InnerException != null)
                    Debug.Print(ex.InnerException.Message);

                return false;
            }

            return true;
        }
    }
}
