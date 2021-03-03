using Arksplorer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Metadata;
using System.Resources;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Xaml;

namespace Arksplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Dictionary<string, BitmapImage> MapImages { get; } = new Dictionary<string, BitmapImage>();
        private static string CurrentMapImage { get; set; } = "";
        private static UIElement LoadingSpinnerStore { get; set; }
        private static ServerConfig ServerConfig { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            // These are visible at design time to aid design, but want to hide them when window first opens
            MapImage.Visibility = Visibility.Collapsed;
            Marker.Visibility = Visibility.Collapsed;

            // We drag the loading effect out the page when not visible,
            // so it's not being calculated while hidden/collapsed (have seen that happen before with a bit of needless background ongoing overhead)
            LoadingSpinnerStore = LoadingSpinner.Child;
            LoadingSpinner.Child = null;

            // Grab config from server that feeds into all this
            // ToDo: Config required for where this comes from!
            try
            {
                LoadingVisualEnabled(true);
                Status.Text = "Contacting server...";
                // Initial set up can happen in the background - it will enable relevant bits of interface when it completes
                Task.Run(() => LoadServerConfig("http://wiredcat.hopto.org/ArksplorerData.json"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was a problem during start up...{Environment.NewLine}{ex.Message}{(ex.InnerException == null ? "" : $" ({ex.InnerException.Message})")}{Environment.NewLine}Application will now exit.", "Start up error", MessageBoxButton.OK, MessageBoxImage.Error);
                ExitApplication();
            }
        }

        private void ExitApplication()
        {
            Application.Current.Shutdown();
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
            LoadableControlsEnabled(false);
            LoadingVisualEnabled(true);

            if (IncludeTheIsland.IsChecked ?? false)
                QueUpData("The Island", Types.TameMetadata, false);

            if (IncludeAberration.IsChecked ?? false)
                QueUpData("Aberration", Types.TameMetadata, false);

            if (IncludeRagnarok.IsChecked ?? false)
                QueUpData("Ragnarok", Types.TameMetadata, false);

            if (IncludeTheCenter.IsChecked ?? false)
                QueUpData("The Center", Types.TameMetadata, false);

            if (IncludeCrystalIsles.IsChecked ?? false)
                QueUpData("Crystal Isles", Types.TameMetadata, false);

            if (IncludeValguero.IsChecked ?? false)
                QueUpData("Valguero", Types.TameMetadata, false);

            if (IncludeScorchedEarth.IsChecked ?? false)
                QueUpData("Scorched Earth", Types.TameMetadata, false);

            if (IncludeExtinction.IsChecked ?? false)
                QueUpData("Extinction", Types.TameMetadata, false);

            if (IncludeGenesis.IsChecked ?? false)
                QueUpData("Genesis", Types.TameMetadata, false);

            LoadQueue<TameDino>();
        }


        private async void WildDinos_Click(object sender, RoutedEventArgs e)
        {
            LoadableControlsEnabled(false);
            LoadingVisualEnabled(true);

            if (IncludeTheIsland.IsChecked ?? false)
                QueUpData("The Island", Types.WildMetadata, false);

            if (IncludeAberration.IsChecked ?? false)
                QueUpData("Aberration", Types.WildMetadata, false);

            if (IncludeRagnarok.IsChecked ?? false)
                QueUpData("Ragnarok", Types.WildMetadata, false);

            if (IncludeTheCenter.IsChecked ?? false)
                QueUpData("The Center", Types.WildMetadata, false);

            if (IncludeCrystalIsles.IsChecked ?? false)
                QueUpData("Crystal Isles", Types.WildMetadata, false);

            if (IncludeValguero.IsChecked ?? false)
                QueUpData("Valguero", Types.WildMetadata, false);

            if (IncludeScorchedEarth.IsChecked ?? false)
                QueUpData("Scorched Earth", Types.WildMetadata, false);

            if (IncludeExtinction.IsChecked ?? false)
                QueUpData("Extinction", Types.WildMetadata, false);

            if (IncludeGenesis.IsChecked ?? false)
                QueUpData("Genesis", Types.WildMetadata, false);

            LoadQueue<WildDino>();
        }

        private void Survivors_Click(object sender, RoutedEventArgs e)
        {
            LoadableControlsEnabled(false);
            LoadingVisualEnabled(true);

            if (IncludeTheIsland.IsChecked ?? false)
                QueUpData("The Island", Types.SurvivorMetadata, false);

            if (IncludeAberration.IsChecked ?? false)
                QueUpData("Aberration", Types.SurvivorMetadata, false);

            if (IncludeRagnarok.IsChecked ?? false)
                QueUpData("Ragnarok", Types.SurvivorMetadata, false);

            if (IncludeTheCenter.IsChecked ?? false)
                QueUpData("The Center", Types.SurvivorMetadata, false);

            if (IncludeCrystalIsles.IsChecked ?? false)
                QueUpData("Crystal Isles", Types.SurvivorMetadata, false);

            if (IncludeValguero.IsChecked ?? false)
                QueUpData("Valguero", Types.SurvivorMetadata, false);

            if (IncludeScorchedEarth.IsChecked ?? false)
                QueUpData("Scorched Earth", Types.SurvivorMetadata, false);

            if (IncludeExtinction.IsChecked ?? false)
                QueUpData("Extinction", Types.SurvivorMetadata, false);

            if (IncludeGenesis.IsChecked ?? false)
                QueUpData("Genesis", Types.SurvivorMetadata, false);

            LoadQueue<Survivor>();
        }

        public BitmapImage LoadImage(string mapName, string message = "")
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

            if (string.IsNullOrWhiteSpace(message))
            {
                MapMessage.Visibility = Visibility.Collapsed;
            }
            else
            {
                MapMessage.Text = message;
                MapMessage.Visibility = Visibility.Visible;
            }

            return bitmap;
        }

        private void ApplyFilterCriteria(bool exactOnly = false)
        {
            PrevCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            string criteria = FilterCriteria.Text;

            FilterDataTable(CurrentDataPackage, criteria, exactOnly);

            Mouse.OverrideCursor = PrevCursor;
        }

        private void FilterDataTable(DataPackage dataPackage, string criteria, bool exactOnly)
        {
            if (dataPackage == null || string.IsNullOrWhiteSpace(criteria))
                return;

            string filter = exactOnly ? dataPackage.Metadata.NormalSearch : dataPackage.Metadata.WildcardSearch;

            if (string.IsNullOrWhiteSpace(filter))
                return;


            // If we have multiple components e.g. `rag, twog" then we do 2 searches!"
            string[] parts = criteria.Split(",");
            string finalFilter;

            if (parts.Length == 1)
            {
                string trimmed = parts[0].Trim();
                finalFilter = filter.Replace("#", trimmed);
            }
            else
            {
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
            }

            try
            {
                DataRow[] filteredRows = CurrentDataPackage.Data.Select(finalFilter);

                if (filteredRows.Length == 0)
                    DataVisual.DataContext = null;
                else
                    DataVisual.DataContext = filteredRows.CopyToDataTable();
            }
            catch (Exception ex)
            {
                Debug.Print($"Error: {ex.Message}");
            }
        }

        private void ClearFilter()
        {
            if (CurrentDataPackage.Data != null)
                DataVisual.DataContext = CurrentDataPackage.Data;
        }

        private static Dictionary<string, DataPackage> DataPackages { get; set; } = new Dictionary<string, DataPackage>();

        private static DataPackage CurrentDataPackage { get; set; }

        private static List<QueueDataItem> QueuedItems { get; set; } = new List<QueueDataItem>();

        private static async void QueUpData(string map, MetaData metaData, bool forceRefresh)
        {
            QueuedItems.Add(new QueueDataItem()
            {
                MapName = map,
                MetaData = metaData,
                DataUri = ServerConfig.GetUri($"{map}.{metaData.Type}"),
                TimestampUri = ServerConfig.GetUri($"{map}.timestamp"),
                ForceRefresh = forceRefresh
            });
        }

        Cursor PrevCursor { get; set; }

        // When a queue is loading, no others should load as controls that could trigger another load are disabled.
        private async void LoadQueue<T>()
        {
            PrevCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            int mapsLoaded = await Task.Run(() => ProcessQueue<T>(QueuedItems));
        }

        public void LoadableControlsEnabled(bool isEnabled)
        {
            TameDinos.IsEnabled = isEnabled;
            WildDinos.IsEnabled = isEnabled;
            Survivors.IsEnabled = isEnabled;
        }

        public void LoadingVisualEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                LoadingSpinner.Child = LoadingSpinnerStore;
                LoadingSpinner.Visibility = Visibility.Visible;
                ExtraInfoHolder.Visibility = Visibility.Hidden;
            }
            else
            {
                LoadingSpinner.Child = null;
                LoadingSpinner.Visibility = Visibility.Hidden;
                ExtraInfoHolder.Visibility = Visibility.Visible;
            }
        }

        private void ShowData(string type)
        {
            if (DataPackages.ContainsKey(type))
            {
                CurrentDataPackage = DataPackages[type];
                CurrentDataPackage.MakeSureDataIsUpToDate();
                DataVisual.DataContext = CurrentDataPackage.Data;

                Status.Text = $"Loaded {CurrentDataPackage.Data.Rows.Count} {CurrentDataPackage.Metadata.Description}s!";
                ExtraInfo.Text = CurrentDataPackage.MapsDescription;
            }
            else
            {
                // ToDo: Error
            }
        }

        static readonly HttpClient httpClient = new();

        private async void LoadServerConfig(string url)
        {
            string error = "";

            try
            {
                RawServerData rawServerData = await httpClient.GetFromJsonAsync<RawServerData>(new Uri(url));
                ServerConfig = new ServerConfig(rawServerData);

                Dispatcher.Invoke(() =>
                {
                    LoadableControlsEnabled(true);
                    Status.Text = $"Welcome! Remember, data is only{Environment.NewLine}as up - to - date as the server supplies";
                    LoadingVisualEnabled(false);
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


        private async Task<int> ProcessQueue<T>(List<QueueDataItem> queue)
        {
            int count = 0;
            string type = "";   // Should always be the same in a que
            int newRecords = 0;
            string description = "";
            int totalCount = queue.Count;
            int doneCount = 0;

            foreach (QueueDataItem item in queue)
            {
                type = item.MetaData.Type;
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
                    newRecords += await Task.Run(() => GetDataAsync<T>(item, rawServerTimestamp, serverTimestamp));
                }
                else
                {
                    Debug.Print($"Load of data skipped, already exists for {type}.{mapName} and is up to date");
                }

                count++;
            }

            queue.Clear();

            this.Dispatcher.Invoke(() =>
            {
                Status.Text = $"Loaded {newRecords} {description}s!";
                ShowData(type);
                LoadableControlsEnabled(true);
                LoadingVisualEnabled(false);
                Mouse.OverrideCursor = PrevCursor;
            });

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
        private static async Task<int> GetDataAsync<T>(QueueDataItem item, string rawServerTimestamp, DateTime serverTimestamp) // string map, string uri, MetaData metaData)
        {
            try
            {
                string type = item.MetaData.Type;
                DataPackage dataPackage;

                if (DataPackages.ContainsKey(type))
                {
                    dataPackage = DataPackages[type];
                }
                else
                {
                    dataPackage = new DataPackage()
                    {
                        Metadata = item.MetaData,
                        IndividualMaps = new Dictionary<string, MapPackage>()
                    };
                    DataPackages.Add(type, dataPackage);
                }

                string mapName = item.MapName;

                List<T> result = await httpClient.GetFromJsonAsync<List<T>>(item.DataUri);

                DataTable newData = result.ToNewDataTable(mapName);
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
                        MessageBox.Show($"Error reading timestamp for {mapName}.{type}, value returned: '{rawServerTimestamp}'", "Date error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    newMapPackage.Timestamp = serverTimestamp;
                }
                newMapPackage.RawTimestamp = rawServerTimestamp;

                var maps = dataPackage.IndividualMaps;
                if (maps.ContainsKey(mapName))
                    maps[mapName] = newMapPackage;
                else
                    maps.Add(mapName, newMapPackage);

                dataPackage.DataIsStale = true;

                return newData.Rows.Count;
            }
            catch (HttpRequestException ex)
            {
                Debug.Print($"Error: {ex.StatusCode}");
            }
            catch (NotSupportedException ex)
            {
                Debug.Print($"Invalid content type: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Debug.Print($"Invalid JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.Print($"Problem loading data: {ex.Message}");
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
                creature = (string)entity[columns["Creature"].Ordinal];
                overviewMessage += $"Creature: {creature}{Environment.NewLine}";
            }

            string name;
            if (columns["Name"] != null)
            {
                name = (string)entity[columns["Name"].Ordinal];
                if (string.IsNullOrWhiteSpace(name))
                    overviewMessage += $"Name not set{Environment.NewLine}";
                else
                    overviewMessage += $"Name: {name}{Environment.NewLine}";
            }

            string sex;
            if (columns["Sex"] != null)
            {
                sex = ((int)entity[columns["Sex"].Ordinal]) == 0 ? "M" : "F";
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
            IncludeTheIsland.IsChecked = isChecked;
            IncludeAberration.IsChecked = isChecked;
            IncludeRagnarok.IsChecked = isChecked;
            IncludeTheCenter.IsChecked = isChecked;
            IncludeCrystalIsles.IsChecked = isChecked;
            IncludeValguero.IsChecked = isChecked;
            IncludeScorchedEarth.IsChecked = isChecked;
            IncludeExtinction.IsChecked = isChecked;
            IncludeGenesis.IsChecked = isChecked;
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

        private void ShowExtraInfo(UIElement whatToShow)
        {
            ExtraInfoHolder.Visibility = Visibility.Collapsed;
            ResourcesAndLinksExtraInfo.Visibility = Visibility.Collapsed;
            AboutExtraInfo.Visibility = Visibility.Collapsed;

            whatToShow.Visibility = Visibility.Visible;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (MapMessage.Visibility != Visibility.Visible)
                return;

            var pos = e.GetPosition(this);
            var width = this.Width;
            var height = this.Height;

            var percX = (pos.X / width);
            var percY = (pos.Y / height);


            var brush = new SolidColorBrush(Colour.RGBFromHSL(percX, 1.0, 1.0 - percY));
            MapMessage.Foreground = brush;
        }

        private void HandleLinkClick(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
