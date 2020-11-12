// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xamarin.Forms;

namespace Sketch360.XPlat
{
    public class PaletteChangedEventArgs : EventArgs
    {
        public PaletteChangedEventArgs(Color penColor, float penWidth)
        {
            PenColor = penColor;
            PenWidth = penWidth;
        }

        public Color PenColor { get; }
        public float PenWidth { get; }
    }
}
