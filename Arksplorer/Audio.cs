using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace Arksplorer
{
    public static class Audio
    {
        //Problems with MediaPlayer not playing. Not sure why! To retry with a different mp3?
        //private MediaPlayer Player { get; set; } = new();
        private static SoundPlayer Player { get; set; } = new();
        private const string Extention = ".wav";

        public static void PlaySample(string type)
        {
            string filename = $"Audio/{type}{Extention}";
            try
            {
                Player.SoundLocation = filename;
                Player.Play();

            }
            catch (Exception ex)
            {
                Debug.Print($"Something went wrong playing sample '{filename}': {ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}");
            }
        }
    }
}
