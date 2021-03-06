﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Arksplorer
{
    public class SexConverter : JsonConverter<BitmapImage>
    {
        public override bool HandleNull => false;

        public override BitmapImage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string sex = reader.GetString();
            if (sex == "Male")
                return IconImages.Male;
            else if (sex == "Female")
                return IconImages.Female;
            else
                return IconImages.NA;
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
