// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xamarin.Forms.Inking.Support
{
    /// <summary>
    /// Ink renderer
    /// </summary>
    public static class InkRenderer
    {
        /// <summary>
        /// render an image of ink strokes
        /// </summary>
        /// <param name="width">the width of the image</param>
        /// <param name="height">the height of the image</param>
        /// <param name="backgroundColor">the background color</param>
        /// <param name="strokes">the strokes</param>
        /// <param name="stream">the image stream to write to</param>
        /// <param name="scaledWidth">the scaled width</param>
		public static void RenderImage(int width, int height, Color backgroundColor, IReadOnlyList<XInkStroke> strokes, Stream stream, int scaledWidth)
        {
            if (strokes == null) throw new ArgumentNullException(nameof(strokes));

            var scale = Convert.ToDouble(scaledWidth) / Convert.ToDouble(width);

            var bitmapWidth = Convert.ToDouble(width) * scale;

            var bitmapHeight = Convert.ToDouble(height) * scale;

            using var bitmap = new SKBitmap(Convert.ToInt32(bitmapWidth), Convert.ToInt32(bitmapHeight));
            using (var canvas = new SKCanvas(bitmap))
            {
                DrawBackground(backgroundColor, canvas, new SKRect(0, 0, width, height));

                canvas.Scale(Convert.ToSingle(scale), Convert.ToSingle(scale), 0, 0);

                foreach (var stroke in strokes)
                {
                    canvas.Draw(stroke, true);
                }
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
            data.SaveTo(stream);

            //using (var wStream = new SKManagedWStream(stream))
            //{
            //	var pixels = bitmap.PeekPixels();

            //	SKPixmap.Encode(wStream, pixels, SKJpegEncoderOptions.Default);
            //}
        }

        /// <summary>
        /// Draw a textured background
        /// </summary>
        /// <param name="backgroundColor">the backgroud color</param>
        /// <param name="canvas">the canvas</param>
        /// <param name="bounds">the bounds</param>
		public static void DrawBackground(Color backgroundColor, SKCanvas canvas, SKRect bounds)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));

            using var bitmap = new SKBitmap(128, 128);
            using (var bitmapCanvas = new SKCanvas(bitmap))
            {
                //bitmapCanvas.Clear(backgroundColor.ToSKColor());
                using var colorShader = SKShader.CreateColor(backgroundColor.ToSKColor());
                using var perlinShader = SKShader.CreatePerlinNoiseFractalNoise(0.8f, 0.8f, 1, 1, new SKPointI(64, 64));
                using var paint = new SKPaint
                {
                    Color = backgroundColor.ToSKColor(),
                    Shader = SKShader.CreateCompose(colorShader, perlinShader),
                    IsAntialias = true,
                    IsStroke = false
                };
                bitmapCanvas.DrawRect(new SKRect(0, 0, 128, 128), paint);
            }

            using var paint2 = new SKPaint
            {
                Shader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat),
                IsStroke = false,
                IsAntialias = true
            };
            canvas.Clear(Color.Transparent.ToSKColor());

            canvas.DrawRect(bounds, paint2);
        }

        /// <summary>
        /// render the ink as an image
        /// </summary>
        /// <param name="width">the image width</param>
        /// <param name="height">the image height</param>
        /// <param name="strokes">the strokes</param>
        /// <param name="backgroundColor">the background color</param>
        /// <param name="scaledWidth">the scaled width</param>
        /// <returns>a Base64 string</returns>
		public static string RenderImage(int width, int height, IEnumerable<XInkStroke> strokes, Color backgroundColor, int scaledWidth)
        {
            using var memStream = new MemoryStream();
            RenderImage(width, height, backgroundColor, strokes.ToList(), memStream, scaledWidth);

            var base64 = Convert.ToBase64String(memStream.ToArray());

            return base64;
        }

        /// <summary>
        /// create a path for the stroke
        /// </summary>
        /// <param name="stroke">the stroke</param>
        /// <returns>a new path</returns>
		public static SKPath CreatePath(this XInkStroke stroke)
        {
            if (stroke == null) throw new ArgumentNullException(nameof(stroke));

            var path = new SKPath();

            var inkPoints = stroke.GetInkPoints();

            var points = from point in inkPoints
                         let x = (float)point.Position.X
                         let y = (float)point.Position.Y
                         select new SKPoint(x, y);

            var count = points.Count();

            //path.AddPoly(points.ToArray(), false);

            if (count > 1)
            {
                var firstPoints = new Point[count - 1];
                var secondPoints = new Point[count - 1];

                var controlPoints = (from item in inkPoints
                                     select item.Position).ToArray();
                BezierSpline.GetCurveControlPoints(controlPoints, out firstPoints, out secondPoints);

                path.MoveTo(controlPoints.First().ToSKPoint());

                for (var i = 1; i < count; i++)
                {
                    path.CubicTo(firstPoints[i - 1].ToSKPoint(), secondPoints[i - 1].ToSKPoint(), points.ElementAt(i));
                }
            }

            return path;
        }

        /// <summary>
        /// create the path points
        /// </summary>
        /// <param name="stroke">the stroke</param>
        /// <returns>the path points</returns>
		public static SKPoint[] CreatePathPoints(XInkStroke stroke)
        {
            if (stroke == null) throw new ArgumentNullException(nameof(stroke));

            var inkPoints = stroke.GetInkPoints().ToArray();

            if (inkPoints.Length < 2)
            {
                return null;
            }
            var pointCount = inkPoints.Length * 2 + 2;

            var allPoints = new SKPoint[pointCount];

            double angle = 0.0;

            for (var i = 0; i < inkPoints.Length; i++)
            {
                var point1 = inkPoints[i];

                if (i < inkPoints.Length - 1)
                {
                    var point2 = inkPoints[i + 1];

                    var x = point2.Position.X - point1.Position.X;
                    var y = point2.Position.Y - point1.Position.Y;

                    angle = Math.Atan2(y, x) - Math.PI / 2;
                }

                var h = stroke.DrawingAttributes.Size * 0.5;

                if (!stroke.DrawingAttributes.IgnorePressure)
                {
                    h *= point1.Pressure;
                }

                var leftX = point1.Position.X + (Math.Cos(angle) * h);
                var leftY = point1.Position.Y + (Math.Sin(angle) * h);
                var rightX = point1.Position.X - (Math.Cos(angle) * h);
                var rightY = point1.Position.Y - (Math.Sin(angle) * h);

                allPoints[i + 1] = new Point(leftX, leftY).ToSKPoint();

                allPoints[pointCount - i - 1] = new Point(rightX, rightY).ToSKPoint();
            }

            allPoints[0] = GetModifiedFirstPoint(stroke, inkPoints).ToSKPoint();

            allPoints[inkPoints.Length + 1] = GetModifiedLastPoint(stroke, inkPoints).ToSKPoint();

            return allPoints;

        }

        /// <summary>
        /// Create a variable thickness path
        /// </summary>
        /// <param name="stroke">the stroke</param>
        /// <returns>a variable thickness path</returns>
		public static SKPath CreateVariableThicknessPath(this XInkStroke stroke)
        {
            var pathPoints = CreatePathPoints(stroke);

            if (pathPoints == null || pathPoints.Length == 0) return null;

            var newPath = new SKPath();

            newPath.AddPoly(pathPoints);

            return newPath;
        }

        private static Point GetModifiedFirstPoint(XInkStroke stroke, XInkPoint[] inkPoints)
        {
            var firstPoint = inkPoints[0];
            var secondPoint = inkPoints[1];

            var x1 = secondPoint.Position.X - firstPoint.Position.X;
            var y1 = secondPoint.Position.Y - firstPoint.Position.Y;

            var angle1 = Math.Atan2(y1, x1) + Math.PI;

            var h1 = stroke.DrawingAttributes.Size * 0.5;

            if (!stroke.DrawingAttributes.IgnorePressure)
            {
                h1 *= firstPoint.Pressure;
            }

            var lastX = firstPoint.Position.X + (Math.Cos(angle1) * h1);
            var lastY = firstPoint.Position.Y + (Math.Sin(angle1) * h1);

            var modifiedLastPoint = new Point(lastX, lastY);

            return modifiedLastPoint;
        }


        private static Point GetModifiedLastPoint(XInkStroke stroke, XInkPoint[] inkPoints)
        {
            var lastPoint = inkPoints[inkPoints.Length - 1];
            var secondLastPoint = inkPoints[inkPoints.Length - 2];

            var x1 = lastPoint.Position.X - secondLastPoint.Position.X;
            var y1 = lastPoint.Position.Y - secondLastPoint.Position.Y;

            var angle1 = Math.Atan2(y1, x1);

            var h1 = stroke.DrawingAttributes.Size * 0.5;

            if (!stroke.DrawingAttributes.IgnorePressure)
            {
                h1 *= lastPoint.Pressure;
            }

            var lastX = lastPoint.Position.X + (Math.Cos(angle1) * h1);
            var lastY = lastPoint.Position.Y + (Math.Sin(angle1) * h1);

            var modifiedLastPoint = new Point(lastX, lastY);

            return modifiedLastPoint;
        }


        /// <summary>
        /// Create a SkiaSharp paint object to paint the ink stroke 
        /// </summary>
        /// <param name="stroke">an ink stroke</param>
        /// <param name="paintStyle">the paint style (default is 
        /// <see cref="SKPaintStyle.Stroke"/>)</param>
        /// <param name="blendMode">the pencil drawing blend mode (default is 
        /// <see cref="SKBlendMode.SrcATop"/></param>
        /// <returns>a tuple with the<see cref="SKPaint"/> object and any 
        /// disposable objects that need to be managed</returns>
        public static Tuple<SKPaint, IEnumerable<IDisposable>> CreatePaint(this XInkStroke stroke, SKPaintStyle paintStyle = SKPaintStyle.Stroke, SKBlendMode blendMode = SKBlendMode.SrcATop)
        {
            if (stroke == null) throw new ArgumentNullException(nameof(stroke));

            var disposables = new List<IDisposable>();

            SKShader shader = null;
            if (stroke.DrawingAttributes.Kind == XInkDrawingAttributesKind.Pencil)
            {
                var perlin = SKShader.CreatePerlinNoiseFractalNoise(0.01f, 0.01f, 1, 1.0f);
                var color = SKShader.CreateColor(stroke.DrawingAttributes.Color.ToSKColor().WithAlpha(0x7F));

                disposables.Add(perlin);
                disposables.Add(color);

                shader = SKShader.CreateCompose(
                        perlin,
                        color,
                        blendMode);
            }

            Tuple<SKPaint, IEnumerable<IDisposable>> tuple = null;
            SKPaint paint = null;

            if (!stroke.DrawingAttributes.IgnorePressure)
            {
                paintStyle = SKPaintStyle.Fill;
            }

            try
            {
                paint = new SKPaint
                {
                    Color = stroke.DrawingAttributes.Kind == XInkDrawingAttributesKind.Default ? stroke.DrawingAttributes.Color.ToSKColor() : new SKColor(),
                    StrokeWidth = stroke.DrawingAttributes.IgnorePressure ? stroke.DrawingAttributes.Size : 0.0f,
                    Style = paintStyle,
                    IsAntialias = true,
                    StrokeCap = stroke.DrawingAttributes.PenTip == Inking.XPenTipShape.Circle ? SKStrokeCap.Round : SKStrokeCap.Butt,
                    PathEffect = SKPathEffect.CreateCorner(100)
                };

                if (shader != null)
                {
                    paint.Shader = shader;
                }

                tuple = Tuple.Create(paint, disposables as IEnumerable<IDisposable>);

                paint = null;
            }
            finally
            {
                paint?.Dispose();
            }

            return tuple;
        }
    }

    /// <summary>
    /// Bezier Spline methods
    /// </summary>
    /// From <![CDATA[https://www.codeproject.com/articles/31859/draw-a-smooth-curve-through-a-set-of-2d-points-wit]]>
    public static class BezierSpline
    {
        /// <summary>
        /// Get open-ended Bezier Spline Control Points.
        /// </summary>
        /// <param name="knots">Input Knot Bezier spline points.</param>
        /// <param name="firstControlPoints">Output First Control points
        /// array of knots.Length - 1 length.</param>
        /// <param name="secondControlPoints">Output Second Control points
        /// array of knots.Length - 1 length.</param>
        /// <exception cref="ArgumentNullException"><paramref name="knots"/>
        /// parameter must be not null.</exception>
        /// <exception cref="ArgumentException"><paramref name="knots"/>
        /// array must contain at least two points.</exception>
        public static void GetCurveControlPoints(Point[] knots,
            out Point[] firstControlPoints, out Point[] secondControlPoints)
        {
            if (knots == null)
                throw new ArgumentNullException(nameof(knots));
            int n = knots.Length - 1;
            if (n < 1)
                throw new ArgumentException
                ("At least two knot points required", nameof(knots));
            if (n == 1)
            { // Special case: Bezier curve should be a straight line.
                firstControlPoints = new Point[1];
                // 3P1 = 2P0 + P3
                firstControlPoints[0].X = (2 * knots[0].X + knots[1].X) / 3;
                firstControlPoints[0].Y = (2 * knots[0].Y + knots[1].Y) / 3;

                secondControlPoints = new Point[1];
                // P2 = 2P1 – P0
                secondControlPoints[0].X = 2 *
                    firstControlPoints[0].X - knots[0].X;
                secondControlPoints[0].Y = 2 *
                    firstControlPoints[0].Y - knots[0].Y;
                return;
            }

            // Calculate first Bezier control points
            // Right hand side vector
            double[] rhs = new double[n];

            // Set right hand side X values
            for (int i = 1; i < n - 1; ++i)
                rhs[i] = 4 * knots[i].X + 2 * knots[i + 1].X;
            rhs[0] = knots[0].X + 2 * knots[1].X;
            rhs[n - 1] = (8 * knots[n - 1].X + knots[n].X) / 2.0;
            // Get first control points X-values
            double[] x = GetFirstControlPoints(rhs);

            // Set right hand side Y values
            for (int i = 1; i < n - 1; ++i)
                rhs[i] = 4 * knots[i].Y + 2 * knots[i + 1].Y;
            rhs[0] = knots[0].Y + 2 * knots[1].Y;
            rhs[n - 1] = (8 * knots[n - 1].Y + knots[n].Y) / 2.0;
            // Get first control points Y-values
            double[] y = GetFirstControlPoints(rhs);

            // Fill output arrays.
            firstControlPoints = new Point[n];
            secondControlPoints = new Point[n];
            for (int i = 0; i < n; ++i)
            {
                // First control point
                firstControlPoints[i] = new Point(x[i], y[i]);
                // Second control point
                if (i < n - 1)
                    secondControlPoints[i] = new Point(2 * knots
                        [i + 1].X - x[i + 1], 2 *
                        knots[i + 1].Y - y[i + 1]);
                else
                    secondControlPoints[i] = new Point((knots
                        [n].X + x[n - 1]) / 2,
                        (knots[n].Y + y[n - 1]) / 2);
            }
        }

        /// <summary>
        /// Solves a tridiagonal system for one of coordinates (x or y)
        /// of first Bezier control points.
        /// </summary>
        /// <param name="rhs">Right hand side vector.</param>
        /// <returns>Solution vector.</returns>
        private static double[] GetFirstControlPoints(double[] rhs)
        {
            int n = rhs.Length;
            double[] x = new double[n]; // Solution vector.
            double[] tmp = new double[n]; // Temp workspace.

            double b = 2.0;
            x[0] = rhs[0] / b;
            for (int i = 1; i < n; i++) // Decomposition and forward substitution.
            {
                tmp[i] = 1 / b;
                b = (i < n - 1 ? 4.0 : 3.5) - tmp[i];
                x[i] = (rhs[i] - x[i - 1]) / b;
            }
            for (int i = 1; i < n; i++)
                x[n - i - 1] -= tmp[n - i] * x[n - i]; // Backsubstitution.

            return x;
        }
    }
}
