using Arksplorer;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Emit;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Arksplorer
{
    /// <summary>
    /// General purpose info for showing basic description/value data plus any icons
    /// </summary>
    public class Info
    {
        public int GlobalIndex { get; set; }
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
        public string Tamer { get; set; }
        public string Imprinter { get; set; }
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
            GlobalIndex = (int)row[Globals.GlobalIndexColumn];
            ColumnPositions columnPositions = ((DataTablePlus)row[Globals.DataColumn]).ColumnPositions; // ((DataTablePlus)row[Globals.DataColumn]).ColumnPositions;

            Lat = (float)row[columnPositions.LatColumn];
            Lon = (float)row[columnPositions.LonColumn];

            if (columnPositions.NameColumn > -1)
                Name = row[columnPositions.NameColumn] as string;

            if (columnPositions.CreatureColumn > -1)
                Creature = row[columnPositions.CreatureColumn] as string;

            if (columnPositions.CreatureIdColumn > -1)
                CreatureId = row[columnPositions.CreatureIdColumn] as string;

            if (columnPositions.SexColumn > -1)
            {
                BitmapImage sex = row[columnPositions.SexColumn] as BitmapImage;
                Sex = (sex == IconImages.Male ? "M" : "F");
                AddIcon(sex);
            }

            if (columnPositions.BaseColumn > -1)
            {
                int baseLevel = (row[columnPositions.BaseColumn] as int? ?? -1);
                if (baseLevel > 0)
                {
                    BaseLevel = baseLevel;
                    Add("Base level", $"{row[columnPositions.BaseColumn] as int?}");
                }
            }

            int level = (int)row[columnPositions.LvlColumn];
            Add("Level", $"{level}");
            Level = level;

            if (columnPositions.HpColumn > -1)
                Add("Health", $"{row[columnPositions.WeightColumn]}");

            if (columnPositions.StamColumn > -1)
                Add("Stamina", $"{row[columnPositions.StamColumn]}");

            if (columnPositions.MeleeColumn > -1)
                Add("Melee", $"{row[columnPositions.MeleeColumn]}");

            if (columnPositions.WeightColumn > -1)
                Add("Weight", $"{row[columnPositions.WeightColumn]}");

            if (columnPositions.SpeedColumn > -1)
                Add("Speed", $"{row[columnPositions.SpeedColumn]}");

            if (columnPositions.FoodColumn > -1)
                Add("Food", $"{row[columnPositions.FoodColumn]}");

            if (columnPositions.OxyColumn > -1)
                Add("Oxygen", $"{row[columnPositions.OxyColumn]}");

            if (columnPositions.CraftColumn > -1)
                Add("Crafting", $"{row[columnPositions.CraftColumn]}");

            if (columnPositions.StamColumn > -1)
                Add("Stamina", $"{row[columnPositions.StamColumn]}");

            if (columnPositions.TribeColumn > -1)
            {
                string tribe = $"{row[columnPositions.TribeColumn] as string}";
                Add("Tribe", tribe);
                Tribe = tribe;
            }

            if (columnPositions.TamerColumn > -1)
            {
                string tamer = $"{row[columnPositions.TamerColumn] as string}";
                Add("Tamer", tamer);
                Tamer = tamer;
            }

            if (columnPositions.ImprinterColumn > -1)
            {
                string imprinter = $"{row[columnPositions.ImprinterColumn] as string}";
                Add("Imprinter", imprinter);
                Imprinter = imprinter;
            }

            if (columnPositions.ImprintColumn > -1)
                Add("Imprint", $"{row[columnPositions.ImprintColumn]}");

            if (columnPositions.MutFColumn > -1)
                Add("Mutations F", $"{row[columnPositions.MutFColumn]}");

            if (columnPositions.MutMColumn > -1)
                Add("Mutations M", $"{row[columnPositions.MutMColumn]}");

            if (columnPositions.CCCColumn > -1)
                Add("3D Coordinates", $"{row[columnPositions.CCCColumn] as string}");

            if (columnPositions.CryoColumn > -1)
            {
                if (row[columnPositions.CryoColumn] != System.DBNull.Value)
                {
                    AddIcon((BitmapImage)row[columnPositions.CryoColumn]);
                    Cryoed = true;
                }
            }

            if (columnPositions.C0Column > -1)
                C0 = row[columnPositions.C0Column] as ArkColor;
            if (columnPositions.C1Column > -1)
                C1 = row[columnPositions.C1Column] as ArkColor;
            if (columnPositions.C2Column > -1)
                C2 = row[columnPositions.C2Column] as ArkColor;
            if (columnPositions.C3Column > -1)
                C3 = row[columnPositions.C3Column] as ArkColor;
            if (columnPositions.C4Column > -1)
                C4 = row[columnPositions.C4Column] as ArkColor;
            if (columnPositions.C5Column > -1)
                C5 = row[columnPositions.C5Column] as ArkColor;

            //return (info);
        }


        ////public static int GlobalIndexColumn { get; set; }
        ////public static int MapColumn { get; set; }
        //public static int LatColumn { get; set; }
        //public static int LonColumn { get; set; }
        //private static int CreatureIdColumn { get; set; }
        //public static int LvlColumn { get; set; }
        //public static int BaseColumn { get; set; }
        //public static int CreatureColumn { get; set; }
        //public static int NameColumn { get; set; }
        //public static int SexColumn { get; set; }
        //public static int CryoColumn { get; set; }
        //public static int C0Column { get; set; }
        //public static int C1Column { get; set; }
        //public static int C2Column { get; set; }
        //public static int C3Column { get; set; }
        //public static int C4Column { get; set; }
        //public static int C5Column { get; set; }
        //public static int CCCColumn { get; set; }
        //public static int HpColumn { get; set; }
        //public static int StamColumn { get; set; }
        //public static int MeleeColumn { get; set; }
        //public static int WeightColumn { get; set; }
        //public static int SpeedColumn { get; set; }
        //public static int FoodColumn { get; set; }
        //public static int OxyColumn { get; set; }
        //public static int CraftColumn { get; set; }
        //public static int TribeColumn { get; set; }
        //public static int TamerColumn { get; set; }
        //public static int ImprinterColumn { get; set; }
        //public static int ImprintColumn { get; set; }
        //public static int MutFColumn { get; set; }
        //public static int MutMColumn { get; set; }

        ///// <summary>
        ///// Column positions are all assumed to be dynamic (even if they are actually pretty static)
        ///// This is to account for future expansion, new datasets, columns, definitions, changes etc.
        ///// so we don't need to hardcode/recode values. Not quite as performant as hardcoding but does
        ///// the job nicely.
        ///// </summary>
        ///// <param name="data"></param>
        //public static void InitColumnIndexPositions(DataTable data)
        //{
        //    //GlobalIndexColumn = data.Columns["GlobalIndex"]?.Ordinal ?? -1;
        //    // ...Always 0, set in Globals
        //    //MapColumn = data.Columns["Map"]?.Ordinal ?? -1;
        //    // ...Always 1, set in Globals
        //    LatColumn = data.Columns["Lat"]?.Ordinal ?? -1;
        //    LonColumn = data.Columns["Lon"]?.Ordinal ?? -1;
        //    CreatureIdColumn = data.Columns["CreatureId"]?.Ordinal ?? -1;
        //    LvlColumn = data.Columns["Lvl"]?.Ordinal ?? -1;
        //    BaseColumn = data.Columns["Base"]?.Ordinal ?? -1;
        //    CreatureColumn = data.Columns["Creature"]?.Ordinal ?? -1;
        //    NameColumn = data.Columns["Name"]?.Ordinal ?? -1;
        //    SexColumn = data.Columns["Sex"]?.Ordinal ?? -1;
        //    CryoColumn = data.Columns["Cryo"]?.Ordinal ?? -1;
        //    C0Column = data.Columns["C0"]?.Ordinal ?? -1;
        //    C1Column = data.Columns["C1"]?.Ordinal ?? -1;
        //    C2Column = data.Columns["C2"]?.Ordinal ?? -1;
        //    C3Column = data.Columns["C3"]?.Ordinal ?? -1;
        //    C4Column = data.Columns["C4"]?.Ordinal ?? -1;
        //    C5Column = data.Columns["C5"]?.Ordinal ?? -1;
        //    // Ark has some W variants of columns in wild vs tame. We just re-use below for either as we dont care
        //    HpColumn = data.Columns["Hp"]?.Ordinal ?? data.Columns["HpW"]?.Ordinal ?? -1;
        //    StamColumn = data.Columns["Stam"]?.Ordinal ?? data.Columns["StamW"]?.Ordinal ?? -1;
        //    MeleeColumn = data.Columns["Melee"]?.Ordinal ?? data.Columns["MeleeW"]?.Ordinal ?? -1;
        //    WeightColumn = data.Columns["Weight"]?.Ordinal ?? data.Columns["WeightW"]?.Ordinal ?? -1;
        //    SpeedColumn = data.Columns["Speed"]?.Ordinal ?? data.Columns["SpeedW"]?.Ordinal ?? -1;
        //    FoodColumn = data.Columns["Food"]?.Ordinal ?? data.Columns["FoodW"]?.Ordinal ?? -1;
        //    OxyColumn = data.Columns["Oxy"]?.Ordinal ?? data.Columns["OxyW"]?.Ordinal ?? -1;
        //    CraftColumn = data.Columns["Craft"]?.Ordinal ?? data.Columns["CraftT"]?.Ordinal ?? -1;
        //    TribeColumn = data.Columns["Tribe"]?.Ordinal ?? -1;
        //    TamerColumn = data.Columns["Tamer"]?.Ordinal ?? -1;
        //    ImprinterColumn = data.Columns["Imprinter"]?.Ordinal ?? -1;
        //    ImprintColumn = data.Columns["Imprint"]?.Ordinal ?? -1;
        //    MutFColumn = data.Columns["MutF"]?.Ordinal ?? -1;
        //    MutMColumn = data.Columns["MutM"]?.Ordinal ?? -1;
        //    CCCColumn = data.Columns["CCC"]?.Ordinal ?? -1;
        //}

        // ToDo: Regeneration of extra info all the time is a needless overhead. Cache once generated and reuse
        public static void ShowMassMarkers(string creatureId, string mapName, DataTable data, Grid massMarkerHolder, MassMarkerType colourType)
        {
            RemoveMassMarkers(massMarkerHolder);    // Make sure any previous markers are no longer there

            if (string.IsNullOrWhiteSpace(creatureId) || string.IsNullOrWhiteSpace(mapName))
                return;

            if (data == null)
                return;

            if (data.Rows.Count == 0)
                return;

            // We can get columnPositions from first record
            ColumnPositions columnPositions = ((DataTablePlus)data.Rows[0].ItemArray[Globals.DataColumn]).ColumnPositions;

            if (columnPositions.LatColumn == -1 || columnPositions.LonColumn == -1 || columnPositions.CreatureIdColumn == -1)
                return;

            double lat, lon;
            int level;
            //// Translate level from 0->150+ to 0->1%
            //double levelPerc;
            //byte g;

            foreach (DataRow row in data.Rows)
            {
                if ((string)row[Globals.MapColumn] == mapName)
                {
                    if ((string)row[columnPositions.CreatureIdColumn] == creatureId)
                    {
                        lat = (float)row[columnPositions.LatColumn];
                        lon = (float)row[columnPositions.LonColumn];

                        if (lat > -1 && lon > -1)
                        {
                            //Info info = CreateInfoFromRow(row);
                            Info info = new(row);
                            level = info.Level;

                            double yPos = lat - 50.0f;
                            double xPos = lon - 50.0f;

                            //levelPerc = level / 150.0d; // e.g. 120 out of max 150 = 80% <-- this will need to become dynamic e.g. if its a Tek dino and max wild level is 180
                            //if (levelPerc > 1.0)
                            //    levelPerc = 1.0;

                            //g = (byte)(255 * (1.0 - levelPerc));

                            var rec = new System.Windows.Shapes.Rectangle()
                            {
                                //Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, g, 0)),
                                //Stroke = (info.Sex == "M" ? Brushes.DarkBlue : Brushes.DarkRed),
                                //StrokeThickness = 0.2,
                                Width = 1.4,
                                Height = 1.4,
                                RenderTransform = new TranslateTransform(xPos, yPos)
                            };

                            SetMassMarkerColour(rec, info, colourType);

                            rec.Tag = info;

                           massMarkerHolder.Children.Add(rec);
                        }
                    }
                }
            }
        }

        //public static void SetMassMarkersColours(Grid massMarkerHolder, MassMarkerColours colourType)
        //{
        //    foreach (var marker in massMarkerHolder.Children)
        //    {
        //        ((Rectangle)marker).Fill = 
        //    }
        //}

        private static System.Windows.Point GradientStartPoint = new System.Windows.Point(0.5, 0);
        private static System.Windows.Point GradientEndPoint = new System.Windows.Point(0.5, 1);

        public static void SetMassMarkerColour(Rectangle rectangle, Info info, MassMarkerType colourType)
        {
            switch (colourType)
            {
                case MassMarkerType.Sex:
                    double levelPerc = info.Level / 150.0d; // e.g. 120 out of max 150 = 80% <-- this will need to become dynamic e.g. if its a Tek dino and max wild level is 180
                    if (levelPerc > 1.0)
                        levelPerc = 1.0;

                    byte g = (byte)(255 * (1.0 - levelPerc));

                    rectangle.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, g, 0));

                    rectangle.Stroke = (info.Sex == "M" ? Brushes.DarkBlue : Brushes.DarkRed);
                    rectangle.StrokeThickness = 0.2;

                    break;
                case MassMarkerType.Colours:
                    GradientStopCollection gradientStops = new();

                    if (info.C0 != null)
                    {
                        gradientStops.Add(new GradientStop(((SolidColorBrush)info.C0.Color).Color, 0d));
                        gradientStops.Add(new GradientStop(((SolidColorBrush)info.C0.Color).Color, 0.1667d));
                    }
                    if (info.C1 != null)
                    {
                        gradientStops.Add(new GradientStop(((SolidColorBrush)info.C1.Color).Color, 0.1667d));
                        gradientStops.Add(new GradientStop(((SolidColorBrush)info.C1.Color).Color, 0.3333d));
                    }
                    if (info.C2 != null)
                    {
                        gradientStops.Add(new GradientStop(((SolidColorBrush)info.C2.Color).Color, 0.3333d));
                        gradientStops.Add(new GradientStop(((SolidColorBrush)info.C2.Color).Color, 0.5d));
                    }
                    if (info.C3 != null)
                    {
                        gradientStops.Add(new GradientStop(((SolidColorBrush)info.C3.Color).Color, 0.5d));
                        gradientStops.Add(new GradientStop(((SolidColorBrush)info.C3.Color).Color, 0.6667d));
                    }
                    if (info.C4 != null)
                    {
                        gradientStops.Add(new GradientStop(((SolidColorBrush)info.C4.Color).Color, 0.6667d));
                        gradientStops.Add(new GradientStop(((SolidColorBrush)info.C4.Color).Color, 0.8333d));
                    }
                    if (info.C5 != null)
                    {
                        gradientStops.Add(new GradientStop(((SolidColorBrush)info.C5.Color).Color, 0.8333d));
                        gradientStops.Add(new GradientStop(((SolidColorBrush)info.C5.Color).Color, 1.0d));
                    }

                    rectangle.Fill = new LinearGradientBrush(gradientStops)
                    {
                        StartPoint = new System.Windows.Point(0.5, 0),
                        EndPoint = new System.Windows.Point(0.5, 1)
                    };

                    rectangle.Stroke = Brushes.Black;
                    rectangle.StrokeThickness = 0.1;

                    break;
                default:
                    break;
            }
        }

        public static void RemoveMassMarkers(Grid massMarkerHolder)
        {
            massMarkerHolder.Children.Clear();
        }

    }

    public enum MassMarkerType
    {
        None = 0,
        Colours = 1,
        Sex = 2
    }
}
