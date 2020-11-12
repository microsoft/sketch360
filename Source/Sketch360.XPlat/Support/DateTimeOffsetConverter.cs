// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sketch360.XPlat.Support
{
    /// <summary>
    /// DateTimeOffset JSON converter
    /// </summary>
    /// <remarks>from <![CDATA[https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to]]></remarks>
    public sealed class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return DateTimeOffset.ParseExact(reader.GetString(),
                "u", CultureInfo.InvariantCulture);
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTimeOffset dateTimeValue,
            JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteStringValue(dateTimeValue.ToString("u", CultureInfo.InvariantCulture));
        }
    }
}
