// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SkiaSharp;
using System;
using System.Collections.Generic;
using Xamarin.Forms.Inking.Support;

namespace Xamarin.Forms.Inking.Interfaces
{
    /// <summary>
    /// Ink Canvas View Interface
    /// </summary>
    public interface IInkCanvasView
    {
        /// <summary>
        /// Zoom factor changed event
        /// </summary>
        event EventHandler<TypedEventArgs<double>> ZoomFactorChanged;

        /// <summary>
        /// Gets the zoom factor
        /// </summary>
        double ZoomFactor { get; set; }

        /// <summary>
        /// Gets or sets the horizontal offset
        /// </summary>
        double HorizontalOffset { get; set; }

        /// <summary>
        /// Gets or sets the wet ink strokes
        /// </summary>
        IDictionary<long, XInkStroke> WetStrokes { get; }

        /// <summary>
        /// Gets or sets the vertical offset
        /// </summary>
        double VerticalOffset { get; set; }

        /// <summary>
        /// Gets the ink presenter
        /// </summary>
        IInkPresenter InkPresenter { get; }

        /// <summary>
        /// Gets or sets the canvas width
        /// </summary>
        double CanvasWidth { get; set; }

        /// <summary>
        /// Gets or sets the canvas height
        /// </summary>
        double CanvasHeight { get; set; }

        /// <summary>
        /// Gets the pixel width
        /// </summary>
        double PixelWidth { get; }

        /// <summary>
        /// Gets the pixel height
        /// </summary>
        double PixelHeight { get; }

        /// <summary>
        /// Gets the pixel density
        /// </summary>
        double PixelDensity { get; }

        /// <summary>
        /// Invalidate the wet and dry ink canvases
        /// </summary>
        /// <param name="wetInk">true to invalidate the wet ink</param>
        /// <param name="dryInk">true to invalidate the dry ink</param>
        void InvalidateCanvas(bool wetInk, bool dryInk);

        /// <summary>
        /// Gets the canvas point
        /// </summary>
        /// <param name="location">the screen point</param>
        /// <returns>the canvas point</returns>
        Point GetCanvasPosition(SKPoint location);

        /// <summary>
        /// Paint the background
        /// </summary>
        /// <param name="canvas">the SkiaSharp canvas</param>
        void PaintBackground(SKCanvas canvas);

        /// <summary>
        /// Paint the foreground
        /// </summary>
        /// <param name="canvas">the SkiaSharp canvas</param>
        void PaintForeground(SKCanvas canvas);
    }
}
