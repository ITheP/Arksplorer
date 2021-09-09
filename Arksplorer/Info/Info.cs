using Arksplorer;
using Arksplorer.Properties;
using System.Collections.Generic;
using System.Configuration;
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
            GlobalIndex = (int)row[Globals.StaticColumnIndex_GlobalIndex];
            ColumnPositions columnPositions = ((DataTablePlus)row[Globals.StaticColumnIndex_DataSource]).ColumnPositions; // ((DataTablePlus)row[Globals.DataColumn]).ColumnPositions;

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
        }

        // ToDo: Regeneration of extra info all the time is a needless overhead. Cache once generated and reuse?

        /// <summary>
        /// Handling mass markers does allow for all creatures on a map to be displayed, but so croweded you cant really see anything. Will only show first e.g. 512 dinos of non selected type.
        /// </summary>
        /// <param name="creatureId"></param>
        /// <param name="mapName"></param>
        /// <param name="data"></param>
        /// <param name="massMarkerHolder"></param>
        /// <param name="colourType"></param>
        /// <param name="showAll"></param>
        public static void ShowMassMarkers(string creatureId, string mapName, DataTable data, Grid massMarkerHolder, MassMarkerColouring colourType, bool showAll)
        {
            RemoveMassMarkers(massMarkerHolder);    // Make sure any previous markers are no longer there

            if (!showAll && string.IsNullOrWhiteSpace(creatureId))
                return;

            if (string.IsNullOrWhiteSpace(mapName))
                return;

            if (data == null)
                return;

            if (data.Rows.Count == 0)
                return;

            // We can get columnPositions from first record
            ColumnPositions columnPositions = ((DataTablePlus)data.Rows[0].ItemArray[Globals.StaticColumnIndex_DataSource]).ColumnPositions;

            if (columnPositions.LatColumn == -1 || columnPositions.LonColumn == -1 || columnPositions.CreatureIdColumn == -1)
                return;

            double lat, lon;
            int nonSelectedCount = 0;
            int MaxNonSelectedCount = Settings.Default.MassMarkerMaxNonSelectedCreatureCount;

            foreach (DataRow row in data.Rows)
            {
                if ((string)row[Globals.StaticColumnIndex_Map] == mapName)
                {
                    if ((string)row[columnPositions.CreatureIdColumn] == creatureId || (showAll && nonSelectedCount++ < MaxNonSelectedCount))   // nonSelectedCount will only increase if creature id doesnt match (only evaluates if creatureId doesn't match)
                    {
                        lat = (float)row[columnPositions.LatColumn];
                        lon = (float)row[columnPositions.LonColumn];

                        if (lat > -1 && lon > -1)
                        {
                            Info info = new(row);

                            // ToDo: Cache info on extra field in row so only generated once when required

                            double yPos = lat - 50.0f;
                            double xPos = lon - 50.0f;

                            var rec = new System.Windows.Shapes.Rectangle()
                            {
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

        private static System.Windows.Point GradientStartPoint = new System.Windows.Point(0.5, 0);
        private static System.Windows.Point GradientEndPoint = new System.Windows.Point(0.5, 1);

        public static void SetMassMarkerColour(Rectangle rectangle, Info info, MassMarkerColouring colourType)
        {
            switch (colourType)
            {
                case MassMarkerColouring.Sex:
                    double levelPerc = info.Level / 150.0d; // e.g. 120 out of max 150 = 80% <-- this will need to become dynamic e.g. if its a Tek dino and max wild level is 180
                    if (levelPerc > 1.0)
                        levelPerc = 1.0;

                    byte g = (byte)(255 * (1.0 - levelPerc));

                    rectangle.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, g, 0));

                    rectangle.Stroke = (info.Sex == "M" ? Brushes.DarkBlue : Brushes.DarkRed);
                    rectangle.StrokeThickness = 0.2;

                    break;
                case MassMarkerColouring.Colours:
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
}
