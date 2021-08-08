using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Shell;

namespace Arksplorer
{
    //public class DataRow : DataRow
    //{
    //    new DataTablePlus Table { get; set; }

    //    public DataRow(DataRowBuilder builder) : base(builder)
    //    {

    //    }
    //}

    // Note: Did try extending DataTable + DataRows so ColumnPositions could be stored directly on a row.DataTable.ColumnPositions basis, save a lot of faffing around. However
    // there were problems, especially when it came to DataRowViews and casting back to get at ColumnPositions. After not working, research seems to show
    // that it can't work that way (due to protection levels of system classes). So to get a reference from a row back to what the column definitions are,
    // we just set another system value on each row that points back to a DataTablePlus, which includes ColumnPositions.
    // Reminder that when doing things like spawning Popups from mouseover of row data, we don't know what type of row this is to get relevant ColumnPositions, without storing a reference somewhere to it via the row.
    // This is what this faffing around is for.
    // Might try again another day!


    public class DataTablePlus : DataTable //Extensions
    {
        /// <summary>
        /// Direct mappings for specific column indexes to certain types of Ark Data (quicker to use these than re-looking up column indexes all the time)
        /// </summary>
        public ColumnPositions ColumnPositions { get; set; }

        public DataTablePlus DeepCopy()
        {
            // Want a clone of base DataTable, but can copy reference of ColumnPositions
            DataTablePlus dataTablePlus = (DataTablePlus)base.Clone();
            dataTablePlus.ColumnPositions = this.ColumnPositions;

            return dataTablePlus;
        }

        public DataTablePlus()
        {
            // Required for .Clone base mthod to work
        }

        /// <summary>
        /// Adds to an existing DataTable, or creates a new one if DataTable is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="map"></param>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public DataTablePlus(IEnumerable<IArkEntity> arkData, Type type, string mapName, MetaData metaData)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(type);

            // Populate missing data
            // mapName is not part of source data, so we add that
            // If the dataset has a creature id in it, we keep and populate a looked up creature name too
            // Little bit of over-optimisation here
            if (metaData.IncludesCreatures)
            {
                IArkEntityWithCreature tmpCreature;

                foreach (var item in arkData)
                {
                    item.GlobalIndex = Globals.GlobalIndex++;
                    item.Map = mapName;
                    item.DataParent = this;

                    tmpCreature = (IArkEntityWithCreature)item;
                    if (Lookup.Dinos.TryGetValue(tmpCreature.CreatureId, out DinoData dino))
                        tmpCreature.Creature = dino.ArksplorerName;
                }
            }
            else
            {
                foreach (var item in arkData)
                {
                    item.GlobalIndex = Globals.GlobalIndex++;
                    item.Map = mapName;
                    item.DataParent = this;
                }
            }

            // Note references to DataDefinition won't be populated yet, comes later after this data table is populated

            // Add columns to DataTable, using our types properties for each column
            foreach (PropertyDescriptor property in properties)
                this.Columns.Add(property.Name, property.PropertyType);

            this.ColumnPositions = new ColumnPositions(this);

            //foreach (var item in arkData)
            //    ((IArkEntity)item).Data = data;

            int numColumns = properties.Count;
            object[] columns = new object[numColumns];

            foreach (var item in arkData)
            {
                for (int i = 0; i < numColumns; i++)
                    columns[i] = properties[i].GetValue(item);

                this.Rows.Add(columns);
            }
        }
    }
}
