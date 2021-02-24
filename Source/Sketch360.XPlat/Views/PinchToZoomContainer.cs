// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Sketch360.XPlat.Views
{
    /// <summary>
    /// From https://docs.microsoft.com/en-us/samples/xamarin/xamarin-forms-samples/workingwithgestures-pinchgesture/
    /// </summary>
    public class PinchToZoomContainer : ContentView
    {
        private readonly PanGestureRecognizer _panRecognizer = new PanGestureRecognizer();
        private readonly PinchGestureRecognizer _pinchGestureRecognizer = new PinchGestureRecognizer();

        double currentScale = 1;
        double startScale = 1;
        double xOffset;
        double yOffset;
        private double _startX;
        private double _startY;

        public PinchToZoomContainer()
        {
            _pinchGestureRecognizer.PinchUpdated += OnPinchUpdated;
            _panRecognizer.PanUpdated += PanRecognizer_PanUpdated;
        }

        public bool IsTouchEnabled
        {
            get { return (bool)GetValue(IsTouchEnabledProperty); }
            set { SetValue(IsTouchEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTouchEnabled.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty IsTouchEnabledProperty =
            BindableProperty.Create(nameof(IsTouchEnabled), typeof(bool), typeof(PinchToZoomContainer), false, default, default, OnIsEnabledChanged);

        private static void OnIsEnabledChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PinchToZoomContainer container)
            {
                if (container.IsEnabled)
                {
                    container.GestureRecognizers.Add(container._panRecognizer);
                    container.GestureRecognizers.Add(container._pinchGestureRecognizer);
                }
                else
                {
                    container.GestureRecognizers.Remove(container._panRecognizer);
                    container.GestureRecognizers.Remove(container._pinchGestureRecognizer);

                }
            }
        }

        private void PanRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _startX = Content.TranslationX;
                    _startY = Content.TranslationY;

                    break;

                case GestureStatus.Running:
                    Content.TranslationX = _startX + e.TotalX;
                    Content.TranslationY = _startY + e.TotalY;
                    break;

                case GestureStatus.Completed:
                    xOffset = Content.TranslationX;
                    yOffset = Content.TranslationY;
                    break;
            }
        }


        void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                // Store the current scale factor applied to the wrapped user interface element,
                // and zero the components for the center point of the translate transform.
                startScale = Content.Scale;
                Content.AnchorX = 0;
                Content.AnchorY = 0;
            }
            if (e.Status == GestureStatus.Running)
            {
                // Calculate the scale factor to be applied.
                currentScale += (e.Scale - 1) * startScale;
                currentScale = Math.Max(0.25, currentScale);

                // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                // so get the X pixel coordinate.
                double renderedX = Content.X + xOffset;
                double deltaX = renderedX / Width;
                double deltaWidth = Width / (Content.Width * startScale);
                double originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                // so get the Y pixel coordinate.
                double renderedY = Content.Y + yOffset;
                double deltaY = renderedY / Height;
                double deltaHeight = Height / (Content.Height * startScale);
                double originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                // Calculate the transformed element pixel coordinates.
                double targetX = xOffset - (originX * Content.Width) * (currentScale - startScale);
                double targetY = yOffset - (originY * Content.Height) * (currentScale - startScale);

                // Apply translation based on the change in origin.
                Content.TranslationX = targetX.Clamp(-Content.Width * (currentScale - 1), 0);
                Content.TranslationY = targetY.Clamp(-Content.Height * (currentScale - 1), 0);

                // Apply scale factor
                Content.Scale = currentScale;
            }
            if (e.Status == GestureStatus.Completed)
            {
                // Store the translation delta's of the wrapped user interface element.
                xOffset = Content.TranslationX;
                yOffset = Content.TranslationY;
            }

            //if (Content is SKCanvasView view)
            //{
            //    //view.InvalidateSurface();
            //}
        }
    }
}
