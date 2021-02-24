// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Inking;

namespace Sketch360.XPlat.Interfaces
{
    public interface ISketchData
    {
        int Width { get; set; }

        int Height { get; set; }

        IEnumerable<XInkStroke> InkStrokes { get; set; }

        Color BackgroundColor { get; set; }

        string Name { get; set; }

        public TimeSpan Duration { get; }
        DateTimeOffset Start { get; set; }

        void Add(IEnumerable<XInkStroke> strokes);

        void Remove(XInkStroke stroke);
    }
}
