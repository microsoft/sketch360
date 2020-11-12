// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Sketch360.XPlat
{
    /// <summary>
    /// Stream extensions
    /// </summary>
    public static class StreamExtension
    {
        /// <summary>
        /// Get the base 64 string for a stream
        /// </summary>
        /// <param name="stream">the stream</param>
        /// <returns>a base-64 string</returns>
        public static string ConvertToBase64(this Stream stream)
        {
            if (stream == null) return null;

            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            string base64 = Convert.ToBase64String(bytes);

            return base64;
        }
    }
}
