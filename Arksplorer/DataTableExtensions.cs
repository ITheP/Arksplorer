using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Shapes;

namespace Arksplorer
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// Adds to an existing DataTable, or creates a new one if DataTable is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="map"></param>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static DataTable AddToDataTable(IEnumerable<IArkEntity> data, Type type, string mapName, MetaData metaData, DataTable dataTable = null)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(type);

            // Populate missing data
            // mapName is not part of source data, so we add that
            // If the dataset has a creature id in it, we keep and populate a looked up creature name too
            // Little bit of over-optimisation here
            if (metaData.IncludesCreatures)
            {
                IArkEntityWithCreature tmpCreature;

                foreach (var item in data)
                {
                    ((IArkEntity)item).Map = mapName;

                    tmpCreature = (IArkEntityWithCreature)item;
                    if (Lookup.Dinos.TryGetValue(tmpCreature.CreatureId, out DinoData dino))
                        tmpCreature.Creature = dino.ArksplorerName;
                }
            }
            else
            {
                foreach (var item in data)
                    ((IArkEntity)item).Map = mapName;
            }

            if (dataTable == null)
            {
                dataTable = new DataTable();

                // Add columns to DataTable, using our types properties for each column
                foreach (PropertyDescriptor property in properties)
                    dataTable.Columns.Add(property.Name, property.PropertyType);
            }

            int numColumns = properties.Count;
            object[] columns = new object[numColumns];

            foreach (var item in data)
            {
                for (int i = 0; i < numColumns; i++)
                    columns[i] = properties[i].GetValue(item);

                dataTable.Rows.Add(columns);
            }

            return dataTable;
        }
    }
}
