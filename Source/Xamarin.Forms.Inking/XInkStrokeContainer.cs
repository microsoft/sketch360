// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Inking.Interfaces;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Xamarin InkStrokeContainer
    /// </summary>
    public sealed class XInkStrokeContainer : IDisposable, IInkStrokeContainer
    {
        private readonly List<XInkStroke> _inkStrokes = new List<XInkStroke>();

        /// <summary>
        /// Ink changed event handler
        /// </summary>
        public event EventHandler InkChanged;

        /// <summary>
        /// Gets the ink strokes
        /// </summary>
        /// <returns>a read-only list of ink strokes</returns>
        public IReadOnlyList<XInkStroke> GetStrokes()
        {
            return _inkStrokes.ToList();
        }

        /// <summary>
        /// Adds ink strokes
        /// </summary>
        /// <param name="strokes">a collection of ink strokes</param>
        public void Add(IEnumerable<XInkStroke> strokes)
        {
            lock (_inkStrokes)
            {
                _inkStrokes.AddRange(strokes);

                InkChanged?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Add an ink stroke to the container
        /// </summary>
        /// <param name="stroke">the stroke to add</param>
        public void Add(XInkStroke stroke)
        {
            lock (_inkStrokes)
            {
                _inkStrokes.Add(stroke);

                InkChanged?.Invoke(this, new EventArgs());

            }
        }

        /// <summary>
        /// Remove all of the ink stroks from the container
        /// </summary>
        public void Clear()
        {
            lock (_inkStrokes)
            {
                var args = new XInkStrokesErasedEventArgs(_inkStrokes.ToList());

                if (_inkStrokes.Any())
                {
                    _inkStrokes.ForEach(delegate (XInkStroke item)
                    {
                        item.Dispose();
                    });

                    _inkStrokes.Clear();

                    InkChanged?.Invoke(this, new EventArgs());
                }
            }

            GC.Collect();
        }

        //public void Deserialize(string json)
        //{
        //    Dispose();

        //    var strokes = JsonConvert.DeserializeObject<List<InkStroke>>(json);

        //    Strokes.ForEach(delegate (InkStroke stroke)
        //    {
        //        if (stroke.Color.A == 0)
        //        {
        //            stroke.Color = Xamarin.Forms.Color.FromRgba(stroke.Color.R, stroke.Color.G, stroke.Color.B, 255);
        //        }

        //        stroke.UpdateBounds();
        //    });
        //}

        //public string Serialize()
        //{
        //    return JsonConvert.SerializeObject(Strokes);
        //}
        /// <summary>
        /// Dispose of the strokes
        /// </summary>
        public void Dispose()
        {
            _inkStrokes.ForEach(delegate (XInkStroke stroke)
            {
                stroke.Dispose();
            });

            _inkStrokes.Clear();

            InkChanged?.Invoke(this, new EventArgs());

        }

        /// <summary>
        /// Remove the ink stroke
        /// </summary>
        /// <param name="item">the ink stroke</param>
        public void Remove(XInkStroke item)
        {
            if (item == null) return;

            lock (_inkStrokes)
            {
                item.Dispose();

                _inkStrokes.Remove(item);
            }

            InkChanged?.Invoke(this, new EventArgs());

        }

        /// <summary>
        /// remove a list of strokes
        /// </summary>
        /// <param name="strokesToRemove">the strokes to remove</param>
        public void Remove(IReadOnlyList<XInkStroke> strokesToRemove)
        {
            if (strokesToRemove == null) throw new ArgumentNullException(nameof(strokesToRemove));

            lock (_inkStrokes)
            {
                foreach (var item in strokesToRemove)
                {
                    item.Dispose();

                    _inkStrokes.Remove(item);
                }
            }

            InkChanged?.Invoke(this, new EventArgs());

        }

        /// <summary>
        /// Sets the ink strokes
        /// </summary>
        /// <param name="inkStrokes">a collection of ink strokes</param>
        public void SetRange(IEnumerable<XInkStroke> inkStrokes)
        {
            lock (_inkStrokes)
            {
                _inkStrokes.ForEach(delegate (XInkStroke stroke)
                {
                    stroke.Dispose();
                });
                _inkStrokes.Clear();

                _inkStrokes.AddRange(inkStrokes);
            }

            InkChanged?.Invoke(this, new EventArgs());

        }
    }
}
