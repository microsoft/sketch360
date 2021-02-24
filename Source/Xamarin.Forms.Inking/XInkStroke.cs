// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Xamarin InkStroke
    /// </summary>
    [DebuggerDisplay("Points: {Points.Count} Start time: {StrokeStartTime.Value}")]
    public sealed class XInkStroke : IDisposable
    {
        IEnumerable<IDisposable> _resources;

        /// <summary>
        /// Initializes a new instance of the XInkStroke class.
        /// </summary>
        public XInkStroke()
        {
            Points = new List<XInkPoint>();
        }
        /// <summary>
        /// Gets or sets the unique Id of the ink stroke
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();


        /// <summary>
        /// Gets or sets the points for serialization
        /// </summary>
        public IList<XInkPoint> Points { get; set; }

        /// <summary>
        /// Gets or sets the drawing attributes
        /// </summary>
        public XInkDrawingAttributes DrawingAttributes { get; set; } = new XInkDrawingAttributes();

        /// <summary>
        /// Gets or sets the date and time when the InkStroke was started
        /// </summary>
        public DateTimeOffset? StrokeStartTime
        {
            get
            {
                if (Points == null) return null;

                if (!Points.Any()) return null;

                var min = Points.Min(point => point.Timestamp);

                var offset = new DateTimeOffset(Convert.ToInt64(min), TimeSpan.Zero);

                return offset;
            }
        }

        /// <summary>
        /// Update the bounds of the stroke
        /// </summary>
        public void UpdateBounds()
        {
            if (!Points.Any()) return;

            foreach (var point in Points)
            {
                var pointRect = ToRect(point);

                if (BoundingRect.IsEmpty)
                {
                    BoundingRect = pointRect;
                }
                else
                {
                    BoundingRect = Rectangle.Union(BoundingRect, pointRect);
                }
            }
        }


        /// <summary>
        /// Gets or sets the path
        /// </summary>
        [JsonIgnore]
        public SKPath Path { get; set; }

        /// <summary>
        /// Gets or sets the paint
        /// </summary>
        [JsonIgnore]
        public SKPaint Paint { get; set; }

        /// <summary>
        /// Gets or sets the bounds
        /// </summary>
        [JsonIgnore]
        public Rectangle BoundingRect { get; private set; } = new Rectangle();

        /// <summary>
        /// Dispose of the Path and Paint
        /// </summary>
        public void Dispose()
        {
            if (Path != null)
            {
                Path.Dispose();

                Path = null;
            }

            if (Paint != null)
            {
                Paint.Dispose();

                Paint = null;
            }

            if (Resources != null)
            {
                foreach (var item in Resources)
                {
                    item.Dispose();
                }

                Resources = null;
            }
        }

        /// <summary>
        /// Gets or sets the resources that the stroke manages
        /// </summary>
        internal IEnumerable<IDisposable> Resources
        {
            get => _resources;

            set
            {
                if (_resources != null)
                {
                    foreach (var item in _resources)
                    {
                        item.Dispose();
                    }

                }

                _resources = value;
            }
        }

        /// <summary>
        /// Gets the ink points
        /// </summary>
        /// <returns>a read-only list of the ink points</returns>
        public IReadOnlyList<XInkPoint> GetInkPoints()
        {
            return Points.ToList();
        }

        private Rectangle ToRect(XInkPoint point)
        {
            return new Rectangle(point.Position.X - (DrawingAttributes.Size / 2.0),
                                 point.Position.Y - (DrawingAttributes.Size / 2),
                                point.Position.X + (DrawingAttributes.Size / 2.0),
                                point.Position.Y + (DrawingAttributes.Size / 2));
        }

        internal void AddRange(IEnumerable<XInkPoint> inkPoints)
        {
            lock (Points)
            {
                Points.Clear();
                var bounds = Rectangle.Zero;

                foreach (var item in inkPoints)
                {
                    Points.Add(item);
                }

                foreach (var point in Points)
                {
                    var pointRect = ToRect(point);

                    if (bounds.IsEmpty)
                    {
                        bounds = pointRect;
                    }
                    else
                    {
                        bounds = Rectangle.Union(bounds, pointRect);
                    }

                }

                BoundingRect = bounds;
            }
        }
        /// <summary>
        /// Add a point that is not too close to the last point, update the bounds, and invalidate the path 
        /// </summary>
        /// <param name="point">the point to add</param>
        internal void Add(XInkPoint point)
        {
            lock (Points)
            {
                // Don't add an identical point
                if (Points.Any() && point.Position == Points.Last().Position)
                {
                    Points.Last().Pressure = point.Pressure;

                    return;
                }
                var pointRect = ToRect(point);

                //if (Points.Any())
                //{
                //    var lastPoint = Points.Last();

                //    var lastPointRect = new SKRect(
                //        (float)(lastPoint.X - (Width / 2.0)), 
                //        (float)(lastPoint.Y - (Width / 2)), 
                //        (float)(lastPoint.X + (Width / 2.0)),
                //        (float)(lastPoint.Y + (Width / 2)));

                //    if (lastPointRect.IntersectsWith(pointRect))
                //    {
                //        return;
                //    }
                //}

                if (BoundingRect.IsEmpty)
                {
                    BoundingRect = pointRect;
                }
                else
                {
                    BoundingRect = Rectangle.Union(BoundingRect, pointRect);
                }

                if (Path != null)
                {
                    Path.Dispose();
                    Path = null;
                }

                Points.Add(point);
            }
        }
    }
}
