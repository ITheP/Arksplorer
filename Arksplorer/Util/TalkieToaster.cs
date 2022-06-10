using Arksplorer.Properties;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace Audinator
{
    public static class TalkieToaster
    {
        public const string ApplicationSpokenName = "Ordinator";

        private static SpeechSynthesizer Synth { get; set; }

        private static Settings UserSettings { get; set; }

        public static void Init(Settings userSettings)
        {
            UserSettings = userSettings;
        }

        public static void Say(string text)
        {
            if (!UserSettings.Speech_Enabled)
                return;

            CheckSetup();

            // ToDo: Reimpliment speech using .net core friendly variant
            Task.Run(() => { Synth.Speak(text); });
        }

        private static void CheckSetup()
        {
            if (Synth == null)
            {
                Synth = new SpeechSynthesizer();

                // ToDo: Needs to be the same audio device as the song plays to. Either that or we can just replace the whole thing with NAudio playing some samples. This was handy for testing.
                Synth.SetOutputToDefaultAudioDevice();

                Synth.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
            }
        }
    }
}