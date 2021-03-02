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
        public MainWindow()
        {
            InitializeComponent();
            Marker.Visibility = Visibility.Collapsed;

            // We drag the loading effect out the page when not visible,
            // so it's not being calculated while hidden/collapsed (have seen that happen before)
            LoadingSpinnerStore = LoadingSpinner.Child;
            LoadingSpinner.Child = null;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            ShowExtraInfo(AboutExtra);
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
            LoadableControlEnabled(false);
            LoadingVisualEnabled(true);

            if (IncludeTheIsland.IsChecked ?? false)
                QueUpData("The Island", TameMetadata, "http://wiredcat.hopto.org/WiredcatIsland/", "Dinos.json", "timestamp.json", false);

            if (IncludeAberration.IsChecked ?? false)
                QueUpData("Aberration", TameMetadata, "http://wiredcat.hopto.org/WiredcatAberration/", "Dinos.json", "timestamp.json", false);

            if (IncludeRagnarok.IsChecked ?? false)
                QueUpData("Ragnarok", TameMetadata, "http://wiredcat.hopto.org/WiredcatRagnarok/", "Dinos.json", "timestamp.json", false);

            if (IncludeTheCenter.IsChecked ?? false)
                QueUpData("The Center", TameMetadata, "http://wiredcat.hopto.org/WiredcatCenter/", "Dinos.json", "timestamp.json", false);

            if (IncludeCrystalIsles.IsChecked ?? false)
                QueUpData("Crystal Isles", TameMetadata, "http://wiredcat.hopto.org/WiredcatCrystal/", "Dinos.json", "timestamp.json", false);

            if (IncludeValguero.IsChecked ?? false)
                QueUpData("Valguero", TameMetadata, "http://wiredcat.hopto.org/WiredcatValguero/", "Dinos.json", "timestamp.json", false);

            if (IncludeScorchedEarth.IsChecked ?? false)
                QueUpData("Scorched Earth", TameMetadata, "http://wiredcat.hopto.org/WiredcatScorchedEarth/", "Dinos.json", "timestamp.json", false);

            if (IncludeExtinction.IsChecked ?? false)
                QueUpData("Extinction", TameMetadata, "http://wiredcat.hopto.org/WiredcatExtinction/", "Dinos.json", "timestamp.json", false);

            if (IncludeGenesis.IsChecked ?? false)
                QueUpData("Genesis", TameMetadata, "http://wiredcat.hopto.org/WiredcatGenesis/", "Dinos.json", "timestamp.json", false);

            LoadQueue<TameDino>();
        }


        private async void WildDinos_Click(object sender, RoutedEventArgs e)
        {
            LoadableControlEnabled(false);
            LoadingVisualEnabled(true);

            if (IncludeTheIsland.IsChecked ?? false)
                QueUpData("The Island", WildMetadata, "http://wiredcat.hopto.org/WiredcatIsland/", "Wild.json", "timestamp.json", false);

            if (IncludeAberration.IsChecked ?? false)
                QueUpData("Aberration", WildMetadata, "http://wiredcat.hopto.org/WiredcatAberration", "/Wild.json", "timestamp.json", false);

            if (IncludeRagnarok.IsChecked ?? false)
                QueUpData("Ragnarok", WildMetadata, "http://wiredcat.hopto.org/WiredcatRagnarok/", "Wild.json", "timestamp.json", false);

            if (IncludeTheCenter.IsChecked ?? false)
                QueUpData("The Center", WildMetadata, "http://wiredcat.hopto.org/WiredcatCenter/", "Wild.json", "timestamp.json", false);

            if (IncludeCrystalIsles.IsChecked ?? false)
                QueUpData("Crystal Isles", WildMetadata, "http://wiredcat.hopto.org/WiredcatCrystal/", "Wild.json", "timestamp.json", false);

            if (IncludeValguero.IsChecked ?? false)
                QueUpData("Valguero", WildMetadata, "http://wiredcat.hopto.org/WiredcatValguero/", "Wild.json", "timestamp.json", false);

            if (IncludeScorchedEarth.IsChecked ?? false)
                QueUpData("Scorched Earth", WildMetadata, "http://wiredcat.hopto.org/WiredcatScorchedEarth/", "Wild.json", "timestamp.json", false);

            if (IncludeExtinction.IsChecked ?? false)
                QueUpData("Extinction", WildMetadata, "http://wiredcat.hopto.org/WiredcatExtinction/", "Wild.json", "timestamp.json", false);

            if (IncludeGenesis.IsChecked ?? false)
                QueUpData("Genesis", WildMetadata, "http://wiredcat.hopto.org/WiredcatGenesis/", "Wild.json", "timestamp.json", false);

            LoadQueue<WildDino>();
        }

        private void Survivors_Click(object sender, RoutedEventArgs e)
        {
            LoadableControlEnabled(false);
            LoadingVisualEnabled(true);

            if (IncludeTheIsland.IsChecked ?? false)
                QueUpData("The Island", SurvivorMetadata, "http://wiredcat.hopto.org/WiredcatIsland/", "Survivors.json", "timestamp.json", false);

            if (IncludeAberration.IsChecked ?? false)
                QueUpData("Aberration", SurvivorMetadata, "http://wiredcat.hopto.org/WiredcatAberration/", "Survivors.json", "timestamp.json", false);

            if (IncludeRagnarok.IsChecked ?? false)
                QueUpData("Ragnarok", SurvivorMetadata, "http://wiredcat.hopto.org/WiredcatRagnarok/", "Survivors.json", "timestamp.json", false);

            if (IncludeTheCenter.IsChecked ?? false)
                QueUpData("The Center", SurvivorMetadata, "http://wiredcat.hopto.org/WiredcatCenter/", "Survivors.json", "timestamp.json", false);

            if (IncludeCrystalIsles.IsChecked ?? false)
                QueUpData("Crystal Isles", SurvivorMetadata, "http://wiredcat.hopto.org/WiredcatCrystal/", "Survivors.json", "timestamp.json", false);

            if (IncludeValguero.IsChecked ?? false)
                QueUpData("Valguero", SurvivorMetadata, "http://wiredcat.hopto.org/WiredcatValguero/", "Survivors.json", "timestamp.json", false);

            if (IncludeScorchedEarth.IsChecked ?? false)
                QueUpData("Scorched Earth", SurvivorMetadata, "http://wiredcat.hopto.org/WiredcatScorchedEarth/", "Survivors.json", "timestamp.json", false);

            if (IncludeExtinction.IsChecked ?? false)
                QueUpData("Extinction", SurvivorMetadata, "http://wiredcat.hopto.org/WiredcatExtinction/", "Survivors.json", "timestamp.json", false);

            if (IncludeGenesis.IsChecked ?? false)
                QueUpData("Genesis", SurvivorMetadata, "http://wiredcat.hopto.org/WiredcatGenesis/", "Survivors.json", "timestamp.json", false);

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

            string filter = exactOnly ? dataPackage.Metadata.NormalSearch : dataPackage.Metadata.WildcardSearch;  //criteria.Contains("*") ? dataPackage.SearchType.WildcardSearch : dataPackage.SearchType.NormalSearch;

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
            // FilterColumn.Text = null;
            // FilterCriteria.Text = null;

            if (CurrentDataPackage.Data != null)
                DataVisual.DataContext = CurrentDataPackage.Data;
        }

        private static Dictionary<string, DataPackage> DataPackages { get; set; } = new Dictionary<string, DataPackage>();

        private static DataPackage CurrentDataPackage { get; set; }

        private static List<QueueDataItem> QueuedItems { get; set; } = new List<QueueDataItem>();

        // Highest level - show data if already loaded, or load if not (which will then show later in time)
        private static async void QueUpData(string map, MetaData metaData, string baseUri, string dataFilename, string timestampFilename, bool forceRefresh)
        {
            QueuedItems.Add(new QueueDataItem()
            {
                MapName = map,
                MetaData = metaData,
                DataUri = baseUri + dataFilename,
                TimestampUri = baseUri + timestampFilename,
                ForceRefresh = forceRefresh
            });
        }

        Cursor PrevCursor { get; set; }

        private async void LoadQueue<T>()
        {
            // ToDo: STOP LOADING A QUE IF ONES ALREADY LOADING
            PrevCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            int mapsLoaded = await Task.Run(() => ProcessQueue<T>(QueuedItems));
        }

        public void LoadableControlEnabled(bool isEnabled)
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
                ExtraInfo.Visibility = Visibility.Hidden;
            }
            else
            {
                LoadingSpinner.Child = null;
                LoadingSpinner.Visibility = Visibility.Hidden;
                ExtraInfo.Visibility = Visibility.Visible;
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

        // ToDo: We may repeatidly re-read timestamps when not required. We should be able to cache these for speed!

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
                    //Debug.Print($"{doneCount}/{totalCount} Loading data for {type}.{mapName} [{rawServerTimestamp}]");
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
                LoadableControlEnabled(true);
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

            // Map is always in column 0
            string mapName = (string)entity[0];
            if (mapName != CurrentMapImage)
                MapImage.Source = LoadImage(mapName);

            SetOverviewMessage(overviewMessage);
        }

        private void SetOverviewMessage(string message)
        {
            OverviewMessage.Text = message;
        }

        // Really want to tag static variants of these into the class definitions for ArkEntities, but, no luck so far (without them becoming duplicating strings)
        public static MetaData TameMetadata = new()
        {
            Type = "Tame",
            Description = "Tamed Dino",
            NormalSearch = "Map='#' OR Tribe='#' OR Tamer='#' OR Imprinter='#' OR Creature='#' OR Name='#'",
            WildcardSearch = "Map LIKE '*#*' OR Tribe LIKE '*#*' OR Tamer LIKE '*#*' OR Imprinter LIKE '*#*' OR Creature LIKE '*#*' OR Name LIKE '*#*'"
        };

        public static MetaData WildMetadata = new()
        {
            Type = "Wild",
            Description = "Wild Dino",
            NormalSearch = "Map='#' OR Creature='#'",
            WildcardSearch = "Map LIKE '*#*' AND Creature LIKE '*#*'"
        };

        public static MetaData SurvivorMetadata = new()
        {
            Type = "Survivor",
            Description = "Survivor",
            NormalSearch = "Map='#' OR Steam='#' OR Name='#' OR Tribe='#'",
            WildcardSearch = "Map LIKE '*#*' Steam LIKE '*#*' OR Name LIKE '*#*' OR Tribe LIKE '*#*'",
        };

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
            ExtraInfo.Visibility = Visibility.Collapsed;
            ResourcesAndLinksExtraInfo.Visibility = Visibility.Collapsed;
            AboutExtra.Visibility = Visibility.Collapsed;

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

            //Debug.Print($"{percX},{percY}");
            // X = R
            // Y = G
            var brush = new SolidColorBrush(Colour.RGBFromHSL(percX, 1.0, 1.0 - percY));
            MapMessage.Foreground = brush;
            //OverviewMessage.Foreground = brush;
        }
    }
}
