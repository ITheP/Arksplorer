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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

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
        public static ServerConfig ServerConfig { get; set; }

        private static List<Server> KnownServers { get; set; }

        private static DispatcherTimer Timer { get; set; }

        public static ObservableCollection<MapSelection> MapList { get; set; } = new();
        public static bool DetailInPopUps { get; set; }

        /// <summary>
        /// Forces use of local temp.json file as a data file. Used for debugging (rather than having to make a server trip for data we can't control)
        /// </summary>
        public bool ForceLocalLoad { get; set; } = false;

        /// <summary>
        /// Set this to true to force refresh of data reload, even if cache timings have not expired. Handy for debugging.
        /// </summary>
        public bool ForceRefreshOfData { get; set; } = false;

        //private Queue DataQueue { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            DataContext = this;
            // These are visible at design time to aid design, but want to hide them when window first opens
            MapImage.Visibility = Visibility.Collapsed;
            Marker.Visibility = Visibility.Collapsed;
            ServerLoadedControls.Visibility = Visibility.Collapsed;
            DataVisual.Visibility = Visibility.Collapsed;
            OverviewInfo.Visibility = Visibility.Collapsed;
            FilterMap.Visibility = Visibility.Collapsed;
            FilterTribe.Visibility = Visibility.Collapsed;
            FilterCreature.Visibility = Visibility.Collapsed;

            // Disable all controls that are reliant on working server connection...
            LoadableControlsEnabled(false);

            // ...except we re-enable the one control that will let us specify a server location :)
            ServerList.IsEnabled = true;

            Version.Text = Globals.Version;

            // We drag the loading effect out the page when not visible,
            // so it's not being calculated while hidden/collapsed (have seen overhead happen even when not visible if its linked into the page)
            LoadingSpinner = GeneralLoadingSpinner.Child;

            DetailInPopUps = Settings.Default.IncludeDetailsInPopUps;
            IncludeDetailsInPopUps.IsChecked = DetailInPopUps;

            InitWebTabs();

            // Load data and grab config from server that feeds into all this
            try
            {
                Lookup.LoadDataFromLookupFiles();

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
        public WebTab ArkpediaWebTab { get; set; }
        public WebTab DododexWebTab { get; set; }
        public WebTab ServerWebTab { get; set; }

        private void InitWebTabs()
        {
            ArkpediaWebTab = new(ArkpediaBrowser) { LoadingControl = ArkpediaLoadingSpinner, Browser = ArkpediaBrowser, Tab = ArkpediaTab };
            DododexWebTab = new(DododexBrowser) { LoadingControl = DododexLoadingSpinner, Browser = DododexBrowser, Tab = DododexTab };
            ServerWebTab = new(ServerBrowser) { LoadingControl = ServerLoadingSpinner, Browser = ServerBrowser, Tab = ServerTab };
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

        // Different types of dino may be loaded with different maps. There may be no consistency across packages of data!

        private bool CheckingQueue { get; set; }

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

        private void TameDinos_Click(object sender, RoutedEventArgs e)
        {
            SetUpQue(Types.TameMetadata);
        }

        private void SetUpQue(MetaData type)
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
                    queue.AddNewItem( selection.Name, type, ForceRefreshOfData, ServerConfig);
            }

            LoadQueue(queue, true);
        }


        private void WildDinos_Click(object sender, RoutedEventArgs e)
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

        public DataPackage CurrentDataPackage { get; set; }

        private Cursor PrevCursor { get; set; }
        //private bool ProcessingQueue { get; set; }

        // When a queue is loading, no others should load as controls that could trigger another load are disabled.
        private async void LoadQueue(Queue queue, bool autoUpdateDataGrid)
        {
            // PrevCursor = Mouse.OverrideCursor;
            //  Mouse.OverrideCursor = Cursors.Wait;

            int mapsLoaded = await Task.Run(() => queue.Process( autoUpdateDataGrid, DataPackages, this, ServerConfig, ForceLocalLoad));
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
            Info.InitColumnIndexPositions(data);
            DataVisualCount.Text = $"{data.Rows.Count} entries";
            HideFlashMessage();
        }


        private async void LoadServerConfig(Server server)
        {
            string error = "";

            try
            {
                RawServerData rawServerData = await Web.HttpClient.GetFromJsonAsync<RawServerData>(new Uri(server.Url));
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

                    ServerBrowser.Source = new Uri(ServerConfig.Website);
                    ServerHome.Tag = ServerConfig.Website;

                    ServerInfoList.ItemsSource = ServerConfig.GetServerOverview().Items;

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
        private string LastCreatureId { get; set; } = string.Empty;

        private void DataVisual_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

        // Note: at the moment mass markers are based around CreatureId's, but could be used in the future for other purposes (e.g. resources)

        private string LastSelected_Map { get; set; }
        private string LastSelected_CreatureId { get; set; }

        private void SetSelectedInfo(Info info)
        {
            if (info == null)
                return;

            OverviewInfo.ShowInfo(info, true, DetailInPopUps);

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

        private void ExternalResources_Click(object sender, RoutedEventArgs e)
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
        private Info CurrentRectanglePopUpInfo { get; set; }
        private System.Windows.Shapes.Rectangle LastRectangle { get; set; }
        // Note that we COULD attach mouse enter/leave events to all rectangles we have created (mass markers) along with
        // other entities, but then we have to keep track of all the events and remove on rectangle disposal as well (else
        // we can end up with references hanging around). Nightmare. So we don't bother and just check if we are
        // over a rectangle or not with a tag of type Info.
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            bool showPopUpInfo = false;
            CurrentRectanglePopUpInfo = null;

            // MouseOver on the DataGrid results list
            System.Windows.Point pos = e.GetPosition(Root);
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(Root, pos);
            var hitElement = hitTestResult?.VisualHit;
            if (hitElement != null)
            {
                DependencyObject element = hitElement;
                while (element != null && !(element is DataGridCell || element is System.Windows.Shapes.Rectangle))
                    element = VisualTreeHelper.GetParent(element);

                if (element != null)
                {
                    if (element is Rectangle rectangle)
                    {
                        // Only interested in rectangles that have an Info in their tag - otherwise we assume its some random other rectangle
                        if (rectangle.Tag is Info info)
                        {
                            if (rectangle != LastRectangle)
                            {
                                LastRectangle = rectangle;

                                PopUpInfoVisual.ShowInfo(info, false, DetailInPopUps);
                            }

                            // Make sure we remember what we are looking at, then if we mouse_up over a rectangle, this will exist and we know what to show in the main static pop up
                            CurrentRectanglePopUpInfo = info;
                            showPopUpInfo = true;
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
                            PopUpInfoVisual.ShowInfo(new Info(dataRowView.Row), false, DetailInPopUps);
                        }

                        // Even if we don't change how this looks, we want to make sure its position is updated
                        showPopUpInfo = true;
                    }


                }
            }

            PopUpInfo.IsOpen = showPopUpInfo;

            PopUpInfo.HorizontalOffset = pos.X + 25;
            PopUpInfo.VerticalOffset = pos.Y + 25;

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

        private static void GoForward(Microsoft.Web.WebView2.Wpf.WebView2 browser)
        {
            if (browser.CanGoForward)
                browser.GoForward();
        }

        public static void Navigate(WebTab webTab, string url, bool jumpToTab = true)
        {
            try
            {
                var browser = webTab.Browser;

                if (browser.CoreWebView2 == null)
                    browser.Source = new Uri(url);
                else
                    browser.CoreWebView2?.Navigate(url);

                if (jumpToTab && webTab.Tab != null)
                    webTab.Tab.Focus();
            }
            catch (Exception ex)
            {
                Debug.Print($"Error navigating to '{url}': {ex.Message}");
            }
        }

        private void DododexNavigate_Click(object sender, RoutedEventArgs e)
        {
            Navigate(DododexWebTab, (string)((Button)sender).Tag);
        }

        private void DododexBack_Click(object sender, RoutedEventArgs e)
        {
            GoBack(DododexWebTab.Browser);
        }

        private void ArkpediaNavigate_Click(object sender, RoutedEventArgs e)
        {
            Navigate(ArkpediaWebTab, (string)((Button)sender).Tag);
        }


        private void ArkpediaBack_Click(object sender, RoutedEventArgs e)
        {
            GoBack(ArkpediaWebTab.Browser);
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

                    FrameworkElementFactory factory = new(typeof(Rectangle));
                    factory.SetValue(Rectangle.WidthProperty, 32.0d);
                    factory.SetValue(Rectangle.HeightProperty, 16.0d);
                    Binding binding = new($"{e.PropertyName}");
                    binding.Converter = new ArkColorDBNullConverter();
                    factory.SetBinding(Rectangle.FillProperty, binding);

                    dataTemplate.VisualTree = factory;

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

        private void ShowMassMarkers(string creatureId, string mapName)
        {
            Info.ShowMassMarkers(creatureId, mapName, (DataTable)DataVisual.DataContext, MassMarkerHolder);
        }

        private void RemoveMassMarkers()
        {
            Info.RemoveMassMarkers(MassMarkerHolder);
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

        private void ServerBack_Click(object sender, RoutedEventArgs e)
        {
            GoBack(ServerWebTab.Browser);
        }

        private void ServerNavigate_Click(object sender, RoutedEventArgs e)
        {
            Navigate(ServerWebTab, (string)((Button)sender).Tag);
        }

        private void ServerOpenExternal_Click(object sender, RoutedEventArgs e)
        {
            OpenUrlInExternalBrowser(ServerWebTab.CurrentUrl);
        }

        private void ArkpediaForward_Click(object sender, RoutedEventArgs e)
        {
            GoForward(ArkpediaWebTab.Browser);
        }

        private void DododexForward_Click(object sender, RoutedEventArgs e)
        {
            GoForward(DododexWebTab.Browser);
        }

        private void ServerForward_Click(object sender, RoutedEventArgs e)
        {
            GoForward(ServerWebTab.Browser);
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentRectanglePopUpInfo != null)
                SetSelectedInfo(CurrentRectanglePopUpInfo);
        }

        private void ButtonFilter_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FilterCriteria.Text = button.Tag as string;
            ApplyFilterCriteria(true);
        }

        private void IncludeDetailsInPopUps_Click(object sender, RoutedEventArgs e)
        {
            DetailInPopUps = IncludeDetailsInPopUps.IsChecked ?? false;
            Properties.Settings.Default.IncludeDetailsInPopUps = DetailInPopUps;
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
                dataView.SortDescriptions.Add(new SortDescription($"{column.Header}_Sort", column.SortDirection??ListSortDirection.Ascending));

                e.Handled = true;
            }
            else
                return;
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
