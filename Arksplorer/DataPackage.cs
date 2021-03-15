using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
//using System.Windows.Shapes;

namespace Arksplorer
{
    public class DataPackage
    {
        public DataTable Data { get; set; }
        public MetaData Metadata { get; set; }
        public Dictionary<string, MapPackage> IndividualMaps { get; set; }
        public string MapsDescription { get; set; }
        public bool DataIsStale { get; set; }

        // Goes through each individual map and generates a new master set of data
        public void MakeSureDataIsUpToDate()
        {
            if (!DataIsStale)
                return;

            Data = new DataTable();

            if (IndividualMaps.Count == 0)
            {
                MapsDescription = "No maps loaded";
                return;
            }

            // Set up copy of datatable structure only (no data)
            Data = IndividualMaps.First().Value.Data.Clone();
            MapsDescription = string.Empty;
            Debug.Print("Map data is made up of...");

            foreach (KeyValuePair<string, MapPackage> map in IndividualMaps)
            {
                MapPackage mapPackage = map.Value;
                MapsDescription += $"{map.Key} ({mapPackage.Data.Rows.Count}) [{mapPackage.Timestamp:HH:mm}]{Environment.NewLine}";
                Debug.Print($"   {Metadata.ArkEntityType}.{map.Key} - {mapPackage.Data.Rows.Count}");
                Data.Merge(mapPackage.Data);
            }

            MapsDescription = $"Showing {Metadata.Description}s ({Data.Rows.Count} total){Environment.NewLine}{MapsDescription}";
            Debug.Print($"...{Data.Rows.Count} total");
            DataIsStale = false;
        }
    }
}
