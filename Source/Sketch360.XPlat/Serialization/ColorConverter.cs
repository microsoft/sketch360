// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xamarin.Forms;

namespace Sketch360.XPlat.Serialization
{
    public class ColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var colorText = reader.GetString();

            return Color.FromHex(colorText);
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            var hex = value.ToHex();

            writer.WriteStringValue(hex);
        }
    }
}
