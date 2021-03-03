using System.Collections.Generic;

namespace Arksplorer
{
    public class ServerData
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
        public List<ServerFiles> FileTypes { get; set; }
        /// <summary>
        /// The different maps available on the server
        /// </summary>
        public List<ServerMaps> Maps { get; set; }
    }

    public class ServerFiles
    {
        public string Name { get; set; }
        public string Filename { get; set; }
    }

    public class ServerMaps
    {
        public string Name { get; set; }
        public string BaseUrl { get; set; }
    }
}

