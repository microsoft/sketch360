// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SkiaSharp;
using System;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Inking;
using Xamarin.Forms.Inking.Interfaces;

namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// Scrolling ink canvas
    /// </summary>
    public sealed partial class ScrollingInkCanvas : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the ScrollingInkCanvas class.
        /// </summary>
        public ScrollingInkCanvas()
        {
            this.InitializeComponent();

        }

        #region Properties
        /// <summary>
        /// Gets the UWP ink canvas
        /// </summary>
        public InkCanvas InkCanvas => InkCanvasControl;

        /// <summary>
        /// Gets or sets the Xamarin Forms ink canvas view
        /// </summary>
        public IInkCanvasView InkCanvasView { get; set; }
        #endregion

        /// <summary>
        /// View changed event handler
        /// </summary>
        public event EventHandler<ViewChangedEventArgs> ViewChanged;

        /// <summary>
        /// Invalidate the skiaSharp canvases
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the invalidate canvas event arguments</param>
        internal void Invalidated(object sender, InvalidateCanvasEventArgs e)
        {
            ForegroundCanvas.Invalidate();
            BackgroundCanvas.Invalidate();
        }
        #region Implementation

        private void OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                ViewChanged?.Invoke(this, new ViewChangedEventArgs(scrollViewer, e.IsIntermediate));
            }
        }
        #endregion

        private void OnPaintBackground(object sender, SkiaSharp.Views.UWP.SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear(SKColors.Transparent);
            InkCanvasView?.PaintBackground(e.Surface.Canvas);
        }

        private void OnPaintForeground(object sender, SkiaSharp.Views.UWP.SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear(SKColors.Transparent);

            InkCanvasView?.PaintForeground(e.Surface.Canvas);

        }
    }
}
