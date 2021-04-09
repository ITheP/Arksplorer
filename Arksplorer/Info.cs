using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Arksplorer
{
    /// <summary>
    /// General purpose info for showing basic description/value data plus any icons
    /// </summary>
    public class Info
    {
        // General purpose `stick whatever you want in` lists
        public List<ListInfoItem> Items { get; } = new();
        public List<BitmapImage> Icons { get; } = new();

        // Specifics (usually with custom visualisation)
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string Creature { get; set; }
        public string CreatureId { get; set; }
        public int Level { get; set; } = -1;
        public int BaseLevel { get; set; } = -1;
        public string Tribe { get; set; }
        public string Sex { get; set; }
        public bool Cryoed { get; set; }
        public ArkColor C0 { get; set; }
        public ArkColor C1 { get; set; }
        public ArkColor C2 { get; set; }
        public ArkColor C3 { get; set; }
        public ArkColor C4 { get; set; }
        public ArkColor C5 { get; set; }

        public void Add(string description, string value = null)
        {
            Items.Add(new ListInfoItem() { Description = description, Value = value });
        }

        public void AddIcon(BitmapImage image)
        {
            Icons.Add(image);
        }

        public Info(DataRow row)
        {
            Lat = (float)row[LatColumn];
            Lon = (float)row[LonColumn];

            if (NameColumn > -1)
                Name = row[NameColumn] as string;

            if (CreatureColumn > -1)
                Creature = row[CreatureColumn] as string;

            if (CreatureIdColumn > -1)
                CreatureId = row[CreatureIdColumn] as string;

            if (SexColumn > -1)
            {
                BitmapImage sex = row[SexColumn] as BitmapImage;
                Sex = (sex == IconImages.Male ? "M" : "F");
                AddIcon(sex);
            }

            if (BaseColumn > -1)
            {
                int baseLevel = (row[BaseColumn] as int? ?? -1);
                if (baseLevel > 0)
                {
                    BaseLevel = baseLevel;
                    Add("Base level", $"{row[BaseColumn] as int?}");
                }
            }

            int level = (int)row[LvlColumn];
            Add("Level", $"{level}");
            Level = level;

            if (HpColumn > -1)
                Add("Health", $"{row[WeightColumn]}");
            if (StamColumn > -1)
                Add("Stamina", $"{row[StamColumn]}");
            if (MeleeColumn > -1)
                Add("Melee", $"{row[MeleeColumn]}");
            if (WeightColumn > -1)
                Add("Weight", $"{row[WeightColumn]}");
            if (SpeedColumn > -1)
                Add("Speed", $"{row[SpeedColumn]}");
            if (FoodColumn > -1)
                Add("Food", $"{row[FoodColumn]}");
            if (OxyColumn > -1)
                Add("Oxygen", $"{row[OxyColumn]}");
            if (CraftColumn > -1)
                Add("Crafting", $"{row[CraftColumn]}");
            if (StamColumn > -1)
                Add("Stamina", $"{row[StamColumn]}");

            if (TamerColumn > -1)
            {
                string tribe = $"{row[TamerColumn] as string}";
                Add("Tamer (Tribe)", tribe);
                Tribe = tribe;
            }
            if (ImprinterColumn > -1)
                Add("Imprinter", $"{row[ImprinterColumn] as string}");
            if (ImprintColumn > -1)
                Add("Imprint", $"{row[ImprintColumn]}");

            if (MutFColumn > -1)
                Add("Mutations F", $"{row[MutFColumn]}");
            if (MutMColumn > -1)
                Add("Mutations M", $"{row[MutMColumn]}");

            if (CCCColumn > -1)
                Add("3D Coordinates", $"{row[CCCColumn] as string}");

            if (CryoColumn > -1)
            {
                if (row[CryoColumn] != System.DBNull.Value)
                {
                    AddIcon((BitmapImage)row[CryoColumn]);
                    Cryoed = true;
                }
            }

            if (C0Column > -1)
                C0 = row[C0Column] as ArkColor;
            if (C1Column > -1)
                C1 = row[C1Column] as ArkColor;
            if (C2Column > -1)
                C2 = row[C2Column] as ArkColor;
            if (C3Column > -1)
                C3 = row[C3Column] as ArkColor;
            if (C4Column > -1)
                C4 = row[C4Column] as ArkColor;
            if (C5Column > -1)
                C5 = row[C5Column] as ArkColor;

            //return (info);
        }


        public static int MapColumn { get; set; }
        public static int LatColumn { get; set; }
        public static int LonColumn { get; set; }
        private static int CreatureIdColumn { get; set; }
        public static int LvlColumn { get; set; }
        public static int BaseColumn { get; set; }
        public static int CreatureColumn { get; set; }
        public static int NameColumn { get; set; }
        public static int SexColumn { get; set; }
        public static int CryoColumn { get; set; }
        public static int C0Column { get; set; }
        public static int C1Column { get; set; }
        public static int C2Column { get; set; }
        public static int C3Column { get; set; }
        public static int C4Column { get; set; }
        public static int C5Column { get; set; }
        public static int CCCColumn { get; set; }
        public static int HpColumn { get; set; }
        public static int StamColumn { get; set; }
        public static int MeleeColumn { get; set; }
        public static int WeightColumn { get; set; }
        public static int SpeedColumn { get; set; }
        public static int FoodColumn { get; set; }
        public static int OxyColumn { get; set; }
        public static int CraftColumn { get; set; }
        public static int TamerColumn { get; set; }
        public static int ImprinterColumn { get; set; }
        public static int ImprintColumn { get; set; }
        public static int MutFColumn { get; set; }
        public static int MutMColumn { get; set; }

        /// <summary>
        /// Column positions are all assumed to be dynamic (even if they are actually pretty static)
        /// This is to account for future expansion, new datasets, columns, definitions, changes etc.
        /// so we don't need to hardcode/recode values. Not quite as performant as hardcoding but does
        /// the job nicely.
        /// </summary>
        /// <param name="data"></param>
        public static void InitColumnIndexPositions(DataTable data)
        {
            MapColumn = data.Columns["Map"]?.Ordinal ?? -1;
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
            // Ark has some W variants of columns in wild vs tame. We just re-use below for either as we dont care
            HpColumn = data.Columns["Hp"]?.Ordinal ?? data.Columns["HpW"]?.Ordinal ?? -1;
            StamColumn = data.Columns["Stam"]?.Ordinal ?? data.Columns["StamW"]?.Ordinal ?? -1;
            MeleeColumn = data.Columns["Melee"]?.Ordinal ?? data.Columns["MeleeW"]?.Ordinal ?? -1;
            WeightColumn = data.Columns["Weight"]?.Ordinal ?? data.Columns["WeightW"]?.Ordinal ?? -1;
            SpeedColumn = data.Columns["Speed"]?.Ordinal ?? data.Columns["SpeedW"]?.Ordinal ?? -1;
            FoodColumn = data.Columns["Food"]?.Ordinal ?? data.Columns["FoodW"]?.Ordinal ?? -1;
            OxyColumn = data.Columns["Oxy"]?.Ordinal ?? data.Columns["OxyW"]?.Ordinal ?? -1;
            CraftColumn = data.Columns["Craft"]?.Ordinal ?? data.Columns["CraftT"]?.Ordinal ?? -1;
            TamerColumn = data.Columns["Tamer"]?.Ordinal ?? -1;
            ImprinterColumn = data.Columns["Imprinter"]?.Ordinal ?? -1;
            ImprintColumn = data.Columns["Imprint"]?.Ordinal ?? -1;
            MutFColumn = data.Columns["MutF"]?.Ordinal ?? -1;
            MutMColumn = data.Columns["MutM"]?.Ordinal ?? -1;
            CCCColumn = data.Columns["CCC"]?.Ordinal ?? -1;
        }

        // ToDo: Regeneration of extra info all the time is a needless overhead. Cache once generated and reuse
        public static void ShowMassMarkers(string creatureId, string mapName, DataTable data, Grid massMarkerHolder)
        {
            RemoveMassMarkers(massMarkerHolder);    // Make sure any previous markers are no longer there

            if (string.IsNullOrWhiteSpace(creatureId) || string.IsNullOrWhiteSpace(mapName))
                return;

            if (data == null)
                return;

            if (MapColumn == -1 || LatColumn == -1 || LonColumn == -1 || CreatureIdColumn == -1)
                return;

            double lat, lon;
            int level;
            // Translate level from 0->150+ to 0->1%
            double levelPerc;
            byte g;

            foreach (DataRow row in data.Rows)
            {
                if ((string)row[MapColumn] == mapName)
                {
                    if ((string)row[CreatureIdColumn] == creatureId)
                    {
                        lat = (float)row[LatColumn];
                        lon = (float)row[LonColumn];

                        if (lat > -1 && lon > -1)
                        {
                            //Info info = CreateInfoFromRow(row);
                            Info info = new(row);
                            level = info.Level;

                            double yPos = lat - 50.0f;
                            double xPos = lon - 50.0f;

                            levelPerc = level / 150.0d; // e.g. 120 out of max 150 = 80% <-- this will need to become dynamic e.g. if its a Tek dino and max wild level is 180
                            if (levelPerc > 1.0)
                                levelPerc = 1.0;

                            g = (byte)(255 * (1.0 - levelPerc));

                            var rec = new System.Windows.Shapes.Rectangle()
                            {
                                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, g, 0)),
                                Stroke = (info.Sex == "M" ? Brushes.DarkBlue : Brushes.DarkRed),
                                StrokeThickness = 0.2,
                                Width = 1.4,
                                Height = 1.4,
                                RenderTransform = new TranslateTransform(xPos, yPos)
                            };

                            rec.Tag = info;

                           massMarkerHolder.Children.Add(rec);
                        }
                    }
                }
            }
        }

        public static void RemoveMassMarkers(Grid massMarkerHolder)
        {
            massMarkerHolder.Children.Clear();
        }

    }
}
