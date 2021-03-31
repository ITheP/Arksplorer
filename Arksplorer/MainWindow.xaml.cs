using Arksplorer;
using Arksplorer.Properties;
using Microsoft.VisualBasic;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Media;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Json;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Resources;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Threading;
//using System.Windows.Shapes;
using System.Xaml;
using System.Xml.Linq;

namespace Arksplorer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Dictionary<string, BitmapImage> MapImages { get; } = new();
        private static string CurrentMapImage { get; set; } = "";
        private static UIElement LoadingSpinner { get; set; }
        private static ServerConfig ServerConfig { get; set; }

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

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            // These are visible at design time to aid design, but want to hide them when window first opens
            MapImage.Visibility = Visibility.Collapsed;
            Marker.Visibility = Visibility.Collapsed;
            ServerLoadedControls.Visibility = Visibility.Collapsed;
            DataVisual.Visibility = Visibility.Collapsed;
            OverviewInfo.Visibility = Visibility.Collapsed;

            // Disable all controls that are reliant on working server connection...
            LoadableControlsEnabled(false);
            // ...except we re-enable the one control that will let us specify a server location :)
            ServerList.IsEnabled = true;

            // We drag the loading effect out the page when not visible,
            // so it's not being calculated while hidden/collapsed (have seen overhead happen even when not visible if its linked into the page)
            LoadingSpinner = GeneralLoadingSpinner.Child;

            InitWebTabs();

            // Grab config from server that feeds into all this
            try
            {
                Lookup.LoadDinoData("./Data/Lookup-Dinos.json");

                string lastServer = Settings.Default.LastServer;

                KnownServers = JsonSerializer.Deserialize<List<Server>>(File.ReadAllText("./Servers.json"));
                ServerList.ItemsSource = KnownServers;

                // If we have a previous server, then setting the ServerList will trigger loading of the server on its SelectionChanged event
                ServerList.SelectedValue = lastServer;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was a problem during start up...{Environment.NewLine}{ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}{Environment.NewLine}Application will now exit.", "Start up error", MessageBoxButton.OK, MessageBoxImage.Error);
                ExitApplication();
            }

            Timer = new();
            Timer.Interval = new TimeSpan(0, 0, 1); // Recheck if the cache can be refreshed every 10 seconds
            Timer.Tick += TimerTrigger;
            Timer.Start();
        }

        //private List<WebTab> WebTabs { get; set; }
        private WebTab ArkpediaWebTab { get; set; }
        private WebTab DododexWebTab { get; set; }

        private void InitWebTabs()
        {
            ArkpediaWebTab = new(ArkpediaBrowser) { LoadingControl = ArkpediaLoadingSpinner };
            DododexWebTab = new(DododexBrowser) { LoadingControl = DododexLoadingSpinner };
        }

        public void SaveMapPreference(object sender, RoutedEventArgs e)
        {
            // Save currently flagged maps to settings
            string flaggedMaps = "";

            foreach (var selection in MapList)
            {
                if (selection.Load)
                    flaggedMaps += selection.Name;
            }

            if (Settings.Default.LastMaps != flaggedMaps)
                Settings.Default.LastMaps = flaggedMaps;
        }

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

        // Different types of dino may be loaded with different maps. There may be no consistence across packages of data!

        private bool CheckingQueue { get; set; }

        private void CheckAndRefreshCache(string from)
        {
            // Not propper locking, but a basic check to make sure we arent re-triggering the setting up of a queue while its mid-process
            if (CheckingQueue)
                return;

            CheckingQueue = true;

            DateTime now = DateTime.Now;

            Debug.Print($"{now:HH:mm:ss} {from}");

            List<QueueDataItem> queue = new();

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
                        AddToQueue(queue, map.Key, package.Value.Metadata, ForceRefreshOfData); // false);
                    }

                    string difference = $"{(mapPackage.ApproxNextServerUpdateTimestamp - now):mm\\:ss}";

                    Debug.Print($"{package.Value.Metadata.Description}.{map.Key} {mapPackage.Timestamp} {(expired ? $"Data expired {difference} ago" : $"Already using latest available data, expires in {difference}")}");
                }
            }

            if (queue.Count > 0)
            {
                Debug.Print($"Refreshing cache...");
                // We don't automatically refresh the visual display as someone may be part way through looking at data.
                // Next filter/search it will show the new data.
                LoadQueue(queue, false);
            }

            CheckingQueue = false;
        }

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

        private static void ExitApplication()
        {
            Application.Current.Shutdown();
        }

        private void SetAlarm(object sender, RoutedEventArgs e)
        {
            string duration = (string)((Button)sender).Tag;
            AlarmTimestamp = DateTime.Now.AddMinutes(double.Parse(duration));
            AlarmTriggered = false;
            AlarmEnabled = true;
            TimerTrigger();
        }

        private void TriggerAlarm()
        {
            AlarmTriggered = true;
            PlaySample("FeedMe");
        }

        private void RemoveAlarm()
        {
            AlarmEnabled = false;
            Player.Stop();
            AlarmTimeLeft.Foreground = Brushes.Black;
            AlarmTimeLeft.FontWeight = FontWeights.Normal;
            AlarmTimeLeft.Text = "Off";
        }

        // Problems with MediaPlayer not playing. Not sure why! To retry with a different mp3?
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

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            ShowExtraInfo(AboutExtraInfo);
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilterCriteria(false);
        }

        private void RemoveFilter_Click(object sender, RoutedEventArgs e)
        {
            ClearFilter();
        }

        private async void TameDinos_Click(object sender, RoutedEventArgs e)
        {
            SetUpQue(Types.TameMetadata);
        }

        private async void SetUpQue(MetaData type)
        {
            if (!IsServerConfigLoaded())
                return;

            ProcessingQueue = true;

            LoadableControlsEnabled(false);
            LoadingVisualEnabled(true);

            List<QueueDataItem> queue = new();

            foreach (var selection in MapList)
            {
                if (selection.Load)
                    AddToQueue(queue, selection.Name, type, ForceRefreshOfData);
            }

            LoadQueue(queue, true);
        }


        private async void WildDinos_Click(object sender, RoutedEventArgs e)
        {
            SetUpQue(Types.WildMetadata);
        }

        private void Survivors_Click(object sender, RoutedEventArgs e)
        {
            SetUpQue(Types.SurvivorMetadata);
        }

        public static BitmapImage LoadImage(string mapName)
        {
            if (MapImages.ContainsKey(mapName))
                return MapImages[mapName];

            var path = Path.Combine(Environment.CurrentDirectory, "Images", $"{mapName}.png");
            if (!File.Exists(path))
            {
                path = Path.Combine(Environment.CurrentDirectory, "Images", $"{mapName}.jpg");
                if (!File.Exists(path))
                    return null; // backup image
            }

            var uri = new Uri(path);
            var bitmap = new BitmapImage(uri);

            MapImages.Add(mapName, bitmap);

            return bitmap;
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

        private void ApplyFilterCriteria(bool exactOnly = false)
        {
            string criteria = FilterCriteria.Text;

            FilterDataTable(CurrentDataPackage, criteria, exactOnly);
        }

        private void FilterDataTable(DataPackage dataPackage, string criteria, bool exactOnly)
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
                    finalFilter += $"{separator}({filter.Replace("#", trimmed)})";
                    separator = " AND ";
                }
            }

            if (dataPackage.Metadata.IncludesLevel)
            {
                string extra = $"{(string.IsNullOrEmpty(finalFilter) ? "" : $" AND ({finalFilter})") }";

                if (levelFilter == "Above")
                    finalFilter = $"(Lvl < {FilterLevelNumber.Text}){extra}";
                else if (levelFilter == "Below")
                    finalFilter = $"(Lvl > {FilterLevelNumber.Text}){extra}";
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
            if (CurrentDataPackage.Data != null)
                DataVisual.DataContext = CurrentDataPackage.Data;
        }

        private static Dictionary<string, DataPackage> DataPackages { get; set; } = new Dictionary<string, DataPackage>();

        private static DataPackage CurrentDataPackage { get; set; }
        private static async void AddToQueue(List<QueueDataItem> queue, string map, MetaData metaData, bool forceRefresh)
        {
            queue.Add(new QueueDataItem()
            {
                MapName = map,
                MetaData = metaData,
                DataUri = ServerConfig.GetUri($"{map}.{metaData.ArkEntityType}"),
                TimestampUri = ServerConfig.GetUri($"{map}.timestamp"),
                ForceRefresh = forceRefresh
            });
        }

        private Cursor PrevCursor { get; set; }
        private bool ProcessingQueue { get; set; }

        // When a queue is loading, no others should load as controls that could trigger another load are disabled.
        private async void LoadQueue(List<QueueDataItem> queue, bool autoUpdateDataGrid)
        {
            // PrevCursor = Mouse.OverrideCursor;
            //  Mouse.OverrideCursor = Cursors.Wait;

            int mapsLoaded = await Task.Run(() => ProcessQueue(queue, autoUpdateDataGrid));
        }

        public void LoadableControlsEnabled(bool isEnabled)
        {
            ServerList.IsEnabled = isEnabled;
            TameDinos.IsEnabled = isEnabled;
            WildDinos.IsEnabled = isEnabled;
            Survivors.IsEnabled = isEnabled;
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

        private void ShowData(string type, bool updateVisualDataGrid)
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
                ShowExtraInfo(ExtraInfo, ExtraInfoMapDataHolder);

                ExtraInfoMapDataHolder.Visibility = Visibility.Visible;

                if (CurrentDataPackage.Data == null)
                    Status.Text = $"No data found!";
                else
                {
                    if (CurrentDataPackage.Data.Rows.Count == 0)
                        Status.Text = $"No data loaded yet";
                    else
                        Status.Text = $"Loaded {CurrentDataPackage.Data.Rows.Count} {CurrentDataPackage.Metadata.Description}s!";
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
            InitColumnPositions(data);
            DataVisualCount.Text = $"{data.Rows.Count} entries";
            HideFlashMessage();
        }

        static readonly HttpClient httpClient = new();

        private async void LoadServerConfig(Server server)
        {
            string error = "";

            try
            {
                RawServerData rawServerData = await httpClient.GetFromJsonAsync<RawServerData>(new Uri(server.Url));
                ServerConfig = new ServerConfig(rawServerData);

                // Save this as the last selected server - that we know has worked!
                Properties.Settings.Default.LastServer = server.Name;

                // If we were successfull, we also want to clear down any displayed data, caches, etc. which could be from an older server
                CurrentDataPackage = null;

                Debug.Print($"LoadServerConfig.CurrentDataPackage = null");

                DataPackages.Clear();

                Dispatcher.Invoke(() =>
                {
                    MapList.Clear();
                    string lastMaps = Properties.Settings.Default.LastMaps;
                    foreach (var mapName in ServerConfig.Maps)
                        MapList.Add(new() { Name = mapName, CacheState = "Not loaded", Load = lastMaps.Contains(mapName) });

                    DataVisual.DataContext = null; // Clear any current results list
                    Status.Text = $"Welcome! This server updates data approx. every {ServerConfig.RefreshRate} minutes.";
                    LoadableControlsEnabled(true);
                    LoadingVisualEnabled(false);
                    ServerLoadedControls.Visibility = Visibility.Visible;
                    GeneralLoadingSpinner.Child = null;
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
        private async Task<int> ProcessQueue(List<QueueDataItem> queue, bool autoUpdateVisualDataGrid)
        {
            int count = 0;
            string type = "";
            int newRecords = 0;
            string description = "";
            int totalCount = queue.Count;
            int doneCount = 0;

            foreach (QueueDataItem item in queue)
            {
                type = item.MetaData.ArkEntityType;
                string mapName = item.MapName;
                description = item.MetaData.Description;
                bool grabData = false;
                string rawServerTimestamp = string.Empty;
                DateTime serverTimestamp = DateTime.MinValue;

                bool dataExists = (DataPackages.ContainsKey(type) && DataPackages[type].IndividualMaps.ContainsKey(mapName));

                if (dataExists)
                {
                    if (item.ForceRefresh)
                    {
                        Debug.Print("ProcessQue.GrabData forced refresh of data");
                        grabData = true;
                    }
                    else
                    {
                        // Check for timeout
                        MapPackage mapPackage = DataPackages[type].IndividualMaps[mapName];
                        DataTimestamp jsonTimestamp = await httpClient.GetFromJsonAsync<DataTimestamp>(item.TimestampUri);
                        rawServerTimestamp = jsonTimestamp.Date;

                        if (DateTime.TryParseExact(rawServerTimestamp, "yyyyMMdd_HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out serverTimestamp))
                        {
                            if (serverTimestamp > mapPackage.Timestamp)
                            {
                                // Newer data available!!!
                                Debug.Print("ProcessQue.GrabData on server time stamp out of date");
                                grabData = true;
                            }
                        }
                        else
                            MessageBox.Show($"Error reading timestamp for {mapName}.{type}, value returned: '{rawServerTimestamp}'", "Date error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    grabData = true;
                }

                if (grabData)
                {
                    // ToDo: More obvious+visual loading marker
                    this.Dispatcher.Invoke(() => Status.Text = $"Loading {++doneCount}/{totalCount}{Environment.NewLine}{mapName} {type} data...");
                    // We re-use the timestamp from earlier, if its available. Saves a server trip.
                    newRecords += await Task.Run(() => GetDataAsync(item, rawServerTimestamp, serverTimestamp));
                }
                else
                {
                    Debug.Print($"Load of data skipped, already exists for {type}.{mapName} and is up to date");
                    Debug.Print($"{type}.{mapName}.{serverTimestamp} Already using latest available data, loading skipped");
                }
            }

            Dispatcher.Invoke(() =>
            {
                Debug.Print($"ProcessQue with done count {doneCount}");
                if (doneCount > 0)
                {
                    if (newRecords > 0)
                        Status.Text = $"Loaded {newRecords} {description}s!";
                    else
                        Status.Text = "Select maps to load first";

                }
                else
                {
                    Status.Text = "Nothing new to load";
                }

                // Even if nothing was loaded, if it's a different type of dino, we still want to `show things` - so always update here
                ShowData(type, autoUpdateVisualDataGrid);

                LoadableControlsEnabled(true);
                LoadingVisualEnabled(false);
                //   Mouse.OverrideCursor = PrevCursor;
            });

            ProcessingQueue = false;

            return count;
        }

        /// <summary>
        /// Gets (or creates) a DataPackage (collection of data around a map)
        /// Grabs data for a type/map
        /// Updates/creates set of data in DataPackage IndividualMaps with new data
        /// Makes master data stale
        /// When required, stale data will regenerate to master list
        /// Means that we only ever update individual maps, if required - rather than requiry all if we are requested to refresh one
        /// Also means we only update stale data when required (rather than every time, if we are e.g. loading multiple maps we do this at the end)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map"></param>
        /// <param name="uri"></param>
        /// <param name="metaData"></param>
        /// <returns></returns>
        private async Task<int> GetDataAsync(QueueDataItem item, string rawServerTimestamp, DateTime serverTimestamp)
        {
            try
            {
                string metaDataType = item.MetaData.ArkEntityType;
                DataPackage dataPackage;

                if (DataPackages.ContainsKey(metaDataType))
                {
                    dataPackage = DataPackages[metaDataType];
                }
                else
                {
                    dataPackage = new DataPackage()
                    {
                        Metadata = item.MetaData,
                        IndividualMaps = new Dictionary<string, MapPackage>()
                    };
                    DataPackages.Add(metaDataType, dataPackage);
                }

                string mapName = item.MapName;

                IEnumerable<IArkEntity> result;
#if DEBUG
                if (ForceLocalLoad)
                    result = (IEnumerable<IArkEntity>)JsonSerializer.Deserialize(File.ReadAllText("./temp.json"), typeof(List<>).MakeGenericType(item.MetaData.JsonClassType));
                else
#endif
                    result = (IEnumerable<IArkEntity>)await httpClient.GetFromJsonAsync(item.DataUri, typeof(List<>).MakeGenericType(item.MetaData.JsonClassType));

                DataTable newData = DataTableExtensions.AddToDataTable(result, item.MetaData.JsonClassType, mapName, item.MetaData, null);
                MapPackage newMapPackage = new();
                newMapPackage.Data = newData;

                // Only calculate timestamps if they haven't already just been snagged
                if (rawServerTimestamp == string.Empty)
                {
                    DataTimestamp jsonTimestamp = await httpClient.GetFromJsonAsync<DataTimestamp>(item.TimestampUri);
                    rawServerTimestamp = jsonTimestamp.Date;
                    if (DateTime.TryParseExact(rawServerTimestamp, "yyyyMMdd_HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out serverTimestamp))
                    {
                        Debug.Print($"GetDataAsync updating from web site into newMapPackage.Timestamp from {newMapPackage.Timestamp} to {serverTimestamp} : {mapName}.{metaDataType}");
                        newMapPackage.Timestamp = serverTimestamp;
                    }
                    else
                        MessageBox.Show($"Error reading timestamp for {mapName}.{metaDataType}, value returned: '{rawServerTimestamp}'", "Date error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Debug.Print($"GetDataAsync updating from cached timestamp into newMapPackage.Timestamp from {newMapPackage.Timestamp} to {serverTimestamp} : {mapName}.{metaDataType}");
                    newMapPackage.Timestamp = serverTimestamp;
                }
                newMapPackage.RawTimestamp = rawServerTimestamp;
                newMapPackage.ApproxNextServerUpdateTimestamp = newMapPackage.Timestamp.AddMinutes(ServerConfig.RefreshRate);

                var maps = dataPackage.IndividualMaps;
                if (maps.ContainsKey(mapName))
                    maps[mapName] = newMapPackage;
                else
                    maps.Add(mapName, newMapPackage);

                Debug.Print($"GetDataAsync CurrentDataPackage == dataPackage {CurrentDataPackage == dataPackage} to trigger visual refresh");

                this.Dispatcher.Invoke(() => { ExtraInfoMapData.ItemsSource = CurrentDataPackage?.IndividualMaps.ToList(); }); //THIS should work but doesnt visually --> if (CurrentDataPackage == dataPackage) ExtraInfoMapData.Items.Refresh(); });

                dataPackage.DataIsStale = true;

                return newData.Rows.Count;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error: {ex.StatusCode}", $"Error retrieving JSON data for {item.MetaData.Description}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show($"Invalid content type: {ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}", $"Error retrieving JSON data for {item.MetaData.Description}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Invalid JSON: {ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}", $"Error retrieving JSON data for {item.MetaData.Description}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem loading data: {ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}", $"Error retrieving JSON data for {item.MetaData.Description}", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return -1;
        }

        private string LastCreatureId { get; set; } = string.Empty;

        private void DataVisual_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView entity = (DataRowView)(DataVisual.SelectedItem);

            if (entity == null)
            {
                SetSelectedInfo(null);
                return;
            }

            Info info = CreateInfoFromRow(entity.Row);

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

            // Map name is always in column 0
            string mapName = (string)entity[0];
            if (mapName != CurrentMapImage)
            {
                MapImage.Source = LoadImage(mapName);
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
        }

        // Note: at the moment mass markers are based around CreatureId's, but could be used in the future for other purposes (e.g. resources)

        private string LastSelected_Map { get; set; }
        private string LastSelected_CreatureId { get; set; }

        private void RemoveMassMarkers()
        {
            MassMarkerHolder.Children.Clear();
        }

        private int MapColumn { get; set; }
        private int LatColumn { get; set; }
        private int LonColumn { get; set; }
        private int CreatureIdColumn { get; set; }
        private int LvlColumn { get; set; }
        private int BaseColumn { get; set; }
        private int CreatureColumn { get; set; }
        private int NameColumn { get; set; }
        private int SexColumn { get; set; }
        private int CryoColumn { get; set; }

        /// <summary>
        /// Column positions are all assumed to be dynamic (even if they are actually pretty static)
        /// This is to account for future expansion, new datasets, columns, definitions, changes etc.
        /// so we don't need to hardcode/recode values. Not quite as performant as hardcoding but does
        /// the job nicely.
        /// </summary>
        /// <param name="data"></param>
        private void InitColumnPositions(DataTable data)
        {
            MapColumn = data.Columns["Map"]?.Ordinal ?? -1;
            LatColumn = data.Columns["Lat"]?.Ordinal ?? -1;
            LonColumn = data.Columns["Lon"]?.Ordinal ?? -1;
            CreatureIdColumn = data.Columns["CreatureId"]?.Ordinal ?? -1;
            LvlColumn = data.Columns["Lvl"]?.Ordinal ?? -1;
            BaseColumn = data.Columns["Base"]?.Ordinal ?? -1;
            CreatureColumn = data.Columns["Creature"]?.Ordinal ?? -1;
            NameColumn = data.Columns["Name"]?.Ordinal ?? -1;
            SexColumn = data.Columns["Sex"]?.Ordinal ?? -1;
            CryoColumn = data.Columns["Cryo"]?.Ordinal ?? -1;
        }


        // ToDo: Regeneration of extra info all the time is a needless overhead. Cache once generate and reuse!
        private void ShowMassMarkers(string creatureId, string mapName)
        {
            RemoveMassMarkers();    // Make sure any previous markers are no longer there

            if (string.IsNullOrWhiteSpace(creatureId) || string.IsNullOrWhiteSpace(mapName))
                return;

            var data = (DataTable)DataVisual.DataContext;

            if (data == null)
                return;

            if (MapColumn == -1 || LatColumn == -1 || LonColumn == -1 || CreatureIdColumn == -1)
                return;

            double lat, lon;
            int level;
            // Translate level from 0->150+ to 0->1%
            double levelPerc;
            byte g;

            foreach (DataRow row in data.Rows)
            {
                if ((string)row[MapColumn] == mapName)
                {
                    if ((string)row[CreatureIdColumn] == creatureId)
                    {
                        lat = (float)row[LatColumn];
                        lon = (float)row[LonColumn];

                        if (lat > -1 && lon > -1)
                        {
                            Info info = CreateInfoFromRow(row);

                            level = info.Level;

                            double yPos = lat - 50.0f;
                            double xPos = lon - 50.0f;

                            levelPerc = level / 150.0d; // e.g. 120 out of max 150 = 80% <-- this will need to become dynamic e.g. if its a Tek dino and max wild level is 180
                            if (levelPerc > 1.0)
                                levelPerc = 1.0;

                            g = (byte)(255 * (1.0 - levelPerc));

                            var rec = new System.Windows.Shapes.Rectangle()
                            {
                                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, g, 0)),
                                Stroke = (info.Sex == "M" ? Brushes.DarkBlue : Brushes.DarkRed),
                                StrokeThickness = 0.2,
                                Width = 1.4,
                                Height = 1.4,
                                RenderTransform = new TranslateTransform(xPos, yPos)
                            };

                            rec.Tag = info;

                            MassMarkerHolder.Children.Add(rec);
                        }
                    }
                }
            }
        }

        private Info CreateInfoFromRow(DataRow row)
        {
            Info info = new();

            info.Lat = (float)row[LatColumn];
            info.Lon = (float)row[LonColumn];

            if (NameColumn > -1)
                info.Name = row[NameColumn] as string;

            //info.Add("Location", $"{info.Lat:0.00},{info.Lon:0.00}");

            if (CreatureColumn > -1)
                info.Creature = row[CreatureColumn] as string;

            if (CreatureIdColumn > -1)
                info.CreatureId = row[CreatureIdColumn] as string;

            if (SexColumn > -1)
            {
                string sex = row[SexColumn] as string;
                info.Add("Sex", sex);
                info.Sex = sex;
                if (sex == "M")
                    info.AddIcon(Icons.Male);
                else
                    info.AddIcon(Icons.Female);
            }

            if (BaseColumn > -1)
                info.Add("Base level", $"{row[BaseColumn] as int?}");

            int level = (int)row[LvlColumn];
            info.Add("Level", $"{level}");
            info.Level = level;

            if (CryoColumn > -1)
            {
                if ((bool)row[CryoColumn] == true)
                {
                    info.AddIcon(Icons.Cryopod);
                    info.Cryoed = true;
                }
            }

            return (info);
        }

        private void SetSelectedInfo(Info info) //string message)
        {
            if (info == null)
                return;

            OverviewInfo.ShowInfo(info, true);

            OverviewInfo.Visibility = Visibility.Visible;
        }

        private void ExactFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilterCriteria(true);
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

        private void FilterCriteria_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter || e.Key == Key.Tab)
                ApplyFilterCriteria();
        }

        private void Resources_Click(object sender, RoutedEventArgs e)
        {
            ShowExtraInfo(ResourcesAndLinksExtraInfo);
        }

        private void ShowExtraInfo(UIElement whatToShow, UIElement whatToShow2 = null)
        {
            ExtraInfoHolder.Visibility = Visibility.Collapsed;
            ExtraInfoMapDataHolder.Visibility = Visibility.Collapsed;
            ResourcesAndLinksExtraInfo.Visibility = Visibility.Collapsed;
            AboutExtraInfo.Visibility = Visibility.Collapsed;

            if (whatToShow != null)
                whatToShow.Visibility = Visibility.Visible;

            if (whatToShow2 != null)
                whatToShow2.Visibility = Visibility.Visible;
        }

        private DataGridRow LastDataGridRow { get; set; }
        private System.Windows.Shapes.Rectangle LastRectangle { get; set; }
        // Note that we COULD attach mouse enter/leave events to all rectangles we have created (mass markers) along with
        // other entities, but then we have to keep track of all the events and remove on rectangle disposal as well (else
        // we can end up with references hanging around). Nightmare. So we don't bother and just check if we are
        // over a rectangle or not with a tag of type Info.
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            bool updatePopUpPosition = false;

            // MouseOver on the DataGrid results list
            System.Windows.Point pos = e.GetPosition(Root); // DataVisual);
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(Root, pos); // DataVisual, pos);
            var hitElement = hitTestResult?.VisualHit;
            if (hitElement != null)
            {
                DependencyObject element = hitElement;
                while (element != null && !(element is DataGridCell || element is System.Windows.Shapes.Rectangle))
                    element = VisualTreeHelper.GetParent(element);

                if (element != null)
                {
                    if (element is System.Windows.Shapes.Rectangle rectangle)
                    {
                        // Only interested in rectangles that have an Info in their tag - otherwise we assume its some random other rectangle
                        if (rectangle.Tag is Info info)
                        {
                            if (rectangle != LastRectangle)
                            {
                                LastRectangle = rectangle;
                                PopUpInfoVisual.ShowInfo(info);
                            }

                            // updatePopUpPosition = true;
                        }
                    }
                    else
                    {
                        DataGridCell cell = (DataGridCell)element;
                        DataGridRow gridRow = DataGridRow.GetRowContainingElement(cell);
                        if (gridRow != LastDataGridRow)
                        {
                            LastDataGridRow = gridRow;

                            DataRowView dataRowView = (System.Data.DataRowView)gridRow.Item;
                            PopUpInfoVisual.ShowInfo(CreateInfoFromRow(dataRowView.Row));
                        }
                    }

                    // Even if we don't change how this looks, we want to make sure its position is updated
                    updatePopUpPosition = true;
                }
            }

            PopUpInfo.IsOpen = updatePopUpPosition;
            if (updatePopUpPosition)
            {
                PopUpInfo.HorizontalOffset = pos.X + 25;
                PopUpInfo.VerticalOffset = pos.Y + 25;
            }

            if (FlashMessage.Visibility != Visibility.Visible)
                return;

            pos = e.GetPosition(this);
            var width = Width;
            var height = Height;

            var percX = (pos.X / width);
            var percY = (pos.Y / height);


            var brush = new SolidColorBrush(Colour.RGBFromHSL(percX, 1.0, 1.0 - percY));
            FlashMessage.Foreground = brush;
        }

        private void HandleLinkClick(object sender, RequestNavigateEventArgs e)
        {
            OpenUrlInExternalBrowser(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private static void OpenUrlInExternalBrowser(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void ServerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Server server = (Server)(ServerList.SelectedItem);
            LoadServer(server);
        }

        private static void GoBack(Microsoft.Web.WebView2.Wpf.WebView2 browser)
        {
            if (browser.CanGoBack)
                browser.GoBack();
        }

        public static void Navigate(Microsoft.Web.WebView2.Wpf.WebView2 browser, string url, TabItem tab = null)
        {
            try
            {
                if (browser.CoreWebView2 == null)
                {
                    browser.Source = new Uri(url);
                    //browser.CoreWebView2InitializationCompleted += Webview_CoreWebView2InitializationCompleted;
                    //browser.EnsureCoreWebView2Async();
                }
                else
                {
                    browser.CoreWebView2?.Navigate(url);
                }

                if (tab != null)
                    tab.Focus();
            }
            catch (Exception ex)
            {
                Debug.Print($"Error navigating to '{url}': {ex.Message}");
            }
        }

        private void DododexNavigate_Click(object sender, RoutedEventArgs e)
        {
            Navigate(DododexBrowser, (string)((Button)sender).Tag);
        }

        private void DododexBack_Click(object sender, RoutedEventArgs e)
        {
            GoBack(DododexBrowser);
        }

        private void ArkpediaNavigate_Click(object sender, RoutedEventArgs e)
        {
            Navigate(ArkpediaBrowser, (string)((Button)sender).Tag);
        }


        private void ArkpediaBack_Click(object sender, RoutedEventArgs e)
        {
            GoBack(ArkpediaBrowser);
        }

        private void ArkpediaOpenExternal_Click(object sender, RoutedEventArgs e)
        {
            OpenUrlInExternalBrowser(ArkpediaWebTab.CurrentUrl);
        }

        private void DododexOpenExternal_Click(object sender, RoutedEventArgs e)
        {
            OpenUrlInExternalBrowser(DododexWebTab.CurrentUrl);
        }

        private void DataVisual_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridTextColumn column)
            {
                //      if (column.Header == "Index")
                //        column.Visibility = Visibility.Collapsed;

                if (e.PropertyType == typeof(Single))
                    column.Binding = new Binding(e.PropertyName) { StringFormat = "N2" };
            }

        }

        private void AlarmOff_Click(object sender, RoutedEventArgs e)
        {
            RemoveAlarm();
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

        private static bool IsInteger(string text)
        {
            return int.TryParse(text, out _);
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

            Settings.Default.LastMaps = selectedMaps;
        }

        private void ShowSameType_Click(object sender, RoutedEventArgs e)
        {
            if (ShowSameType.IsChecked ?? false)
                ShowMassMarkers(LastSelected_CreatureId, LastSelected_Map);
            else
                RemoveMassMarkers();
        }

        private void FilterLevelType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilterCriteria();
        }

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
