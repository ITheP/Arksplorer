using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Arksplorer
{
    public class DataPackage
    {
        public DataTable Data { get; set; }
        public MetaData Metadata { get; set; }
        public Dictionary<string, MapPackage> IndividualMaps { get; set; }
        public string MapsDescription { get; set; }
        public bool DataIsStale { get; set; }

        // Goes through each individual map and generates a new master set of data by merging them together
        public void MakeSureDataIsUpToDate()
        {
            if (!DataIsStale)
                return;

            if (IndividualMaps.Count == 0)
            {
                MapsDescription = "No maps loaded";
                return;
            }

            bool success = false;
            int attempts = 1;

            while (!success)
            {
                // it's possible for the data to be modified while we are processing it (e.g. background process is re-generating it)
                // which will throw an exception here. So we catch and handle this by trying again.
                try
                {
                    Data = new DataTable();

                    // Set up copy of datatable structure only (no data)
                    Data = IndividualMaps.First().Value.Data.Clone();

                    MapsDescription = $"Showing data for {Metadata.Description}s";
                    Debug.Print($"Map data for {Metadata.Description}s is made up of...");

                    foreach (KeyValuePair<string, MapPackage> map in IndividualMaps)
                    {
                        MapPackage mapPackage = map.Value;
                        Debug.Print($"   {Metadata.ArkEntityType}.{map.Key} - {mapPackage.Data.Rows.Count}");
                        Data.Merge(mapPackage.Data);
                    }

                    success = true;
                }
                catch (Exception ex)
                {
                    Debug.Print($"Error generating map data - restarting");
                    attempts++;

                    Thread.Sleep(attempts * 100);

                    if (attempts > 5)
                    {
                        Errors.ReportProblem(ex, "Internal problem updating data. Map data might not be showing correctly. Will keep going though! Feel free to try again...");

                        success = true;
                    }
                }
            }

            Debug.Print($"...{Data.Rows.Count} total (took {attempts} attempts)");

            DataIsStale = false;
        }
    }
}
