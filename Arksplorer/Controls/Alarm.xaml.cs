using Arksplorer.Properties;
using Audinator;
using System;
using System.Collections.ObjectModel;
using System.Configuration.Internal;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Arksplorer.Controls
{
    //public class AlarmConfig
    //{
    //    public string Description { get; set; }
    //    public bool AutoRepeat { get; set; }
    //}

    /// <summary>
    /// Interaction logic for Alarm.xaml
    /// </summary>
    public partial class Alarm : UserControl
    {
        private bool AlarmEnabled { get; set; }
        private DateTime AlarmTimestamp { get; set; }
        private bool AlarmTriggered { get; set; }

        private string LastInitDuration { get; set; }

        public static ObservableCollection<ListInfoItem> AudioFiles { get; set; }

        public Alarm(int index, bool setAutoRepeat = false)
        {
            InitializeComponent();

            AudioType.SelectedIndex = index;
            AutoRepeat.IsChecked = setAutoRepeat;
        }

        public Alarm(string config)
        {
            InitializeComponent();

            if (config.EndsWith(",Repeat"))
            {
                AutoRepeat.IsChecked = true;
                config = config.Remove(config.Length - 7);
            }

            if (string.IsNullOrWhiteSpace(config))
                AudioType.SelectedIndex = 0;
            else
            {
                var item = (ComboBoxItem)AudioType.FindName(config);
                if (item == null)
                    AudioType.Text = config;
                else
                    AudioType.SelectedItem = item;
            }
        }

        public string GetConfig()
        {
            string value;

            if (AudioType.SelectedItem == null)
                value = AudioType.Text;
            else
                value = ((ListInfoItem)AudioType.SelectedItem).Description;

            string config = $"{value}{(AutoRepeat.IsChecked ?? false ? ",Repeat" : "")};";

            return config;
        }

        public static void InitAudioFiles(string folder)
        {
            AudioFiles = new();
            AudioFiles.Add(new() { Description = "None", Value = null });

            foreach (var filename in Directory.GetFiles("./Audio/", "*.wav"))
                AudioFiles.Add(new() { Description = Path.GetFileNameWithoutExtension(filename), Value = filename });
        }

        private void InitAlarm(string duration)
        {
            LastInitDuration = duration;
            InitAlarm();
        }

        private void InitAlarm()
        {
            DateTime now = DateTime.Now;
            string duration = LastInitDuration;

            bool lapsed = AlarmTimestamp < now;
            DateTime baseTime;

            if (duration.StartsWith("+") || duration.StartsWith("-"))
            {
                if (AlarmEnabled == false)
                    baseTime = now;
                else
                    baseTime = AlarmTimestamp;
            }
            else
                baseTime = now;

            AlarmTimestamp = baseTime.AddMinutes(double.Parse(duration)).AddMilliseconds(500); // We add 1/2 second to give us a nice start time. e.g. 10mins -> 10:00 start, not 9:59
            // Only reset timer if we have gone from a negative time to a positive
            if (lapsed && AlarmTimestamp > now)
                AlarmTriggered = false;
            else if (!lapsed && AlarmTimestamp < now)
                // We have manually gone negative, cancel any alarm
                AlarmTriggered = true;

            AlarmEnabled = true;

            NextTick(DateTime.Now);
        }

        private void SetAlarm(object sender, RoutedEventArgs e)
        {
            string duration = (string)((Button)sender).Tag;

            InitAlarm(duration);
        }

        public void NextTick(DateTime currentTime)
        {
            if (AlarmEnabled)
            {
                TimeSpan timeLeft = AlarmTimestamp - currentTime;

                SolidColorBrush brush;
                FontWeight weight;

                int secondsLeft = timeLeft.Seconds;
                int minutesLeft = timeLeft.Minutes;
                AlarmTimeLeft.Text = $"{(timeLeft.Ticks < 0 ? "-" : "")}{Math.Abs(timeLeft.Minutes):00}:{Math.Abs(secondsLeft):00}";

                if (minutesLeft > 0 || secondsLeft > 30)
                    brush = Brushes.ForestGreen;
                else if (secondsLeft > 5)
                    brush = Brushes.DarkOrange;
                else
                    brush = Brushes.Red;

                if (minutesLeft < 0 || secondsLeft < 0)
                {
                    weight = FontWeights.Bold;
                    if (!AlarmTriggered)
                    {
                        AlarmTriggered = true;
                        TriggerAlarm();

                        if (AutoRepeat.IsChecked ?? false)
                            InitAlarm();
                    }
                }
                else
                    weight = FontWeights.Normal;

                if (AlarmTimeLeft.Foreground != brush)
                    AlarmTimeLeft.Foreground = brush;

                if (AlarmTimeLeft.FontWeight != weight)
                    AlarmTimeLeft.FontWeight = weight;
            }
        }

        public void TriggerAlarm()
        {
            AlarmTriggered = true;
            Globals.MainWindow.TriggerAlarmVisualisation();

            string filename = (string)AudioType.SelectedValue;
            if (filename == null)
            {
                string text = AudioType.Text;
                if (text != "None" && !string.IsNullOrWhiteSpace(text))
                    TalkieToaster.Say(text);
            }
            else
            {
                Audio.PlaySample(filename);
            }
        }

        private void RemoveAlarm()
        {
            AlarmEnabled = false;
            //Player.Stop();
            AlarmTimeLeft.Foreground = Brushes.Black;
            AlarmTimeLeft.FontWeight = FontWeights.Normal;
            AlarmTimeLeft.Text = "Off";
        }

        private void AlarmOff_Click(object sender, RoutedEventArgs e)
        {
            RemoveAlarm();
        }

        public void SetUserDefinedAlarmControl(string value)
        {
            UserDefinedAlarm.Content = value;
            UserDefinedAlarm.Tag = value;
        }

        private void AlarmList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlarmList.SelectedItem == null)
                return;

            string duration = AlarmList.SelectedValue as string;
            SetUserDefinedAlarmControl(duration);
            Settings.Default.UserSpecificAlarmDuration = duration;
            InitAlarm(duration);

            AlarmList.SelectedItem = null;
        }

        private void DeleteAlarm_Click(object sender, RoutedEventArgs e)
        {
            Globals.MainWindow.DeleteAlarm(this);
        }
    }
}
