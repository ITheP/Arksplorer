using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Arksplorer
{
    public class SexConverter : JsonConverter<string>
    {
        public override bool HandleNull => false;

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.GetInt16() == 0)
                return "M";
            else
                return "F";
        }
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            return;
        }
    }
}
