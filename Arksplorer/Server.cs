using System;
using System.Collections.Generic;
using System.IO;

namespace Arksplorer
{
    /// <summary>
    /// Server read in from local Servers.json
    /// </summary>
    public class Server
    {
        public string Name { get; set; }
        public string Website { get; set; }
        public string Url { get; set; }
    }

    public class ServerConfig
    {
        public string Description { get; set; }
        public string Website { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        /// <summary>
        /// How often the server refreshes data, in minutes. Used to help e.g. not try and refresh data during periods we know it won't have changed.
        /// </summary>
        public int RefreshRate { get; set; }
        public float FoodDrainMultiplier { get; set; }
        public double HarvestAmountMultiplier { get; set; }
        public double TamingSpeedMultiplier { get; set; }
        public double XPMultiplier { get; set; }
        public double DinoCountMultiplier { get; set; }
        public double DayTimeSpeedScale { get; set; }
        public double NightTimeSpeedScale { get; set; }
        public double HexagonRewardMultiplier { get; set; }
        public int KickIdlePlayersPeriod { get; set; }
        public double MatingIntervalMultiplier { get; set; }
        public double EggHatchSpeedMultiplier { get; set; }

        public ListInfo ServerDetailsOverview { get; private set; }

        /// <summary>
        /// Basically we want a dumping ground for URL's to web site data sources. These may differ per server etc.
        /// Also include URL's to timestamp information.
        /// </summary>
        public Dictionary<string, Uri> Uris { get; } = new();
        public List<string> Maps { get; } = new();
        public List<String> Types { get; } = new();

        /// <summary>
        /// Converts server data into something internally usable!
        /// </summary>
        /// <param name="serverData"></param>
        public ServerConfig(RawServerData serverData)
        {
            Description = serverData.Description;
            Website = serverData.Website;
            Message = serverData.Message;
            Status = serverData.Status;
            RefreshRate = serverData.RefreshRate;
            FoodDrainMultiplier = serverData.FoodDrainMultiplier;
            HarvestAmountMultiplier = serverData.HarvestAmountMultiplier;
            TamingSpeedMultiplier = serverData.TamingSpeedMultiplier;
            XPMultiplier = serverData.XPMultiplier;
            DinoCountMultiplier = serverData.DinoCountMultiplier;
            DayTimeSpeedScale = serverData.DayTimeSpeedScale;
            NightTimeSpeedScale = serverData.NightTimeSpeedScale;
            HexagonRewardMultiplier = serverData.HexagonRewardMultiplier;
            KickIdlePlayersPeriod = serverData.KickIdlePlayersPeriod;
            MatingIntervalMultiplier = serverData.MatingIntervalMultiplier;
            EggHatchSpeedMultiplier = serverData.EggHatchSpeedMultiplier;

            foreach (RawServerMaps map in serverData.Maps)
            {
                // e.g. http://wiredcat.hopto.org/WiredcatRagnarok/timestamp.json
                //string url = Path.Combine(map.BaseUrl, serverData.TimestampFilename);
                //Uris.Add($"{map.Name}.timestamp", new Uri(url));

                foreach (RawServerFiles type in serverData.Types)
                {
                    // e.g. <Ragnarok>.<WildDinos>
                    string key = $"{map.Name}.{type.Name}".ToLower();

                    // e.g. http://wiredcat.hopto.org/WiredcatRagnarok/Wild.json
                    string url = Path.Combine(map.BaseUrl, type.Filename);
                    Uris.Add(key, new Uri(url));
                }

                Maps.Add(map.Name);
            }

            foreach (RawServerFiles type in serverData.Types)
                Types.Add(type.Name);
        }

        /// <summary>
        /// Returns a Uri (if found) - use key lookup of map.type to search, e.g. "radnarok.wilddinos"
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public Uri GetUri(string description)
        {
            if (Uris.TryGetValue(description.ToLower(), out Uri result))
                return result;

            return null;
        }

        public ListInfo GetServerOverview()
        {
            if (ServerDetailsOverview == null)
            {
                ListInfo info = new();

                info.Add(Description, "Description");
                info.Add(Website, "Website");
                info.Add(Message, "Message");
                info.Add(Status, "Status", "Not currently used");
                info.Add($"{RefreshRate}", "Refresh rate", "How often (in minutes) the server refreshes its data. Note that this is approximate.");
                info.Add($"{FoodDrainMultiplier}x", "Food drain multiplier");
                info.Add($"{HarvestAmountMultiplier}x", "Harvest amount multiplier");
                info.Add($"{TamingSpeedMultiplier}x", "Taming speed multiplier");
                info.Add($"{XPMultiplier}x", "XP Multiplier");
                info.Add($"{DinoCountMultiplier}x", "Dino count multiplier");
                info.Add($"{DayTimeSpeedScale}x", "Day time speed scale");
                info.Add($"{NightTimeSpeedScale}x", "Night time speed scale");
                info.Add($"{HexagonRewardMultiplier}x", "Hexagon reward multiplier");
                info.Add($"{KickIdlePlayersPeriod}", "Kick idle players period", "Time before players are kicked for being idle");
                info.Add($"{MatingIntervalMultiplier}x", "Mating interval multiplier");
                info.Add($"{EggHatchSpeedMultiplier}x", "Egg hatching speed multiplier");

                ServerDetailsOverview = info;
            }

            return ServerDetailsOverview;
        }
    }

    public class RawServerData
    {
        /// <summary>
        /// Basic description of server
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Main public website people can visit for the server
        /// </summary>
        public string Website { get; set; }
        /// <summary>
        /// Filename used for map timestamps
        /// </summary>
        public string TimestampFilename { get; set; } = "timestamp.json";
        /// <summary>
        /// Any extra message the server might want to pass on - e.g. during downtime it might want to say why, or give availability info.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// If disabled, then Arksplorer should quit with a warning message. (E.g. server is undergoing maintenance)
        /// </summary>
        public string Status { get; set; }
        public int RefreshRate { get; set; } = 60;

        public float FoodDrainMultiplier { get; set; }
        public double HarvestAmountMultiplier { get; set; }
        public double TamingSpeedMultiplier { get; set; }
        public double XPMultiplier { get; set; }
        public double DinoCountMultiplier { get; set; }
        public double DayTimeSpeedScale { get; set; }
        public double NightTimeSpeedScale { get; set; }
        public double HexagonRewardMultiplier { get; set; }
        public int KickIdlePlayersPeriod { get; set; }
        public double MatingIntervalMultiplier { get; set; }
        public double EggHatchSpeedMultiplier { get; set; }

        /// <summary>
        /// The different types of data files available, e.g. Dinos, Wild Dinos, Survivors, etc.
        /// </summary>
        public List<RawServerFiles> Types { get; set; }
        /// <summary>
        /// The different maps available on the server
        /// </summary>
        public List<RawServerMaps> Maps { get; set; }
    }

    public class RawServerFiles
    {
        public string Name { get; set; }
        public string Filename { get; set; }
    }

    public class RawServerMaps
    {
        public string Name { get; set; }
        public string BaseUrl { get; set; }
    }
}

