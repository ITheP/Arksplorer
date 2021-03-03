using System;
using System.Collections.Generic;
using System.IO;

namespace Arksplorer
{
    public class ServerConfig
    {
        public string Description { get; set; }
        public string Website { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
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

            foreach (RawServerMaps map in serverData.Maps)
            {
                // e.g. <http://wiredcat.hopto.org/WiredcatRagnarok/><timestamp.json>
                //string url = Path.Combine(map.BaseUrl, serverData.TimestampFilename);
                //Uris.Add($"{map.Name}.timestamp", new Uri(url));

                foreach (RawServerFiles type in serverData.Types)
                {
                    // e.g. <Ragnarok>.<WildDinos>
                    string key = $"{map.Name}.{type.Name}".ToLower();

                    // e.g.<http://wiredcat.hopto.org/WiredcatRagnarok/><Wild.json>
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
            Uri result;

            if (Uris.TryGetValue(description.ToLower(), out result))
                return result;

            return null;
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

