using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Windows.Shapes;

namespace Arksplorer
{
    public static class Extensions
    {
        public static DataTable ToNewDataTable<T>(this List<T> data, string mapName)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            var dataTable = new DataTable();

            dataTable.Columns.Add("Map", typeof(string));

            // Add columns to DataTable, using our types properties for each column
            foreach (PropertyDescriptor property in properties)
                dataTable.Columns.Add(property.Name, property.PropertyType);

            int numColumns = properties.Count;
            object[] columns = new object[numColumns + 1];

            foreach (T item in data)
            {
                columns[0] = mapName;

                for (int i = 0; i < numColumns; i++)
                    columns[i + 1] = properties[i].GetValue(item);

                dataTable.Rows.Add(columns);
            }

            return dataTable;
        }

        /// <summary>
        /// Adds to an existing DataTable, or creates a new one if DataTable is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="map"></param>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static DataTable AddToDataTable<T>(this List<T> data, string map, DataTable dataTable = null)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            if (dataTable == null)
            {
                dataTable = new DataTable();

                dataTable.Columns.Add("Map", typeof(string));

                // Add columns to DataTable, using our types properties for each column
                foreach (PropertyDescriptor property in properties)
                    dataTable.Columns.Add(property.Name, property.PropertyType);
            }

            int numColumns = properties.Count;
            object[] columns = new object[numColumns + 1];

            foreach (T item in data)
            {
                columns[0] = map;

                for (int i = 0; i < numColumns; i++)
                    columns[i + 1] = properties[i].GetValue(item);

                dataTable.Rows.Add(columns);
            }

            return dataTable;
        }
    }
}
