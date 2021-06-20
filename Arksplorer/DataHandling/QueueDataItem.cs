using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Arksplorer
{
    public class QueueDataItem
    {
        public string MapName { get; set; }
        public MapSelection UIMapSelection { get; set; }
        public MetaData MetaData { get; set; }
        public Uri DataUri { get; set; }
        public Uri TimestampUri { get; set; }
        public bool ForceRefresh { get; set; }

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
        public async Task<int> GetDataAsync(string rawServerTimestamp, DateTime serverTimestamp, Dictionary<string, DataPackage> dataPackages, ServerConfig serverConfig, MainWindow mainWindow, bool forceLocalLoad)
        {
            // Note we DON'T do any parallel loading of data from the server to help cut down on parallel server traffic. Lovely that we might
            // download data for all maps at once - unless its eaten all the servers bandwidth and players are affected!

            try
            {
                string metaDataType = MetaData.ArkEntityType;
                DataPackage dataPackage;

                if (dataPackages.ContainsKey(metaDataType))
                {
                    dataPackage = dataPackages[metaDataType];
                    UIMapSelection.CacheState = "Updating existing data package";
                }
                else
                {
                    dataPackage = new DataPackage()
                    {
                        Metadata = MetaData,
                        IndividualMaps = new Dictionary<string, MapPackage>()
                    };
                    dataPackages.Add(metaDataType, dataPackage);

                    UIMapSelection.CacheState = "New data package";
                }

                string mapName = MapName;
                UIMapSelection.DisplayState = "...Downloading";
                mainWindow.Dispatcher.Invoke(() => Globals.MainWindow.MapsToInclude.Items.Refresh());

                IEnumerable<IArkEntity> result;
#if DEBUG
                // Local debugging test - load data from local file
                if (forceLocalLoad)
                    result = (IEnumerable<IArkEntity>)JsonSerializer.Deserialize(File.ReadAllText("./temp.json"), typeof(List<>).MakeGenericType(MetaData.JsonClassType));
                else
#endif
                    result = (IEnumerable<IArkEntity>)await Web.HttpClient.GetFromJsonAsync(DataUri, typeof(List<>).MakeGenericType(MetaData.JsonClassType));

                // Extra processing of data goes here - e.g. calculating extra data values not in JSON and not part of JSON import translations
                // E.g. colour sort data, as we can't easily read single json colour values into 2 properties at once during import
                if (typeof(IArkEntityWithCreature).IsAssignableFrom(result.GetType().GetGenericArguments()[0]))
                {
                    // It's a creature! it has colours :)
                    foreach (IArkEntityWithCreature creature in result)
                    {
                        creature.C0_Sort = creature.C0?.SortOrder ?? -1;   // Colour.SortKeyFromColor(creature.C0);
                        creature.C1_Sort = creature.C1?.SortOrder ?? -1;
                        creature.C2_Sort = creature.C2?.SortOrder ?? -1;
                        creature.C3_Sort = creature.C3?.SortOrder ?? -1;
                        creature.C4_Sort = creature.C4?.SortOrder ?? -1;
                        creature.C5_Sort = creature.C5?.SortOrder ?? -1;
                    }
                }

                UIMapSelection.DisplayState = "...Decoding";
                mainWindow.Dispatcher.Invoke(() => Globals.MainWindow.MapsToInclude.Items.Refresh());

                DataTable newData = DataTableExtensions.AddToDataTable(result, MetaData.JsonClassType, mapName, MetaData, null);
                MapPackage newMapPackage = new();
                newMapPackage.Data = newData;

                UIMapSelection.DisplayState = "Loaded";
                mainWindow.Dispatcher.Invoke(() => Globals.MainWindow.MapsToInclude.Items.Refresh());

                // Only calculate timestamps if they haven't already just been snagged
                if (rawServerTimestamp == string.Empty)
                {
                    DataTimestamp jsonTimestamp = await Web.HttpClient.GetFromJsonAsync<DataTimestamp>(TimestampUri);
                    rawServerTimestamp = jsonTimestamp.Date;
                    if (DateTime.TryParseExact(rawServerTimestamp, "yyyyMMdd_HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out serverTimestamp))
                    {
                        Debug.Print($"GetDataAsync updating from web site into newMapPackage.Timestamp from {newMapPackage.Timestamp} to {serverTimestamp} : {mapName}.{metaDataType}");
                        newMapPackage.Timestamp = serverTimestamp;
                    }
                    else
                    {
                        MessageBox.Show($"Error reading timestamp for {mapName}.{metaDataType}, value returned: '{rawServerTimestamp}'", "Date error", MessageBoxButton.OK, MessageBoxImage.Error);
                        UIMapSelection.CacheState = "Timestamp error";
                        mainWindow.Dispatcher.Invoke(() => Globals.MainWindow.MapsToInclude.Items.Refresh());
                    }
                }
                else
                {
                    Debug.Print($"GetDataAsync updating from cached timestamp into newMapPackage.Timestamp from {newMapPackage.Timestamp} to {serverTimestamp} : {mapName}.{metaDataType}");
                    newMapPackage.Timestamp = serverTimestamp;
                }
                newMapPackage.RawTimestamp = rawServerTimestamp;
                newMapPackage.ApproxNextServerUpdateTimestamp = newMapPackage.Timestamp.AddMinutes(serverConfig.RefreshRate);

                var maps = dataPackage.IndividualMaps;
                if (maps.ContainsKey(mapName))
                    maps[mapName] = newMapPackage;
                else
                    maps.Add(mapName, newMapPackage);

                Debug.Print($"GetDataAsync CurrentDataPackage == dataPackage {mainWindow.CurrentDataPackage == dataPackage} to trigger visual refresh");

                mainWindow.Dispatcher.Invoke(() => { mainWindow.MapData.ItemsSource = mainWindow.CurrentDataPackage?.IndividualMaps.ToList(); }); //THIS should work but doesnt visually --> if (CurrentDataPackage == dataPackage) ExtraInfoMapData.Items.Refresh(); });

                dataPackage.DataIsStale = true;

                return newData.Rows.Count;
            }
            catch (HttpRequestException ex)
            {
                Errors.ReportProblem(ex, $"Error retrieving JSON data for { MetaData.Description}: {ex.StatusCode}");
            }
            catch (NotSupportedException ex)
            {
                Errors.ReportProblem(ex, $"Invalid content type in JSON data for { MetaData.Description}");
            }
            catch (JsonException ex)
            {
                Errors.ReportProblem(ex, $"Invalid JSON retrieving JSON data for { MetaData.Description}");
            }
            catch (Exception ex)
            {
                Errors.ReportProblem(ex, $"Problem loading JSON data for { MetaData.Description}");
            }

            return -1;
        }
    }
}
