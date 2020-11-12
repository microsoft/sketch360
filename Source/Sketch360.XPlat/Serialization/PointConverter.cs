// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AppCenter.Crashes;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xamarin.Forms;

namespace Sketch360.XPlat.Serialization
{
    public class PointConverter : JsonConverter<Point>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                var text = reader.GetString();

                var parts = (from item in text.Split(new[] { ',' })
                             select double.Parse(item, CultureInfo.InvariantCulture)).ToArray();

                if (parts.Length != 2) return Point.Zero;

                return new Point(parts[0], parts[1]);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing Point: {e.Message}");

                Crashes.TrackError(e);
            }

            return Point.Zero;
        }

        public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteStringValue($"{value.X},{value.Y}");
        }
    }
}
