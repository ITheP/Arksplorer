using Arksplorer.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Arksplorer.Controls
{

    /// <summary>
    /// Interaction logic for Alarm.xaml
    /// </summary>
    public partial class Alarm : UserControl
    {
        private bool AlarmEnabled { get; set; }
        private DateTime AlarmTimestamp { get; set; }
        private bool AlarmTriggered { get; set; }

        private string LastInitDuration { get; set; }

        public Alarm(string audioType)
        {
            InitializeComponent();

            AudioType.Text = audioType;
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

            string audioType = (string)AudioType.SelectedValue;
            if (audioType != null)
                Audio.PlaySample(audioType);
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
    }
}
