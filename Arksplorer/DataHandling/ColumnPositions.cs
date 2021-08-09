using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arksplorer
{
    public class ColumnPositions
    {
        public int LatColumn { get; set; }
        public int LonColumn { get; set; }
        public int CreatureIdColumn { get; set; }
        public int LvlColumn { get; set; }
        public int BaseColumn { get; set; }
        public int CreatureColumn { get; set; }
        public int NameColumn { get; set; }
        public int SexColumn { get; set; }
        public int CryoColumn { get; set; }
        public int C0Column { get; set; }
        public int C1Column { get; set; }
        public int C2Column { get; set; }
        public int C3Column { get; set; }
        public int C4Column { get; set; }
        public int C5Column { get; set; }
        public int CCCColumn { get; set; }
        public int HpColumn { get; set; }
        public int StamColumn { get; set; }
        public int MeleeColumn { get; set; }
        public int WeightColumn { get; set; }
        public int SpeedColumn { get; set; }
        public int FoodColumn { get; set; }
        public int OxyColumn { get; set; }
        public int CraftColumn { get; set; }
        public int TribeColumn { get; set; }
        public int TamerColumn { get; set; }
        public int ImprinterColumn { get; set; }
        public int ImprintColumn { get; set; }
        public int MutFColumn { get; set; }
        public int MutMColumn { get; set; }

        /// <summary>
        /// Column positions are all assumed to be dynamic (even if they are actually pretty static)
        /// This is to account for future expansion, new datasets, columns, definitions, changes etc.
        /// so we don't need to hardcode/recode values. Not quite as performant as hardcoding but does
        /// the job nicely.
        /// </summary>
        /// <param name="data"></param>
        public ColumnPositions(DataTable data)
        {
            //GlobalIndexColumn = data.Columns["GlobalIndex"]?.Ordinal ?? -1;
            // ...Always 0, set in Globals
            //MapColumn = data.Columns["Map"]?.Ordinal ?? -1;
            // ...Always 1, set in Globals
            //DataSourceColumn = data.Columns["DataSource"]?.Ordinal ?? -1;
            // ...Always 2, set in Globals

            LatColumn = data.Columns["Lat"]?.Ordinal ?? -1;
            LonColumn = data.Columns["Lon"]?.Ordinal ?? -1;
            CreatureIdColumn = data.Columns["CreatureId"]?.Ordinal ?? -1;
            LvlColumn = data.Columns["Lvl"]?.Ordinal ?? -1;
            BaseColumn = data.Columns["Base"]?.Ordinal ?? -1;
            CreatureColumn = data.Columns["Creature"]?.Ordinal ?? -1;
            NameColumn = data.Columns["Name"]?.Ordinal ?? -1;
            SexColumn = data.Columns["Sex"]?.Ordinal ?? -1;
            CryoColumn = data.Columns["Cryo"]?.Ordinal ?? -1;
            C0Column = data.Columns["C0"]?.Ordinal ?? -1;
            C1Column = data.Columns["C1"]?.Ordinal ?? -1;
            C2Column = data.Columns["C2"]?.Ordinal ?? -1;
            C3Column = data.Columns["C3"]?.Ordinal ?? -1;
            C4Column = data.Columns["C4"]?.Ordinal ?? -1;
            C5Column = data.Columns["C5"]?.Ordinal ?? -1;
            // Ark has some W variants of columns in wild vs tame. We just re-use below for either as we dont care about the name, equivalents
            HpColumn = data.Columns["Hp"]?.Ordinal ?? data.Columns["HpW"]?.Ordinal ?? -1;
            StamColumn = data.Columns["Stam"]?.Ordinal ?? data.Columns["StamW"]?.Ordinal ?? -1;
            MeleeColumn = data.Columns["Melee"]?.Ordinal ?? data.Columns["MeleeW"]?.Ordinal ?? -1;
            WeightColumn = data.Columns["Weight"]?.Ordinal ?? data.Columns["WeightW"]?.Ordinal ?? -1;
            SpeedColumn = data.Columns["Speed"]?.Ordinal ?? data.Columns["SpeedW"]?.Ordinal ?? -1;
            FoodColumn = data.Columns["Food"]?.Ordinal ?? data.Columns["FoodW"]?.Ordinal ?? -1;
            OxyColumn = data.Columns["Oxy"]?.Ordinal ?? data.Columns["OxyW"]?.Ordinal ?? -1;
            CraftColumn = data.Columns["Craft"]?.Ordinal ?? data.Columns["CraftT"]?.Ordinal ?? -1;
            TribeColumn = data.Columns["Tribe"]?.Ordinal ?? -1;
            TamerColumn = data.Columns["Tamer"]?.Ordinal ?? -1;
            ImprinterColumn = data.Columns["Imprinter"]?.Ordinal ?? -1;
            ImprintColumn = data.Columns["Imprint"]?.Ordinal ?? -1;
            MutFColumn = data.Columns["MutF"]?.Ordinal ?? -1;
            MutMColumn = data.Columns["MutM"]?.Ordinal ?? -1;
            CCCColumn = data.Columns["CCC"]?.Ordinal ?? -1;
        }
    }
}
