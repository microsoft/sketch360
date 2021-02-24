// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.Core.Support;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Inking.Interfaces;

namespace Sketch360.XPlat.Support
{
    /// <summary>
    /// Two-point stencil
    /// </summary>
    public class TwoPointStencil
    {
        #region Fields
        private const float Radius = 60f;
        private int _touchingPoint;
        private Point _startPoint;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the first point
        /// </summary>
        public Point Point1 { get; set; } = new Point(100, 100);

        /// <summary>
        /// Gets or sets the second point
        /// </summary>
        public Point Point2 { get; set; } = new Point(335, 284);
        #endregion

        #region Methods
        /// <summary>
        /// Gets a vertex
        /// </summary>
        /// <param name="canvasHeight">the canvas height</param>
        /// <param name="isFirst">true to get Point1, false for Point2</param>
        /// <returns>the vertex</returns>
        public Vertex GetVertex(double canvasHeight, bool isFirst)
        {
            if (canvasHeight <= 0) throw new ArgumentOutOfRangeException(nameof(canvasHeight), $"canvasHeight cannot be less than or equal to 0: {canvasHeight}");

            if (isFirst)
            {
                return Vertex.CreateVertex(Point1, canvasHeight);
            }
            else
            {
                return Vertex.CreateVertex(Point2, canvasHeight);
            }
        }

        /// <summary>
        /// Gets the apex
        /// </summary>
        /// <param name="canvasHeight">the canvas height</param>
        /// <returns></returns>
        public Vertex GetApex(double canvasHeight)
        {
            if (canvasHeight <= 0) throw new ArgumentOutOfRangeException(nameof(canvasHeight), $"canvasHeight cannot be less than or equal to 0: {canvasHeight}");

            return Equirectangular.ApexOf(GetVertex(canvasHeight, true),
                GetVertex(canvasHeight, false));
        }

        /// <summary>
        /// Draw the 2-point stencil drag points
        /// </summary>
        /// <param name="e">the canvas</param>
        /// <param name="zoomFactor">the zoom factor</param>
        public void Draw(SKCanvas e, float zoomFactor)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            if (zoomFactor <= 0) throw new ArgumentOutOfRangeException(nameof(zoomFactor), $"zoomFactor cannot be less than or equal to 0: {zoomFactor}.");

            using var paint2 = new SKPaint
            {
                Color = Color.Red.ToSKColor().WithAlpha(128),
                IsStroke = false,
            };

            e.DrawCircle(Point1.ToSKPoint(), Radius / zoomFactor, paint2);

            e.DrawCircle(Point2.ToSKPoint(), Radius / zoomFactor, paint2);
        }

        /// <summary>
        /// Handle touch events to move the stencil points
        /// </summary>
        /// <param name="inkCanvasView">the ink canvas view</param>
        /// <param name="stencil">the equirectangular stencil</param>
        /// <param name="e">the touch event arguments</param>
        internal void Touch(IInkCanvasView inkCanvasView, EquirectangularStencil stencil, SKTouchEventArgs e)
        {
            if (inkCanvasView == null) throw new ArgumentNullException(nameof(inkCanvasView));

            if (stencil == null || stencil.Mode != EquirectangularStencilMode.TwoPoint)
            {
                return;
            }

            if (e == null) throw new ArgumentNullException(nameof(e));

            var location = inkCanvasView.GetCanvasPosition(e.Location);

            var deltaX = location.X - _startPoint.X;

            var deltaY = location.Y - _startPoint.Y;

            if (e.InContact)
            {
                switch (e.ActionType)
                {
                    case SKTouchAction.Pressed:
                        if (Distance(location, Point1) < Radius / inkCanvasView.ZoomFactor)
                        {
                            _touchingPoint = 1;

                            _startPoint = location;

                            e.Handled = true;
                        }
                        else if (Distance(location, Point2) < Radius / inkCanvasView.ZoomFactor)
                        {
                            _touchingPoint = 2;

                            _startPoint = location;

                            e.Handled = true;
                        }
                        else
                        {
                            _touchingPoint = 0;
                        }
                        break;

                    case SKTouchAction.Moved:

                        switch (_touchingPoint)
                        {
                            case 1:
                                Point1 = Point1.Offset(deltaX, deltaY);
                                e.Handled = true;
                                UpdateApex(inkCanvasView, stencil);
                                break;

                            case 2:
                                Point2 = Point2.Offset(deltaX, deltaY);
                                e.Handled = true;
                                UpdateApex(inkCanvasView, stencil);
                                break;
                        }
                        _startPoint = location;
                        break;
                }
            }
            else if (e.ActionType == SKTouchAction.Released)
            {
                switch (_touchingPoint)
                {
                    case 1:
                        Point1 = Point1.Offset(deltaX, deltaY);
                        e.Handled = true;
                        UpdateApex(inkCanvasView, stencil);
                        break;

                    case 2:
                        Point2 = Point2.Offset(deltaX, deltaY);
                        e.Handled = true;
                        UpdateApex(inkCanvasView, stencil);
                        break;
                }

                _touchingPoint = 0;
            }
        }

        #endregion

        #region Implementation
        static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        private void UpdateApex(IInkCanvasView inkCanvasView, EquirectangularStencil stencil)
        {
            if (Point1 == Point2) return;

            stencil.Apex = GetApex(inkCanvasView.CanvasHeight);

            inkCanvasView.InvalidateCanvas(false, true);
        }
        #endregion
    }
}
