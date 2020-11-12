// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Inking.Interfaces;
using Xamarin.Forms.Internals;

namespace Sketch360.XPlat.Modes
{
    /// <summary>
    /// Panning &amp; zooming mode
    /// </summary>
    /// Add a panning and pinch gesture recognizer to this https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/gestures/pan
    /// Trying sample at https://stackoverflow.com/questions/40181090/xamarin-forms-pinch-and-pan-together
    public class PanningMode : IPanningMode
    {
        private readonly PanGestureRecognizer _panRecognizer = new PanGestureRecognizer();
        private readonly PinchGestureRecognizer _pinchGestureRecognizer = new PinchGestureRecognizer();

        private double _startScale;
        private double _currentScale = 1.0;
        private double _xOffset;
        private double _yOffset;

        /// <summary>
        /// Initializes a new instance of the PanningMode class.
        /// </summary>
        public PanningMode()
        {
            _panRecognizer.PanUpdated += PanUpdated;
            _pinchGestureRecognizer.PinchUpdated += PinchUpdated;
        }

        /// <summary>
        /// Gets or sets the ink canvas
        /// </summary>
        public IInkCanvasView InkCanvasView { get; set; }

        public double MinZoomFactor { get; set; } = 0.5;

        public double MaxZoomFactor { get; set; } = 5;

        /// <summary>
        /// Activate the gesture recognizers
        /// </summary>
        public virtual void Activate()
        {
            if (InkCanvasView is View view)
            {
                view.GestureRecognizers.Add(_panRecognizer);
                view.GestureRecognizers.Add(_pinchGestureRecognizer);
            }
        }

        /// <summary>
        /// Deactivate the gesture recognizers
        /// </summary>
        public virtual void Deactivate()
        {
            if (InkCanvasView is View view)
            {
                view.GestureRecognizers.Remove(_panRecognizer);
                view.GestureRecognizers.Remove(_pinchGestureRecognizer);
            }
        }

        bool _startedWithZoom;

        private void PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _xOffset = InkCanvasView.HorizontalOffset;
                    _yOffset = InkCanvasView.VerticalOffset;
                    _startedWithZoom = false;
                    break;

                case GestureStatus.Running:
                    if (!_pinchGestureRecognizer.IsPinching && !_startedWithZoom)
                    {
                        var newX = _xOffset - (e.TotalX * InkCanvasView.PixelDensity);
                        var newY = _yOffset - (e.TotalY * InkCanvasView.PixelDensity);

                        var zoomFactor = InkCanvasView.ZoomFactor;

                        // allow the canvas to scroll off the screen, but only half way
                        var halfWidth = InkCanvasView.PixelWidth * 0.5;
                        var halfHeight = InkCanvasView.PixelHeight * 0.5;

                        var minX = -halfWidth;
                        var minY = -halfHeight;

                        var maxX = (InkCanvasView.CanvasWidth * zoomFactor) - halfWidth;
                        var maxY = (InkCanvasView.CanvasHeight * zoomFactor) - halfHeight;

                        InkCanvasView.HorizontalOffset = newX.Clamp(minX, maxX);
                        InkCanvasView.VerticalOffset = newY.Clamp(minY, maxY);
                    }
                    break;

                case GestureStatus.Completed:
                    break;
            }
        }

        private void PinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Started:
                    _startScale = InkCanvasView.ZoomFactor;
                    _currentScale = InkCanvasView.ZoomFactor;
                    _startedWithZoom = true;
                    break;

                case GestureStatus.Running:
                    // Calculate the scale factor to be applied.
                    _currentScale += (e.Scale - 1) * _startScale;
                    _currentScale = _currentScale.Clamp(MinZoomFactor, MaxZoomFactor);

                    // Apply scale factor
                    InkCanvasView.ZoomFactor = _currentScale;

                    _xOffset = InkCanvasView.HorizontalOffset;
                    _yOffset = InkCanvasView.VerticalOffset;
                    break;

                case GestureStatus.Completed:
                    break;
            }
        }
    }
}
