// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Inking.Support
{
    /// <summary>
    /// SkiSharp <see cref="SKCanvas"/> extensions for <see cref="XInkStroke"/> drawing
    /// </summary>
    public static class SKCanvasExtensions
    {
        /// <summary>
        /// Draw the ink strokes that are within the bounds
        /// </summary>
        /// <param name="canvas">the SkiaSharp canvas</param>
        /// <param name="strokes">the candidate strokes</param>
        /// <param name="bounds">the bounds to draw in</param>
        public static void Draw(this SKCanvas canvas, IEnumerable<XInkStroke> strokes, SKRect bounds)
        {
            if (strokes == null) return;

            foreach (var stroke in strokes)
            {
                if (!bounds.IntersectsWith(stroke.BoundingRect.ToSKRect()))
                    continue;

                canvas.Draw(stroke, true);
            }
        }

        /// <summary>
        /// Draw strokes
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="strokes"></param>
        public static void Draw(this SKCanvas canvas, IEnumerable<XInkStroke> strokes)
        {
            if (strokes == null) return;

            foreach (var stroke in strokes)
            {
                canvas.Draw(stroke, true);
            }
        }

        /// <summary>
        /// Draw an ink stroke
        /// </summary>
        /// <param name="canvas">the canvas</param>
        /// <param name="stroke">the stroke</param>
        /// <param name="useCache">true to use cached data</param>
        public static void Draw(this SKCanvas canvas, XInkStroke stroke, bool useCache)
        {
            _ = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _ = stroke ?? throw new ArgumentNullException(nameof(stroke));

            var inkPoints = stroke.GetInkPoints();

            if (useCache)
            {
                if (inkPoints.Count == 1)
                {
                    if (stroke.Paint == null)
                    {
                        var paintResult = stroke.CreatePaint(SKPaintStyle.Fill);

                        stroke.Paint = paintResult.Item1;

                        stroke.Resources = paintResult.Item2;
                    }

                    canvas.DrawCircle(
                        stroke.GetInkPoints()[0].Position.ToSKPoint(),
                        stroke.DrawingAttributes.Size / 4.0f,
                        stroke.Paint);
                }
                else
                {
                    stroke.Path ??= stroke.DrawingAttributes.IgnorePressure
                        ? stroke.CreatePath()
                        : stroke.CreateVariableThicknessPath();

                    if (stroke.Paint == null)
                    {
                        var paintResult = stroke.CreatePaint();
                        stroke.Resources = paintResult.Item2;
                        stroke.Paint = paintResult.Item1;
                    }

                    if (stroke.Path != null)
                    {
                        canvas.DrawPath(stroke.Path, stroke.Paint);
                    }
                }
            }
            else
            {
                if (inkPoints.Count == 1)
                {
                    var paintResult = stroke.CreatePaint(SKPaintStyle.Fill);

                    using (var paint = paintResult.Item1)
                    {
                        paint.IsStroke = false;

                        var radius = stroke.DrawingAttributes.Size / 4.0f;

                        //System.Diagnostics.Debug.WriteLine($"Drawing dot radius {radius}");
                        canvas.DrawCircle(
                            inkPoints[0].Position.ToSKPoint(),
                            radius,
                            paint);
                    }

                    if (paintResult.Item2 != null)
                    {
                        foreach (var item in paintResult.Item2)
                        {
                            item.Dispose();
                        }
                    }
                }
                else
                {
                    using var path = stroke.DrawingAttributes.IgnorePressure ? stroke.CreatePath() : stroke.CreateVariableThicknessPath();
                    if (path == null) return;

                    var paintResult = stroke.CreatePaint();

                    using (var paint = paintResult.Item1)
                    {
                        canvas.DrawPath(path, paint);
                    }

                    if (paintResult.Item2 != null)
                    {
                        foreach (var item in paintResult.Item2)
                        {
                            item.Dispose();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw a pen sample width
        /// </summary>
        /// <param name="canvas">the canvas</param>
        /// <param name="width">the pen width</param>
        /// <param name="info">the image info</param>
        public static void DrawPenSize(this SKCanvas canvas, float width, SKImageInfo info)
        {
            _ = canvas ?? throw new ArgumentNullException(nameof(canvas));

            using var paint = new SKPaint
            {
                StrokeWidth = width,
                StrokeCap = SKStrokeCap.Round,
                Style = SKPaintStyle.Stroke
            };

            using var path = new SKPath();
            path.MoveTo(16.0f, (float)(info.Height / 2.0));

            path.CubicTo(
                info.Width / 4.0f, -(info.Height - width),
                info.Width * 3.0f / 4.0f, info.Height * 2.0f - width,
                info.Width - 16.0f, info.Height / 2.0f);

            canvas.DrawPath(path, paint);
        }
    }
}
