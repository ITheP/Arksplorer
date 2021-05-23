using System;
using System.Media;

namespace Arksplorer
{
    public static class Audio
    {
        //Problems with MediaPlayer not playing. Not sure why! To retry with a different mp3?
        //private MediaPlayer Player { get; set; } = new();
        private static SoundPlayer Player { get; set; } = new();
        private const string Extention = ".wav";

        public static void PlaySample(string filename)
        {
            //string filename = $"Audio/{type}{Extention}";
            try
            {
                Player.SoundLocation = filename;
                Player.Play();

            }
            catch (Exception ex)
            {
                string extraDescription = $"Something went wrong playing sample '{filename}'";
                Errors.ReportProblem(ex, extraDescription);
            }
        }
    }
}
