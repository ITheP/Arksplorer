using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Arksplorer
{
    public class Queue
    {
        public List<QueueDataItem> Items { get; set; } = new();

        public void AddNewItem(string map, MetaData metaData, bool forceRefresh, ServerConfig serverConfig)
        {
            Items.Add(new QueueDataItem()
            {
                MapName = map,
                MetaData = metaData,
                DataUri = serverConfig.GetUri($"{map}.{metaData.ArkEntityType}"),
                TimestampUri = serverConfig.GetUri($"{map}.timestamp"),
                ForceRefresh = forceRefresh
            });
        }

        public async Task<int> Process(bool autoUpdateVisualDataGrid, Dictionary<string, DataPackage> dataPacakges, MainWindow mainWindow, ServerConfig serverConfig, bool forceLocalLoad)
        {
            int count = 0;
            string type = "";
            int newRecords = 0;
            string description = "";
            int totalCount = Items.Count;
            int doneCount = 0;

            try
            {

                foreach (QueueDataItem item in Items)
                {
                    type = item.MetaData.ArkEntityType;
                    string mapName = item.MapName;
                    description = item.MetaData.Description;
                    bool grabData = false;
                    string rawServerTimestamp = string.Empty;
                    DateTime serverTimestamp = DateTime.MinValue;

                    bool dataExists = (dataPacakges.ContainsKey(type) && dataPacakges[type].IndividualMaps.ContainsKey(mapName));

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
                            MapPackage mapPackage = dataPacakges[type].IndividualMaps[mapName];
                            DataTimestamp jsonTimestamp = await Web.HttpClient.GetFromJsonAsync<DataTimestamp>(item.TimestampUri);
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
                        mainWindow.Dispatcher.Invoke(() => mainWindow.Status.Text = $"Loading {++doneCount}/{totalCount}{Environment.NewLine}{mapName} {type} data...");
                        // We re-use the timestamp from earlier, if its available. Saves a server trip.
                        newRecords += await Task.Run(() => item.GetDataAsync(rawServerTimestamp, serverTimestamp, dataPacakges, serverConfig, mainWindow, forceLocalLoad));
                    }
                    else
                    {
                        Debug.Print($"Load of data skipped, already exists for {type}.{mapName} and is up to date");
                        Debug.Print($"{type}.{mapName}.{serverTimestamp} Already using latest available data, loading skipped");
                    }
                }

                mainWindow.Dispatcher.Invoke(() =>
                {
                    Debug.Print($"ProcessQue with done count {doneCount}");
                    if (doneCount > 0)
                    {
                        if (newRecords > 0)
                            mainWindow.Status.Text = $"Loaded {newRecords} {description}s!";
                        else
                            mainWindow.Status.Text = "Select maps to load first";

                    }
                    else
                    {
                        mainWindow.Status.Text = "Nothing new to load";
                    }

                    // Even if nothing was loaded, if it's a different type of dino, we still want to `show things` - so always update here
                    mainWindow.ShowData(type, autoUpdateVisualDataGrid);
                    //   Mouse.OverrideCursor = PrevCursor;
                });

                //ProcessingQueue = false;
            }
            catch (Exception ex)
            {
                
                mainWindow.Dispatcher.Invoke(() =>
                {
                    mainWindow.Status.Text = $"Error loading data!";

                    string serverName = Globals.MainWindow?.ServerConfig == null ? "" : $" {Globals.MainWindow.ServerConfig.Description}";

                    Errors.ReportProblem(ex, $"There was an error trying to fetch data from the {serverName}server. " +
                        $"Could be a network or server problem. You can always try again in a minute. " +
                        $"If thing's still don't work, you might just have to sit it out for a while.");
                });

                count = 0;
            }


            mainWindow.Dispatcher.Invoke(() =>
            {
                mainWindow.LoadableControlsEnabled(true);
                mainWindow.LoadingVisualEnabled(false);
            });

            return count;
        }
    }
}
