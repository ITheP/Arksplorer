using System.Collections.Generic;
using System.ComponentModel;

namespace Arksplorer
{
    public class ListInfoItem
    {
        public string Description { get; set; }
        public string Value { get; set; }
        public string Details { get; set; }
    }

    public class ListInfo
    {
        public List<ListInfoItem> Items { get; set; } = new();

        public void Add(string value, string description = null, string details = null)
        {
            ListInfoItem info = new()
            {
                Description = description,
                Value = value,
                Details = details
            };

            Items.Add(info);
        }
    }
}
