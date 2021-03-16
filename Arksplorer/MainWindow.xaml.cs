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
using System.Linq;
using System.Media;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Json;
using System.Numerics;
using System.Reflection;
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

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            // These are visible at design time to aid design, but want to hide them when window first opens
            MapImage.Visibility = Visibility.Collapsed;
            Marker.Visibility = Visibility.Collapsed;
            ServerLoadedControls.Visibility = Visibility.Collapsed;
            DataVisual.Visibility = Visibility.Collapsed;
            ShowExtraInfo(null);

            // Disable all controls that are reliant on working server connection...
            LoadableControlsEnabled(false);
            // ...except we re-enable the one control that will let us specify a server location :)
            ServerList.IsEnabled = true;

            // We drag the loading effect out the page when not visible,
            // so it's not being calculated while hidden/collapsed (have seen overhead happen even when not visible if its linked into the page)
            LoadingSpinner = GeneralLoadingSpinner.Child;
            //GeneralLoadingSpinner.Child = null;

            InitWebTabs();

            // Grab config from server that feeds into all this
            // ToDo: Config required for where this comes from!
            try
            {
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
        private WebTab DodexWebTab { get; set; }

        private void InitWebTabs()
        {
            ArkpediaWebTab = new(ArkpediaBrowser) { LoadingControl = ArkpediaLoadingSpinner };
            DodexWebTab = new(DodexBrowser) { LoadingControl = DodexLoadingSpinner };
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

            DateTime now = DateTime.Now;
            StringBuilder details = new();

            details.AppendLine($"{now:hh:mm:ss} {from}");

            List<QueueDataItem> queue = new();

            foreach (KeyValuePair<string, DataPackage> package in DataPackages)
            {
                foreach (KeyValuePair<string, MapPackage> map in package.Value.IndividualMaps)
                {
                    var mapPackage = map.Value;
                    bool expired = now > mapPackage.ApproxNextServerUpdateTimestamp;

                    if (expired)
                    {
                        // We are reloading what we already have
                        // Load process will flush old data if it exists first
                        AddToQueue(queue, map.Key, package.Value.Metadata, false);
                    }

                    string difference = $"{(mapPackage.ApproxNextServerUpdateTimestamp - now):mm\\:ss}";
                    details.AppendLine($"{package.Value.Metadata.Description}.{map.Key}.{mapPackage.Timestamp} {difference} {(expired ? "Expired" : "")}");
                }
            }

            if (queue.Count > 0)
            {
                details.AppendLine($"Refreshing cache...");
                LoadQueue(queue, false);
            }

            DebugInfo.Text = details.ToString();

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
                    AddToQueue(queue, selection.Name, type, false);
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
            string levelFilter = FilterLevelType.SelectedValue.ToString();

            if (dataPackage == null || (string.IsNullOrWhiteSpace(criteria) && levelFilter == "All"))
                return;

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
                {
                    SetDataVisualData(null);

                    //DataVisual.DataContext = null;
                    //SetFlashMessage("No entries found");
                    //DataVisualCount.Text = "No entries found";
                }
                else
                {
                    //DataVisual.DataContext = filteredRows.CopyToDataTable();
                    SetDataVisualData(filteredRows.CopyToDataTable());
                    //DataVisualCount.Text = $"{filteredRows.Length} entries";
                    //HideFlashMessage();
                }
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

        private void ShowData(string type)
        {
            if (DataPackages.ContainsKey(type))
            {
                CurrentDataPackage = DataPackages[type];
                CurrentDataPackage.MakeSureDataIsUpToDate();
                //DataVisual.DataContext = CurrentDataPackage.Data;
                //DataVisualCount.Text = $"{filteredRows.Length} entries";
                SetDataVisualData(CurrentDataPackage.Data);

                ExtraInfoTitle.Text = CurrentDataPackage.MapsDescription;
                ExtraInfo.Text = $"Total loaded: {CurrentDataPackage.Data.Rows.Count}";
                ExtraInfoMapData.ItemsSource = CurrentDataPackage.IndividualMaps;
                ExtraInfoMapData.Items.Refresh();
                ShowExtraInfo(ExtraInfo, ExtraInfoMapDataHolder);

                ExtraInfoMapDataHolder.Visibility = Visibility.Visible;
                DataVisual.Visibility = Visibility.Visible;

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

        private async Task<int> ProcessQueue(List<QueueDataItem> queue, bool autoUpdateDataGrid)
        {
            int count = 0;
            string type = "";   // Should always be the same in a que
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
                    // Check for timeout
                    MapPackage mapPackage = DataPackages[type].IndividualMaps[mapName];
                    DataTimestamp jsonTimestamp = await httpClient.GetFromJsonAsync<DataTimestamp>(item.TimestampUri);
                    rawServerTimestamp = jsonTimestamp.Date;

                    if (DateTime.TryParseExact(rawServerTimestamp, "yyyyMMdd_HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out serverTimestamp))
                    {
                        if (serverTimestamp > mapPackage.Timestamp)
                            // Newer data available!!!
                            grabData = true;
                    }
                    else
                        MessageBox.Show($"Error reading timestamp for {mapName}.{type}, value returned: '{rawServerTimestamp}'", "Date error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    grabData = true;
                }

                if (grabData)
                {
                    // ToDo: Put loading marker
                    this.Dispatcher.Invoke(() => Status.Text = $"Loading {++doneCount}/{totalCount}{Environment.NewLine}{mapName} {type} data...");
                    // We re-use the timestamp from earlier, if its available. Saves a server trip.
                    newRecords += await Task.Run(() => GetDataAsync(item, rawServerTimestamp, serverTimestamp));
                }
                else
                {
                    Debug.Print($"Load of data skipped, already exists for {type}.{mapName} and is up to date");
                }

                count++;
            }


            Dispatcher.Invoke(() =>
            {
                if (doneCount > 0)
                {
                    if (newRecords > 0)
                        Status.Text = $"Loaded {newRecords} {description}s!";
                    else
                        Status.Text = "Select maps to load first";

                    if (autoUpdateDataGrid)
                        ShowData(type);
                }
                else
                    Status.Text = "Nothing new to load";

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

                var result = (IEnumerable<IArkEntity>)await httpClient.GetFromJsonAsync(item.DataUri, typeof(List<>).MakeGenericType(item.MetaData.JsonClassType));

                //DataTable newData = result.AddToDataTable(mapName, item.MetaData, null);
                DataTable newData = DataTableExtensions.AddToDataTable(result, item.MetaData.JsonClassType, mapName, item.MetaData, null);
                MapPackage newMapPackage = new();
                newMapPackage.Data = newData;

                // Only calculate timestamps if they haven't already just been snagged
                if (rawServerTimestamp == string.Empty)
                {
                    DataTimestamp jsonTimestamp = await httpClient.GetFromJsonAsync<DataTimestamp>(item.TimestampUri);
                    rawServerTimestamp = jsonTimestamp.Date;
                    if (DateTime.TryParseExact(rawServerTimestamp, "yyyyMMdd_HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out serverTimestamp))
                        newMapPackage.Timestamp = serverTimestamp;
                    else
                        MessageBox.Show($"Error reading timestamp for {mapName}.{metaDataType}, value returned: '{rawServerTimestamp}'", "Date error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    newMapPackage.Timestamp = serverTimestamp;
                }
                newMapPackage.RawTimestamp = rawServerTimestamp;
                newMapPackage.ApproxNextServerUpdateTimestamp = newMapPackage.Timestamp.AddMinutes(ServerConfig.RefreshRate);

                var maps = dataPackage.IndividualMaps;
                if (maps.ContainsKey(mapName))
                    maps[mapName] = newMapPackage;
                else
                    maps.Add(mapName, newMapPackage);

                this.Dispatcher.Invoke(() => CheckAndRefreshCache("GetDataAsync"));

                dataPackage.DataIsStale = true;

                return newData.Rows.Count;
            }
            catch (HttpRequestException ex)
            {
                Debug.Print($"Error: {ex.StatusCode}");
            }
            catch (NotSupportedException ex)
            {
                Debug.Print($"Invalid content type: {ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}");
            }
            catch (JsonException ex)
            {
                Debug.Print($"Invalid JSON: {ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}");
            }
            catch (Exception ex)
            {
                Debug.Print($"Problem loading data: {ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}");
            }

            return -1;
        }

        private void DataVisual_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.Print($"{DataVisual.SelectedIndex}");

            DataRowView entity = (DataRowView)(DataVisual.SelectedItem);

            if (entity == null)
            {
                SetOverviewMessage(string.Empty);
                return;
            }

            var data = CurrentDataPackage.Data;
            var columns = data.Columns;

            string overviewMessage = $"{CurrentDataPackage.Metadata.Description}\n";

            double lat = -1;
            if (columns["Lat"] != null)
                lat = (float)entity[columns["Lat"].Ordinal];

            double lon = -1;
            if (columns["Lon"] != null)
                lon = (float)entity[columns["Lon"].Ordinal];

            if (lat > -1 && lon > -1)
                overviewMessage += $"Location: {lat:0.00},{lon:0.00}{Environment.NewLine}";


            string creature;
            if (columns["Creature"] != null)
            {
                creature = entity[columns["Creature"].Ordinal].ToString();
                overviewMessage += $"Creature: {creature}{Environment.NewLine}";
            }

            string name;
            if (columns["Name"] != null)
            {
                name = entity[columns["Name"].Ordinal].ToString();      // Can be DBNull
                if (string.IsNullOrWhiteSpace(name))
                    overviewMessage += $"Name not set{Environment.NewLine}";
                else
                    overviewMessage += $"Name: {name}{Environment.NewLine}";
            }

            string sex;
            if (columns["Sex"] != null)
            {
                sex = (string)entity[columns["Sex"].Ordinal];
                overviewMessage += $"Sex: {sex}{Environment.NewLine}";
            }

            int lvl;
            if (columns["Lvl"] != null)
            {
                lvl = (int)entity[columns["lvl"].Ordinal];
                overviewMessage += $"Level: {lvl}";
            }

            bool cryo;
            if (columns["Cryo"] != null)
            {
                cryo = (bool)entity[columns["Cryo"].Ordinal];
                if (cryo)
                    overviewMessage += $"{Environment.NewLine}Cryo'ed";
            }

            // Marker offset
            // translate 0->100 to -50->50
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
                {
                    OverviewMessage.HorizontalAlignment = HorizontalAlignment.Left;
                    OverviewMessage.TextAlignment = TextAlignment.Left;
                }
                else
                {
                    OverviewMessage.HorizontalAlignment = HorizontalAlignment.Right;
                    OverviewMessage.TextAlignment = TextAlignment.Right;
                }

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

            SetOverviewMessage(overviewMessage);
        }

        private void SetOverviewMessage(string message)
        {
            OverviewMessage.Text = message;
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

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (FlashMessage.Visibility != Visibility.Visible)
                return;

            var pos = e.GetPosition(this);
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

        private void OpenUrlInExternalBrowser(string url)
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

        private static void Navigate(Microsoft.Web.WebView2.Wpf.WebView2 browser, string url)
        {
            try
            {
                browser.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
                Debug.Print($"Error navigating to '{url}': {ex.Message}");
            }
        }

        private void DodexNavigate_Click(object sender, RoutedEventArgs e)
        {
            Navigate(DodexBrowser, (string)((Button)sender).Tag);
        }

        private void DodexBack_Click(object sender, RoutedEventArgs e)
        {
            GoBack(DodexBrowser);
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

        private void DodexOpenExternal_Click(object sender, RoutedEventArgs e)
        {
            OpenUrlInExternalBrowser(DodexWebTab.CurrentUrl);
        }

        private void DataVisual_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            DataGridTextColumn column = e.Column as DataGridTextColumn;
            if (column != null && e.PropertyType == typeof(Single))
            {
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

        private bool IsInteger(string text)
        {
            return int.TryParse(text, out _);
        }

        private void MapsToIncludeCheckbox_Click(object sender, RoutedEventArgs e)
        {
            string lastMaps = Properties.Settings.Default.LastMaps;
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
    }
}
