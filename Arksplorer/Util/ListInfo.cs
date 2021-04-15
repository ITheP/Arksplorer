using System.Collections.Generic;

namespace Arksplorer
{
    public class ListInfo
    {
        public List<ListInfoItem> Items { get; set; }

        public ListInfo()
        {
            Items = new();
        }

        public ListInfo(List<ListInfoItem> items)
        {
            Items = items;
        }

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
