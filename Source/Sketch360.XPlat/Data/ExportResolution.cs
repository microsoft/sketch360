// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Sketch360.XPlat.Data
{
    public class ExportResolution
    {
        public ExportResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public int Width { get; set; }
        public int Height { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} x {1}", Width, Height);
        }
    }
}
