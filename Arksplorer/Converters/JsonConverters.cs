using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Arksplorer
{
    //public class SexConverter : JsonConverter<string>
    //{
    //    public override bool HandleNull => false;

    //    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        if (reader.GetInt16() == 0)
    //            return "M";
    //        else
    //            return "F";
    //    }
    //    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    //    {
    //        return;
    //    }
    //}

    public class SexConverter : JsonConverter<BitmapImage>
    {
        public override bool HandleNull => false;

        public override BitmapImage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.GetInt16() == 0)
                return IconImages.Male;
            else
                return IconImages.Female;
        }
        public override void Write(Utf8JsonWriter writer, BitmapImage value, JsonSerializerOptions options)
        {
            return;
        }
    }

    public class CryoConverter : JsonConverter<BitmapImage>
    {
        public override bool HandleNull => false;

        public override BitmapImage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.GetBoolean())
                return IconImages.Cryopod;
            else
                return null;
        }
        public override void Write(Utf8JsonWriter writer, BitmapImage value, JsonSerializerOptions options)
        {
            return;
        }
    }

    public class ArkColorConverter : JsonConverter<ArkColor>
    {
        public override bool HandleNull => false;

        public override ArkColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            ArkColor arkColor = Lookup.FindColor(reader.GetInt16());

            return arkColor;
        }
        public override void Write(Utf8JsonWriter writer, ArkColor value, JsonSerializerOptions options)
        {
            return;
        }
    }
}
