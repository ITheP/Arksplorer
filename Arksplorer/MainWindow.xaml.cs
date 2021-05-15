using Arksplorer.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Arksplorer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool Initialisting { get; set; }

        private static Dictionary<string, BitmapImage> MapImages { get; } = new();
        private static string CurrentMapImage { get; set; } = "";
        private static UIElement LoadingSpinner { get; set; }
        public static ServerConfig ServerConfig { get; set; }

        private static List<Server> KnownServers { get; set; }

        private static DispatcherTimer Timer { get; set; }

        public static ObservableCollection<MapSelection> MapList { get; set; } = new();

        /// <summary>
        /// Forces use of local temp.json file as a data file. Used for debugging (rather than having to make a server trip for data we can't control)
        /// </summary>
        public bool ForceLocalLoad { get; set; } = false;

        /// <summary>
        /// Set this to true to force refresh of data reload, even if cache timings have not expired. Handy for debugging.
        /// </summary>
        public bool ForceRefreshOfData { get; set; } = false;

        private List<FrameworkElement> FilterColorButtons { get; set; }
        private List<FrameworkElement> LoadDataButtons { get; set; }

        private static Dictionary<string, DataPackage> DataPackages { get; set; } = new Dictionary<string, DataPackage>();

        public DataPackage CurrentDataPackage { get; set; }

        private Cursor PrevCursor { get; set; }

        private Settings UserSettings { get; set; }
        //private bool ProcessingQueue { get; set; }

        public MainWindow()
        {
            Initialisting = true;
            InitializeComponent();
            Initialisting = false;

            Init();
        }

        #region Init

        private void Init()
        {
            InitUILists();
            InitStoryboards();
            InitControlState();

            DataContext = this;

            Version.Text = Globals.Version;

            // We drag the loading effect out the page when not visible,
            // so it's not being calculated while hidden/collapsed (have seen overhead happen even when not visible if its linked into the page)
            LoadingSpinner = GeneralLoadingSpinner.Child;

            UserSettings = Settings.Default;

            ShowSameType.IsChecked = UserSettings.ShowSameType;
            ShowPopups.IsChecked = UserSettings.ShowPopups;
            IncludeDetailsInPopUps.IsChecked = UserSettings.IncludeDetailsInPopUps;
            Zoom.Value = UserSettings.Zoom;

            InitWebTabs();

            // Load data and grab config from server that feeds into all this
            try
            {
                Lookup.LoadDataFromLookupFiles();

                FilterColor.ItemsSource = Lookup.ArkColors;

                string lastServer = UserSettings.LastServer;

                List<Server> KnownServers = new();
                foreach (var file in Directory.GetFiles("./Servers/", "*.json"))
                    KnownServers.AddRange(JsonSerializer.Deserialize<List<Server>>(File.ReadAllText(file)));

                ServerList.ItemsSource = KnownServers.OrderBy(s => s.Name).ToList();

                // If we have a previous server, then setting the ServerList will trigger loading of the server on its SelectionChanged event
                ServerList.SelectedValue = lastServer;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was a problem during start up...{Environment.NewLine}{ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}{Environment.NewLine}Application will now exit.", "Start up error", MessageBoxButton.OK, MessageBoxImage.Error);
                ExitApplication();
            }

            // Timer is used for both triggering any Alarm and triggering checks to see if data needs to be refreshed
            Timer = new();
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Tick += TimerTrigger;
            Timer.Start();
        }

        /// <summary>
        /// Lists of related UI components - defined for e.g triggering highlighting of related controls easily
        /// </summary>
        private void InitUILists()
        {
            FilterColorButtons = new() { FilterC0, FilterC1, FilterC2, FilterC3, FilterC4, FilterC5, FilterCAll };
            LoadDataButtons = new() { LoadTameDinos, LoadWildDinos, LoadSurvivors };
        }

        private void InitControlState()
        {
            // These are visible at design time to aid design, but want to hide them when window first opens
            MapImage.Visibility = Visibility.Collapsed;
            Marker.Visibility = Visibility.Collapsed;
            CustomMarker.Visibility = Visibility.Collapsed;
            ServerLoadedControls.Visibility = Visibility.Collapsed;
            DataVisual.Visibility = Visibility.Collapsed;
            OverviewInfo.Visibility = Visibility.Collapsed;
            FilterMap.Visibility = Visibility.Collapsed;
            FilterTribe.Visibility = Visibility.Collapsed;
            FilterCreature.Visibility = Visibility.Collapsed;

            SetFilterLevelEnabled(false);
            SetFilterColorEnabled(false);

            // Disable all controls that are reliant on working server connection...
            LoadableControlsEnabled(false);

            // ...except we re-enable the one control that will let us specify a server location :)
            ServerList.IsEnabled = true;
        }

        #endregion Init

        #region Timer and Alarm

        private bool AlarmEnabled { get; set; }
        private DateTime AlarmTimestamp { get; set; }
        private bool AlarmTriggered { get; set; }

        private void TimerTrigger(object sender, EventArgs e)
        {
            TimerTrigger();
        }

        private void TimerTrigger()
        {
            DateTime now = DateTime.Now;
            int seconds = now.Second;

            if (AlarmEnabled)
            {
                TimeSpan timeLeft = AlarmTimestamp - now;

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
                        TriggerAlarm();
                }
                else
                    weight = FontWeights.Normal;

                if (AlarmTimeLeft.Foreground != brush)
                    AlarmTimeLeft.Foreground = brush;

                if (AlarmTimeLeft.FontWeight != weight)
                    AlarmTimeLeft.FontWeight = weight;
            }

            // Auto check for a cache refresh every 20 - too often and its a waste of time, too little and will annoy the user. This seems like a reasonable balance.
            if (seconds % 20 == 0)
                CheckAndRefreshCache("CacheRefresh");
        }

        private void SetAlarm(object sender, RoutedEventArgs e)
        {
            string duration = (string)((Button)sender).Tag;
            DateTime now = DateTime.Now;

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

            AlarmTimestamp = baseTime.AddMinutes(double.Parse(duration));
            // Only reset timer if we have gone from a negative time to a positive
            if (lapsed && AlarmTimestamp > now)
                AlarmTriggered = false;
            else if (!lapsed && AlarmTimestamp < now)
                // We have manually gone negative, cancel any alarm
                AlarmTriggered = true;

            AlarmEnabled = true;

            TimerTrigger();
        }

        private void TriggerAlarm()
        {
            AlarmTriggered = true;
            PlaySample("FeedMe");
            FlashAlarmStoryboard.Begin(this);
        }

        private void RemoveAlarm()
        {
            AlarmEnabled = false;
            Player.Stop();
            AlarmTimeLeft.Foreground = Brushes.Black;
            AlarmTimeLeft.FontWeight = FontWeights.Normal;
            AlarmTimeLeft.Text = "Off";
        }

        private void AlarmOff_Click(object sender, RoutedEventArgs e)
        {
            RemoveAlarm();
        }

        #endregion Timer and Alarm

        #region Web and WebTabs

        private void InitWebTabs()
        {
            // Could make browser controls publically accessible elsewhere using x:FieldModifier="public" in xml, but we are making them accessible via Global class instead
            Globals.ArkpediaBrowser = ArkpediaBrowser;
            ArkpediaBrowser.Init("Ark Wikipedia", "https://ark.gamepedia.com/ARK_Survival_Evolved_Wiki");
            ArkpediaBrowser.InitRotatingShortcuts(3);

            Globals.DododexBrowser = DododexBrowser;
            DododexBrowser.Init("Dododex", "https://www.dododex.com/");
            DododexBrowser.InitRotatingShortcuts(3);

            Globals.ArkbuddyBrowser = ArkbuddyBrowser;
            ArkbuddyBrowser.Init("Arkbuddy", "https://tristan.games/apps/arkbuddy/"); 

            Globals.YouTubeBrowser = YouTubeBrowser;
            YouTubeBrowser.Init("YouTube", "https://youtube.com/");

            ServerBrowser.Init("Server");
        }

        private void HandleLinkClick(object sender, RequestNavigateEventArgs e)
        {
            Web.OpenUrlInExternalBrowser(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        #endregion Web and WebTabs

        #region Settings

        public void SaveMapPreference(object sender, RoutedEventArgs e)
        {
            // Save currently flagged maps to settings
            string flaggedMaps = "";

            foreach (var selection in MapList)
            {
                if (selection.Load)
                    flaggedMaps += selection.Name;
            }

            if (UserSettings.LastMaps != flaggedMaps)
                UserSettings.LastMaps = flaggedMaps;
        }

        #endregion Settings

        #region Server
        /// <summary>
        /// Highest level contact of a server - kick off attempting to get ArksplorerData.json data file we use to configure all our data fetching
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool LoadServer(Server server)
        {
            if (string.IsNullOrWhiteSpace(server.Url))
                return false;

            // Grab config from server that feeds into all this
            // ToDo: Config required for where this comes from!
            try
            {
                LoadingVisualEnabled(true);
                Status.Text = "Contacting server...";
                // Initial set up can happen in the background - it will enable relevant bits of interface when it completes
                Task.Run(() => LoadServerConfig(server));
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was a problem loading server information from {server.Url}{Environment.NewLine}{ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}{Environment.NewLine}Application will now exit.", "Start up error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadableControlsEnabled(false);
                return false;
            }
        }

        private static bool IsServerConfigLoaded()
        {
            return (ServerConfig != null);
        }

        #endregion Server

        #region Cache

        // Different types of dino may be loaded with different maps. There may be no consistency across packages of data!

        private void CheckAndRefreshCache(string from)
        {
            // Not propper locking, but a basic check to make sure we arent re-triggering the setting up of a queue while its mid-process
            if (CheckingQueue)
                return;

            CheckingQueue = true;

            DateTime now = DateTime.Now;

            Debug.Print($"{now:HH:mm:ss} {from}");

            Queue queue = new();

            foreach (KeyValuePair<string, DataPackage> package in DataPackages)
            {
                foreach (KeyValuePair<string, MapPackage> map in package.Value.IndividualMaps)
                {
                    var mapPackage = map.Value;
                    bool expired = now > mapPackage.ApproxNextServerUpdateTimestamp;

                    if (expired || ForceRefreshOfData)
                    {
                        // We are reloading what we already have
                        // Load process will flush old data if it exists first
                        queue.AddNewItem(map.Key, package.Value.Metadata, ForceRefreshOfData, ServerConfig);
                    }

                    string difference = $"{(mapPackage.ApproxNextServerUpdateTimestamp - now):mm\\:ss}";

                    Debug.Print($"{package.Value.Metadata.Description}.{map.Key} {mapPackage.Timestamp} {(expired ? $"Data expired {difference} ago" : $"Already using latest available data, expires in {difference}")}");
                }
            }

            if (queue.Items.Count > 0)
            {
                Debug.Print($"Refreshing cache...");
                // We don't automatically refresh the visual display as someone may be part way through looking at data.
                // Next filter/search it will show the new data.
                LoadQueue(queue, false);
            }

            CheckingQueue = false;
        }

        #endregion Cache

        #region Audio

        //Problems with MediaPlayer not playing. Not sure why! To retry with a different mp3?
        //private MediaPlayer Player { get; set; } = new();
        SoundPlayer Player { get; set; } = new();

        private void PlaySample(string type)
        {
            try
            {
                Player.SoundLocation = $"Audio/{type}.wav";
                Player.Play();

            }
            catch (Exception ex)
            {
                Debug.Print($"Something went wrong playing sample {type}.mp3: {ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}");
            }
        }

        #endregion Audio

        #region Queue

        private bool CheckingQueue { get; set; }

        private void SetUpQueue(MetaData type)
        {
            if (!IsServerConfigLoaded())
                return;

            //ProcessingQueue = true;

            LoadableControlsEnabled(false);
            LoadingVisualEnabled(true);

            //List<QueueDataItem> queue = new();
            Queue queue = new();

            foreach (var selection in MapList)
            {
                if (selection.Load)
                    queue.AddNewItem(selection.Name, type, ForceRefreshOfData, ServerConfig);
            }

            LoadQueue(queue, true);
        }

        // When a queue is loading, no others should load as controls that could trigger another load are disabled.
        private async void LoadQueue(Queue queue, bool autoUpdateDataGrid)
        {
            // PrevCursor = Mouse.OverrideCursor;
            //  Mouse.OverrideCursor = Cursors.Wait;

            int mapsLoaded = await Task.Run(() => queue.Process(autoUpdateDataGrid, DataPackages, this, ServerConfig, ForceLocalLoad));
        }

        private void WildDinos_Click(object sender, RoutedEventArgs e)
        {
            SetUpQueue(Types.WildMetadata);
        }

        private void TameDinos_Click(object sender, RoutedEventArgs e)
        {
            SetUpQueue(Types.TameMetadata);
        }

        private void Survivors_Click(object sender, RoutedEventArgs e)
        {
            SetUpQueue(Types.SurvivorMetadata);
        }

        #endregion Queue

        #region Filter

        private void ApplyFilterCriteria(bool exactOnly = false)
        {
            string criteria = FilterCriteria.Text;

            FilterDataVisual(CurrentDataPackage, criteria, exactOnly);
        }

        private void FilterDataVisual(DataPackage dataPackage, string criteria, bool exactOnly)
        {
            string levelFilter = FilterLevelType.SelectedValue?.ToString();

            if (dataPackage == null)
                return;

            if (string.IsNullOrWhiteSpace(criteria) && levelFilter == "All")
            {
                ClearFilter();
                return;
            }

            PrevCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            string filter = exactOnly ? dataPackage.Metadata.NormalSearch : dataPackage.Metadata.WildcardSearch;

            if (string.IsNullOrWhiteSpace(filter))
                return;


            // If we have multiple components e.g. `rag, twog" then we do 2 searches!"
            string[] parts = criteria.Split(",");
            string finalFilter;

            SetFlashMessage("Searching...");

            finalFilter = "";
            string separator = "";

            foreach (string p in parts)
            {
                string trimmed = p.Trim();

                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    if (trimmed.Contains("="))
                    {
                        if (dataPackage.Metadata.IncludesColors)
                        {
                            // Check for special case colour filters
                            // C0=, C1=, C2=, C3=, C4=, C5=, CAll=

                            // Find requested color id -> get ArkColor -> get sort index -> search for colours +/- how close we want a match

                            string[] subParts = trimmed.Split("=");
                            if (subParts.Length == 2)
                            {
                                string instruction = subParts[0];
                                string colorId = subParts[1];

                                if (int.TryParse(colorId, out int colorIndex))
                                {
                                    ArkColor color = Lookup.FindColor(colorIndex);
                                    if (color != null)
                                    {
                                        int sortIndex = color.SortOrder;
                                        int closeness = int.Parse((string)FilterColorCloseness.SelectedValue);

                                        string colorFilter = string.Empty;


                                        if (instruction == "CAll")
                                        {
                                            if (closeness == 0)
                                            {
                                                // Exact matches only
                                                colorFilter = $"(" +
                                                    $"(C0_Sort = {sortIndex}) OR " +
                                                    $"(C1_Sort = {sortIndex}) OR " +
                                                    $"(C2_Sort = {sortIndex}) OR " +
                                                    $"(C3_Sort = {sortIndex}) OR " +
                                                    $"(C4_Sort = {sortIndex}) OR " +
                                                    $"(C5_Sort = {sortIndex})" +
                                                    ")";
                                            }
                                            else
                                            {
                                                colorFilter = $"(" +
                                                    $"(C0_Sort >= {sortIndex - closeness} AND C0_Sort <= {sortIndex + closeness}) OR " +
                                                    $"(C1_Sort >= {sortIndex - closeness} AND C1_Sort <= {sortIndex + closeness}) OR " +
                                                    $"(C2_Sort >= {sortIndex - closeness} AND C2_Sort <= {sortIndex + closeness}) OR " +
                                                    $"(C3_Sort >= {sortIndex - closeness} AND C3_Sort <= {sortIndex + closeness}) OR " +
                                                    $"(C4_Sort >= {sortIndex - closeness} AND C4_Sort <= {sortIndex + closeness}) OR " +
                                                    $"(C5_Sort >= {sortIndex - closeness} AND C5_Sort <= {sortIndex + closeness})" +
                                                    ")";
                                            }
                                        }
                                        else
                                        {
                                            if (closeness == 0)
                                            {
                                                // Exact matches only
                                                switch (instruction)
                                                {
                                                    case "C0":
                                                    case "C1":
                                                    case "C2":
                                                    case "C3":
                                                    case "C4":
                                                    case "C5":
                                                        colorFilter = $"({instruction}_Sort = {sortIndex})";
                                                        break;
                                                    default:
                                                        MessageBox.Show($"Unknown colour component '{instruction}' to search in", "Unknown colour component", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                                        break;
                                                }

                                            }
                                            else
                                            {
                                                // Exact matches only
                                                switch (instruction)
                                                {
                                                    case "C0":
                                                    case "C1":
                                                    case "C2":
                                                    case "C3":
                                                    case "C4":
                                                    case "C5":
                                                        colorFilter = $"({instruction}_Sort >= {sortIndex - closeness} AND {instruction}_Sort <= {sortIndex + closeness})";
                                                        break;
                                                    default:
                                                        MessageBox.Show($"Unknown colour component '{instruction}' to search in", "Unknown colour component", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                                        break;
                                                }
                                            }
                                        }

                                        if (!string.IsNullOrWhiteSpace(colorFilter))
                                        {
                                            finalFilter += $"{separator}({colorFilter})";
                                            separator = " AND ";
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show($"Unknown Ark colour Id '{colorId}' to search for", "Unknown colour Id", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                    }
                                }
                            }
                        }
                        // Ignore if entity doesn't include colours
                    }
                    else
                    {

                        finalFilter += $"{separator}({filter.Replace("#", trimmed)})";
                        separator = " AND ";
                    }
                }
            }

            if (dataPackage.Metadata.IncludesLevel)
            {
                string extra = $"{(string.IsNullOrEmpty(finalFilter) ? "" : $" AND ({finalFilter})") }";

                if (levelFilter == "Above")
                    finalFilter = $"(Lvl <= {FilterLevelNumber.Text}){extra}";
                else if (levelFilter == "Below")
                    finalFilter = $"(Lvl >= {FilterLevelNumber.Text}){extra}";
            }

            Debug.Print($"Filter: {finalFilter}");

            try
            {
                DataRow[] filteredRows = CurrentDataPackage.Data.Select(finalFilter);

                if (filteredRows.Length == 0)
                    SetDataVisualData(null);
                else
                    SetDataVisualData(filteredRows.CopyToDataTable());
            }
            catch (Exception ex)
            {
                Debug.Print($"Error: {ex.Message}");
            }

            Mouse.OverrideCursor = PrevCursor;
        }

        private void ClearFilter()
        {
            if (CurrentDataPackage?.Data != null)
            {
                DataVisual.DataContext = CurrentDataPackage.Data;
                HideFlashMessage();
            }
        }

        private void SetFilterLevelEnabled(bool enabled)
        {
            FilterLevelType.IsEnabled = enabled;
            FilterLevelNumber.IsEnabled = enabled;
        }

        private void SetFilterColorEnabled(bool enabled)
        {
            FilterColor.IsEnabled = enabled;
            FilterColorCloseness.IsEnabled = enabled;
            FilterC0.IsEnabled = enabled;
            FilterC1.IsEnabled = enabled;
            FilterC2.IsEnabled = enabled;
            FilterC3.IsEnabled = enabled;
            FilterC4.IsEnabled = enabled;
            FilterC5.IsEnabled = enabled;
            FilterCAll.IsEnabled = enabled;
        }

        public void ApplyPotentialFilterArkColor()
        {
            if (PotentialFilterArkColor != null)
            {
                // Set the filter colour selector to the current ark color (though we don't auto trigger the filter)
                FilterColor.SelectedValue = PotentialFilterArkColor;
                PotentialFilterArkColor = null;
            }
        }

        private void FilterColor_Click(object sender, RoutedEventArgs e)
        {
            if (FilterColor.SelectedItem == null)
            {
                if (FilterColor.SelectedItem == null)
                    FlashMissingControl(FilterColor);

                return;
            }

            ArkColor color = ((KeyValuePair<int, ArkColor>)FilterColor.SelectedItem).Value;

            if (color == null)
                return;

            Button button = (Button)sender;

            FilterCriteria.Text = $"{button.Tag as string}={color.Id}";
            ApplyFilterCriteria();
        }

        private void FilterColorCloseness_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Initialisting)
                return;

            FlashFilterColorButtons();
        }

        private void ExactFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilterCriteria(true);
        }

        private void FilterCriteria_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter || e.Key == Key.Tab)
            {
                FlashTriggeredControl(ApplyFilter);
                ApplyFilterCriteria();
            }
        }
        private void FilterLevelNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsInteger(e.Text);
        }

        private void FilterLevelNumber_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (IsInteger(text))
                    return;
            }

            e.CancelCommand();
        }

        private void FilterLevelType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Initialisting)
                return;

            ApplyFilterCriteria();
        }

        private void ButtonFilter_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FilterCriteria.Text = button.Tag as string;
            ApplyFilterCriteria(true);
        }

        private void FilterColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Initialisting)
                return;

            FlashFilterColorButtons();
        }

        private void FlashFilterColorButtons()
        {
            FlashControl(FilterColorButtons);
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilterCriteria(false);
        }

        private void RemoveFilter_Click(object sender, RoutedEventArgs e)
        {
            ClearFilter();
        }

        #endregion Filter

        #region Control Highlighting

        private void InitStoryboards()
        {
            FlashControlStoryboard = (Storyboard)this.FindResource("FlashControl");
            FlashTriggeredControlStoryboard = (Storyboard)this.FindResource("FlashTriggeredControl");
            FlashMissingControlStoryboard = (Storyboard)this.FindResource("FlashMissingControl");
            FlashAlarmStoryboard = (Storyboard)this.FindResource("FlashAlarm");
            AnimatedMarkerStoryboard = (Storyboard)this.FindResource("AnimatedMarker");

            AnimateMarkers();
        }

        private void AnimateMarkers()
        {
            AnimatedMarkerStoryboard.Begin(Marker);
            AnimatedMarkerStoryboard.Begin(CustomMarker);
        }

        Storyboard FlashControlStoryboard { get; set; }
        Storyboard FlashTriggeredControlStoryboard { get; set; }
        Storyboard FlashMissingControlStoryboard { get; set; }
        Storyboard FlashAlarmStoryboard { get; set; }
        Storyboard AnimatedMarkerStoryboard { get; set; }

        private void FlashControl(FrameworkElement element)
        {
            FlashControlStoryboard.Begin(element);
        }

        private void FlashControl(List<FrameworkElement> elements)
        {
            foreach (var element in elements)
                FlashControlStoryboard.Begin(element);
        }

        private void FlashMissingControl(FrameworkElement element)
        {
            FlashMissingControlStoryboard.Begin(element);
        }

        private void FlashTriggeredControl(FrameworkElement element)
        {
            FlashTriggeredControlStoryboard.Begin(element);
        }

        private void FlashLoadDataButtons()
        {
            FlashControl(LoadDataButtons);
        }

        private double MarkerZoomCenterX { get; set; }
        private double MarkerZoomCenterY { get; set; }

        private void MapImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HandleMapImageClick(e.GetPosition(MapImage));
        }

        private void HandleMapImageClick(Point pos)
        {
            double x = pos.X - 50.0d;
            double y = pos.Y - 50.0d;

            CustomMarkerOffset.X = x;
            CustomMarkerOffset.Y = y;

            MapHolderZoom.CenterX = x + 50.0d;
            MapHolderZoom.CenterY = y + 50.0d;

            if (CustomMarker.Visibility != Visibility.Visible)
            {
                CustomMarker.Visibility = Visibility.Visible;
                HideCustomMarker.Fill = Brushes.DarkOrange;
            }
        }

        private void DisableCustomMarker()
        {
            if (CustomMarker.Visibility != Visibility.Collapsed)
                CustomMarker.Visibility = Visibility.Collapsed;

            // Reset any zoom position
            MapHolderZoom.CenterX = MarkerZoomCenterX;
            MapHolderZoom.CenterY = MarkerZoomCenterY;

            HideCustomMarker.Fill = Brushes.LightGray;
        }

        #endregion Control Highlighting

        #region DataVisual

        private string LastCreatureId { get; set; } = string.Empty;

        public void ShowData(string type, bool updateVisualDataGrid)
        {
            if (DataPackages.ContainsKey(type))
            {
                Debug.Print($"ShowData.CurrentDataPackage = DataPackages[{type}]");

                CurrentDataPackage = DataPackages[type];
                CurrentDataPackage.MakeSureDataIsUpToDate();

                if (updateVisualDataGrid)
                {
                    SetDataVisualData(CurrentDataPackage.Data);
                    DataVisual.Visibility = Visibility.Visible;
                }

                ExtraInfoTitle.Text = CurrentDataPackage.MapsDescription;
                ExtraInfo.Text = $"Total loaded: {CurrentDataPackage.Data?.Rows.Count ?? 0}";
                ExtraInfoMapData.ItemsSource = CurrentDataPackage.IndividualMaps.ToList();  // Don't belive it should require a Tolist() but have seen the display not update without it
                //ExtraInfoMapData.Items.Refresh();
                ShowExtraInfo(ExtraInfoHolder, ExtraInfoMapDataHolder);
                About.Content = "About";    // Just incase this button gets out of sync.

                ExtraInfoMapDataHolder.Visibility = Visibility.Visible;

                if (CurrentDataPackage.Data == null)
                {
                    Status.Text = $"No data found!";

                    SetFilterLevelEnabled(false);
                    SetFilterColorEnabled(false);
                }
                else
                {
                    if (CurrentDataPackage.Data.Rows.Count == 0)
                    {
                        Status.Text = $"No data loaded yet";

                        SetFilterLevelEnabled(false);
                        SetFilterColorEnabled(false);
                    }
                    else
                    {
                        MetaData metadata = CurrentDataPackage.Metadata;

                        Status.Text = $"Loaded {CurrentDataPackage.Data.Rows.Count} {metadata.Description}s!";

                        SetFilterLevelEnabled(metadata.IncludesLevel);
                        SetFilterColorEnabled(metadata.IncludesColors);
                    }
                }
            }
            else
            {
                // ToDo: Error
            }
        }

        private void SetDataVisualData(DataTable data)
        {
            if (data == null || data.Rows.Count == 0)
            {
                DataVisual.DataContext = null;
                DataVisualCount.Text = "No entries found";
                SetFlashMessage("No entries found");

                return;
            }

            DataVisual.DataContext = data;
            Info.InitColumnIndexPositions(data);
            DataVisualCount.Text = $"{data.Rows.Count} entries";
            HideFlashMessage();
        }

        private void DataVisual_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridTextColumn column)
            {
                if (e.PropertyType == typeof(Single))
                    column.Binding = new Binding(e.PropertyName) { StringFormat = "N2" };
                else if (e.PropertyType == typeof(DateTime))
                    column.Binding = new Binding(e.PropertyName) { StringFormat = "dd/MM/yyyy hh:mm:ss" };
                else if (e.PropertyType == typeof(ArkColor))
                {
                    DataGridTemplateColumn template = new();
                    template.Header = column.Header;

                    DataTemplate dataTemplate = new();
                    dataTemplate.DataType = typeof(Rectangle);

                    FrameworkElementFactory rectangle = new(typeof(Rectangle));
                    rectangle.SetValue(Rectangle.WidthProperty, 32.0d);
                    rectangle.SetValue(Rectangle.HeightProperty, 16.0d);

                    Binding fillBinding = new($"{e.PropertyName}");
                    fillBinding.Converter = new ArkColorDBNullConverter();
                    rectangle.SetBinding(Rectangle.FillProperty, fillBinding);

                    // Set ArkColor against Tag for future use (e.g. mouse over -> get colour -> set color in filter)
                    Binding tagBinding = new($"{e.PropertyName}");
                    rectangle.SetValue(Rectangle.TagProperty, tagBinding);

                    rectangle.SetValue(Rectangle.ToolTipProperty, "Right click to set Filter Color");

                    dataTemplate.VisualTree = rectangle;

                    template.CellTemplate = dataTemplate;
                    template.CanUserResize = false;
                    template.CanUserSort = true;
                    template.SortMemberPath = $"{e.PropertyName}";
                    e.Column = template;
                }
                else if (e.PropertyType == typeof(BitmapImage))
                {
                    DataGridTemplateColumn template = new() { Header = column.Header };

                    DataTemplate dataTemplate = new();
                    dataTemplate.DataType = typeof(BitmapImage);

                    FrameworkElementFactory factory = new(typeof(Image));
                    factory.SetValue(Image.WidthProperty, 16.0d);
                    factory.SetValue(Image.HeightProperty, 16.0d);
                    Binding binding = new(e.PropertyName);
                    factory.SetBinding(Image.SourceProperty, binding);
                    dataTemplate.VisualTree = factory;

                    template.CellTemplate = dataTemplate;
                    template.CanUserResize = false;
                    template.CanUserSort = true;
                    template.SortMemberPath = e.PropertyName;
                    e.Column = template;
                }
                // Don't want to see the ccc column or any hidden ..._sort columns
                else if (e.PropertyName == "Ccc" || e.PropertyName.EndsWith("_Sort"))
                {
                    e.Column.Visibility = Visibility.Collapsed;
                }
                else if (column.DisplayIndex == 0)   // Hide GlobalIndex
                    column.Visibility = Visibility.Collapsed;
            }

        }

        private void DataVisual_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Initialisting)
                return;

            DataRowView entity = (DataRowView)(DataVisual.SelectedItem);

            if (entity == null)
            {
                SetSelectedInfo(null);
                return;
            }

            Info info = new(entity.Row);

            // Marker offset
            // translate 0->100 to -50->50
            double lat = info.Lat;
            double lon = info.Lon;

            if (lat > -1 && lon > -1)
            {
                double yPos = lat - 50.0f;
                double xPos = lon - 50.0f;

                MarkerOffset.X = xPos;
                MarkerOffset.Y = yPos;

                MarkerZoomCenterX = xPos + 50.0d;
                MarkerZoomCenterY = yPos + 50.0d;

                MapHolderZoom.CenterX = MarkerZoomCenterX;
                MapHolderZoom.CenterY = MarkerZoomCenterY;

                if (Marker.Visibility != Visibility.Visible)
                    Marker.Visibility = Visibility.Visible;

                // We move the OverviewMessage to a different point over the map if there is a chance it might cover up the Marker
                if (xPos > 0)
                    OverviewInfo.HorizontalAlignment = HorizontalAlignment.Left;
                else
                    OverviewInfo.HorizontalAlignment = HorizontalAlignment.Right;

                //if (yPos > 0)
                //    OverviewMessage.VerticalAlignment = VerticalAlignment.Top;
                //else
                //    OverviewMessage.VerticalAlignment = VerticalAlignment.Bottom;
            }

            if (SplashImage.Visibility == Visibility.Visible)
            {
                SplashImage.Visibility = Visibility.Collapsed;
                SplashImage.Source = null;
            }

            string mapName = (string)entity[Globals.MapColumn];
            if (mapName != CurrentMapImage)
            {
                MapImage.Source = LoadMapImage(mapName);
                MapImage.Visibility = Visibility.Visible;
            }

            if (info.CreatureId != LastCreatureId || mapName != LastSelected_Map)
            {
                // Visualisation of things, only do when the selected type has changed
                if (ShowSameType.IsChecked == true)
                {
                    LastSelected_Map = mapName;
                    LastSelected_CreatureId = info.CreatureId;

                    ShowMassMarkers(info.CreatureId, mapName);
                }
            }

            SetSelectedInfo(info);

            if (string.IsNullOrWhiteSpace(info.CreatureId))
            {
                FilterCreature.Visibility = Visibility.Collapsed;
            }
            else
            {
                FilterCreature.Content = info.Creature;
                FilterCreature.Tag = info.CreatureId;
                FilterCreature.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrWhiteSpace(info.Tribe))
            {
                FilterTribe.Visibility = Visibility.Collapsed;
            }
            else
            {
                FilterTribe.Content = info.Tribe;
                FilterTribe.Tag = info.Tribe;
                FilterTribe.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrWhiteSpace(mapName))
            {
                FilterMap.Visibility = Visibility.Collapsed;
            }
            else
            {
                FilterMap.Content = mapName;
                FilterMap.Tag = mapName;
                FilterMap.Visibility = Visibility.Visible;
            }

        }

        private void DataVisual_Sorting(object sender, DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;
            DataTable data = (DataTable)DataVisual.DataContext;
            var columnType = data.Columns[column.DisplayIndex].DataType;
            if (columnType == typeof(ArkColor))
            {
                // Sorting on our own custom colours is an absolute pain in the rear
                // Also - colours - how do you sort on a colour? Namne? Id? RGB value?
                // So we have custom sort column for each for just this purpose - how a sort code is stuck in here, well, we can be flexible!
                // We intercept the sorting so we can specify the sort column here instead
                ICollectionView dataView = CollectionViewSource.GetDefaultView(DataVisual.ItemsSource);
                dataView.SortDescriptions.Clear();
                dataView.SortDescriptions.Add(new SortDescription($"{column.Header}_Sort", column.SortDirection ?? ListSortDirection.Ascending));

                e.Handled = true;
            }
            else
                return;
        }

        #endregion DataVisual

        #region Server

        private async void LoadServerConfig(Server server)
        {
            string error = "";

            try
            {
                RawServerData rawServerData = await Web.HttpClient.GetFromJsonAsync<RawServerData>(new Uri(server.Url));
                ServerConfig = new ServerConfig(rawServerData);

                // Save this as the last selected server - that we know has worked!
                UserSettings.LastServer = server.Name;

                // If we were successfull, we also want to clear down any displayed data, caches, etc. which could be from an older server
                CurrentDataPackage = null;

                Debug.Print($"LoadServerConfig.CurrentDataPackage = null");

                DataPackages.Clear();

                Dispatcher.Invoke(() =>
                {
                    MapList.Clear();
                    string lastMaps = UserSettings.LastMaps;
                    foreach (var mapName in ServerConfig.Maps)
                        MapList.Add(new() { Name = mapName, CacheState = "Not loaded", Load = lastMaps.Contains(mapName) });

                    DataVisual.DataContext = null; // Clear any current results list
                    Status.Text = $"Welcome! This server updates data every {ServerConfig.RefreshRate}ish minutes.";
                    LoadableControlsEnabled(true);
                    LoadingVisualEnabled(false);
                    ServerLoadedControls.Visibility = Visibility.Visible;

                    ServerBrowser.SetBrowserSource(ServerConfig.Website);
                    ServerBrowser.SetHomeUrl(ServerConfig.Website);

                    ServerInfoList.ItemsSource = ServerConfig.GetServerOverview().Items;

                    GeneralLoadingSpinner.Child = null;

                    FlashLoadDataButtons();
                });

                return;
            }
            catch (HttpRequestException ex)
            {
                error = $"Error: {ex.StatusCode}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}";
            }
            catch (NotSupportedException ex)
            {
                error = $"Invalid content type: {ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}";
            }
            catch (JsonException ex)
            {
                error = $"Invalid JSON: {ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}";
            }
            catch (Exception ex)
            {
                error = $"Problem loading data: {ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}";
            }

            MessageBox.Show($"There was a problem during start up...{Environment.NewLine}{error}){Environment.NewLine}Application will now exit.", "Start up error", MessageBoxButton.OK, MessageBoxImage.Error);
            ExitApplication();
        }

        private void ServerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Initialisting)
                return;

            Server server = (Server)(ServerList.SelectedItem);
            LoadServer(server);
        }

        #endregion Server

        #region Maps

        private string LastSelected_Map { get; set; }
        private string LastSelected_CreatureId { get; set; }

        public static BitmapImage LoadMapImage(string mapName)
        {
            if (MapImages.ContainsKey(mapName))
                return MapImages[mapName];

            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "Images", $"{mapName}.png");
            if (!File.Exists(path))
            {
                path = System.IO.Path.Combine(Environment.CurrentDirectory, "Images", $"{mapName}.jpg");
                if (!File.Exists(path))
                    return null; // backup image
            }

            var uri = new Uri(path);
            var bitmap = new BitmapImage(uri);

            MapImages.Add(mapName, bitmap);

            return bitmap;
        }

        private void MapsToIncludeCheckbox_Click(object sender, RoutedEventArgs e)
        {
            string selectedMaps = "";
            string separator = "";
            foreach (var map in MapList)
            {
                if (map.Load)
                {
                    selectedMaps += separator + map.Name;
                    separator = ",";
                }
            }

            UserSettings.LastMaps = selectedMaps;
        }

        private void IncludeAll_Click(object sender, RoutedEventArgs e)
        {
            SetIncludeList(true);
        }

        private void IncludeNone_Click(object sender, RoutedEventArgs e)
        {
            SetIncludeList(false);
        }

        private void SetIncludeList(bool isChecked)
        {
            foreach (var selected in MapList)
                selected.Load = isChecked;

            MapsToInclude.Items.Refresh();
        }
        #endregion Maps

        #region Info and PopUps

        private void ShowExtraInfo(UIElement whatToShow, UIElement whatToShow2 = null)
        {
            AboutExtraInfo.Visibility = Visibility.Collapsed;
            ExtraInfoHolder.Visibility = Visibility.Collapsed;
            ExtraInfoMapDataHolder.Visibility = Visibility.Collapsed;

            if (whatToShow != null)
                whatToShow.Visibility = Visibility.Visible;

            if (whatToShow2 != null)
                whatToShow2.Visibility = Visibility.Visible;
        }

        private void SetSelectedInfo(Info info)
        {
            if (info == null)
                return;

            OverviewInfo.ShowInfo(info, true, UserSettings.IncludeDetailsInPopUps);

            OverviewInfo.Visibility = Visibility.Visible;
        }

        private DataGridRow LastDataGridRow { get; set; }
        private Info CurrentRectanglePopUpInfo { get; set; }
        private FrameworkElement LastControl { get; set; }
        private ArkColor PotentialFilterArkColor { get; set; }

        // Note that we COULD attach mouse enter/leave events to all rectangles we have created (mass markers) along with
        // other entities, but then we have to keep track of all the events and remove on rectangle disposal as well (else
        // we can end up with references hanging around). Nightmare. So we don't bother and just check if we are
        // over a rectangle or not with a tag of type Info.
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            bool showPopUpInfo = false;
            PotentialFilterArkColor = null;
            CurrentRectanglePopUpInfo = null;

            // MouseOver on the DataGrid results list
            System.Windows.Point pos = e.GetPosition(Root);
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(Root, pos);
            var hitElement = hitTestResult?.VisualHit;
            if (hitElement != null)
            {
                DependencyObject element = hitElement;
                while (element != null && !(element is DataGridCell || (element is FrameworkElement ? ((FrameworkElement)element).Tag != null : false)))
                    element = VisualTreeHelper.GetParent(element);

                if (element != null)
                {
                    bool showPopUps = ShowPopups.IsChecked ?? false;

                    if (element is DataGridCell)
                    {
                        if (showPopUps)
                        {
                            DataGridCell cell = (DataGridCell)element;
                            DataGridRow gridRow = DataGridRow.GetRowContainingElement(cell);
                            if (gridRow != LastDataGridRow)
                            {
                                LastDataGridRow = gridRow;

                                DataRowView dataRowView = (System.Data.DataRowView)gridRow.Item;
                                PopUpInfoVisual.ShowInfo(new Info(dataRowView.Row), false, UserSettings.IncludeDetailsInPopUps);
                            }

                            // Even if we don't change how this looks, we want to make sure its position is updated
                            showPopUpInfo = true;
                        }
                        else
                        {
                            showPopUpInfo = false;
                        }
                    }
                    else
                    {
                        FrameworkElement control = (FrameworkElement)element;

                        // Only interested in rectangles that have an Info in their tag - otherwise we assume its some random other rectangle
                        if (control.Tag is Info info)
                        {
                            if (showPopUps)
                            {

                                if (control != LastControl)
                                {
                                    LastControl = control;

                                    PopUpInfoVisual.ShowInfo(info, false, UserSettings.IncludeDetailsInPopUps);
                                }
                                showPopUpInfo = true;
                            }
                            else
                            {
                                showPopUpInfo = false;
                            }

                            // Make sure we remember what we are looking at, then if we mouse_up over a rectangle, this will exist and we know what to show in the main static pop up
                            CurrentRectanglePopUpInfo = info;
                        }
                        else if (control.Tag is ArkColor arkColor)
                        {
                            // We are over a colour!
                            if (PotentialFilterArkColor != arkColor)
                                PotentialFilterArkColor = arkColor;
                        }
                    }


                }
            }

            PopUpInfo.IsOpen = showPopUpInfo;

            PopUpInfo.HorizontalOffset = pos.X + 25;
            PopUpInfo.VerticalOffset = pos.Y + 25;

            if (FlashMessage.Visibility != Visibility.Visible)
                return;

            pos = e.GetPosition(this);

            // Only update if we have some change that's taken place
            if (Last_PosX != pos.X || Last_PosY != pos.Y)
            {
                Last_PosX = pos.X;
                Last_PosY = pos.Y;

                var width = Width;
                var height = Height;

                var percX = (pos.X / width);
                var percY = (pos.Y / height);

                FlashMessageC1.Color = ColorHelper.HSLToColor(percX - (60.0 / 360.0), 1.0, 1.0 - percY);
                FlashMessageC2.Color = ColorHelper.HSLToColor(percX, 1.0, 1.0 - percY);
                FlashMessageC3.Color = ColorHelper.HSLToColor(percX - (240.0 / 360.0), 1.0, 1.0 - percY);
                FlashMessageC4.Color = ColorHelper.HSLToColor(percX - (120.0 / 360.0), 1.0, 1.0 - percY);
            }

        }

        private double Last_PosX { get; set; }
        private double Last_PosY { get; set; }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Debug.Print($"{CurrentRectanglePopUpInfo?.Name ?? "Null"}");

            if (CurrentRectanglePopUpInfo != null)
                try
                {
                    DataTable data = (DataTable)DataVisual.DataContext;

                    int count = DataVisual.Items.Count;
                    int srcGlobalIndex = CurrentRectanglePopUpInfo.GlobalIndex;
                    DataRow destRow;

                    // Really don't like how we are finding rows from selected Info elsewhere, passing around global indexes, hunting for values, etc.
                    // but we aren't using an indexed database, and the DataGrid virtualisation means actual visual
                    // DataRowView's visualisation may not yet exist to find UI elements to display them, and DataRow instances weren't matching up when comparing them directly,
                    // so we do the below (with supporting code elsewhere). Does the job.

                    for (int i = 0; i < count; i++)
                    {
                        destRow = ((DataRowView)DataVisual.Items[i]).Row;
                        if (srcGlobalIndex == (int)destRow.ItemArray[Globals.GlobalIndexColumn])
                        {
                            DataVisual.SelectedIndex = i;
                            DataVisual.ScrollIntoView(DataVisual.SelectedItem);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print($"Unable to find selected info in DataVisual{ex.Message}");
                }
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ApplyPotentialFilterArkColor();
        }


        #endregion Info and PopUps

        #region MassMarkers

        // Note: at the moment mass markers are based around CreatureId's, but could be used in the future for other purposes (e.g. resources)

        private void ShowMassMarkers(string creatureId, string mapName)
        {
            Info.ShowMassMarkers(creatureId, mapName, (DataTable)DataVisual.DataContext, MassMarkerHolder);
        }

        private void RemoveMassMarkers()
        {
            Info.RemoveMassMarkers(MassMarkerHolder);
        }

        private void ShowSameType_Click(object sender, RoutedEventArgs e)
        {
            bool showSameType = ShowSameType.IsChecked ?? false;
            UserSettings.ShowSameType = showSameType;

            if (showSameType)
                ShowMassMarkers(LastSelected_CreatureId, LastSelected_Map);
            else
                RemoveMassMarkers();
        }

        #endregion MassMarkers

        #region Misc

        private static bool IsInteger(string text)
        {
            return int.TryParse(text, out _);
        }

        private void SetFlashMessage(string message)
        {
            FlashMessage.Text = message;
            FlashMessage.Visibility = Visibility.Visible;
        }

        private void HideFlashMessage()
        {
            FlashMessage.Visibility = Visibility.Collapsed;
        }

        public void LoadableControlsEnabled(bool isEnabled)
        {
            ServerList.IsEnabled = isEnabled;
            LoadTameDinos.IsEnabled = isEnabled;
            LoadWildDinos.IsEnabled = isEnabled;
            LoadSurvivors.IsEnabled = isEnabled;
        }

        public void LoadingVisualEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                GeneralLoadingSpinner.Child = LoadingSpinner;
                GeneralLoadingSpinner.Visibility = Visibility.Visible;
                ExtraInfoHolder.Visibility = Visibility.Hidden;
            }
            else
            {
                GeneralLoadingSpinner.Child = null;
                GeneralLoadingSpinner.Visibility = Visibility.Hidden;
                ExtraInfoHolder.Visibility = Visibility.Visible;
            }
        }

        private void SaveUserSettings()
        {
            // Any non-specifically saved UserSettings, update here incase they have changed
            UserSettings.Zoom = Zoom.Value;
            UserSettings.ShowPopups = ShowPopups.IsChecked ?? false;
            UserSettings.IncludeDetailsInPopUps = IncludeDetailsInPopUps.IsChecked ?? false;
        }

        private static void ExitApplication()
        {
            Application.Current.Shutdown();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            if ((string)About.Content == "About")
            {
                ShowExtraInfo(AboutExtraInfo);
                About.Content = "Show data info";
            }
            else
            {
                ShowExtraInfo(ExtraInfoHolder, ExtraInfoMapDataHolder);
                About.Content = "About";
            }

        }

        private void CustomMarker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HandleMapImageClick(e.GetPosition(MapImage));
        }

        private void HideCustomMarker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DisableCustomMarker();
        }

        private void MapHolder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Very basic easing function to make the zoom usably faster
            double pos = Zoom.Value;

            pos = (Zoom.Value - 1.0d) / 24.0d;              // 1->25 -> 0->1

            if (e.Delta > 0)
                pos += pos + 0.01;
            else if (e.Delta < 0)
                pos -= (pos * 0.5);

            Debug.Print($"{pos}");

            Debug.Print($"{pos}");
            Zoom.Value = (pos * 24.0) + 1.0;
            Debug.Print($"{Zoom.Value}");
        }

        #endregion Misc

        //private bool DebugEnabled { get; set; } = true;

        //private StringBuilder DebugText { get; set; } = new();

        //private void AddToDebug(string content)
        //{
        //    if (DebugEnabled)
        //    {
        //        DebugText.AppendLine(content);
        //        DebugInfo.Text = DebugText.ToString();
        //    }
        //}

        //private void AddToDebug(StringBuilder content)
        //{
        //    if (DebugEnabled)
        //    {
        //        DebugText.Append(content);
        //        DebugInfo.Text = DebugText.ToString();
        //    }
        //}
    }
}
