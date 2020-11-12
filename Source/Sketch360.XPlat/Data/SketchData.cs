// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xamarin.Forms;
using Xamarin.Forms.Inking;

namespace Sketch360.XPlat.Data
{
    /// <summary>
    /// Sketch data
    /// </summary>
    public sealed class SketchData : ISketchData
    {
        private readonly List<XInkStroke> _inkStrokes = new List<XInkStroke>();

        /// <summary>
        /// Initializes a new instance of the SketchData class
        /// </summary>
        public SketchData()
        {
            Start = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Gets or sets the start date/time of the sketch
        /// </summary>
        public DateTimeOffset Start { get; set; }

        /// <summary>
        /// Gets the amount of time from the creation of this sketch
        /// </summary>
        [JsonIgnore]
        public TimeSpan Duration
        {
            get
            {
                return DateTimeOffset.UtcNow - Start;
            }
        }

        /// <summary>
        /// Gets or sets the name of the sketch
        /// </summary>
        public string Name { get; set; } = "Sketch";

        /// <summary>
        /// Gets or sets the background color
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.Beige;

        /// <summary>
        /// gets or sets the ink strokes
        /// </summary>
        public IEnumerable<XInkStroke> InkStrokes
        {
            get => _inkStrokes;

            set
            {
                lock (_inkStrokes)
                {
                    _inkStrokes.Clear();
                    _inkStrokes.AddRange(value);
                }
            }
        }

        public void Add(IEnumerable<XInkStroke> strokes)
        {
            lock (_inkStrokes)
            {
                _inkStrokes.AddRange(strokes);
            }
        }

        public void Remove(XInkStroke stroke)
        {
            lock (_inkStrokes)
            {
                _inkStrokes.Remove(stroke);
            }
        }
        /// <summary>
        /// Gets or sets the width of the sketch in pixels
        /// </summary>
        public int Width { get; set; } = 2000;


        /// <summary>
        /// Gets or sets the height of the sketch in pixels
        /// </summary>
        public int Height { get; set; } = 1000;
    }
}
