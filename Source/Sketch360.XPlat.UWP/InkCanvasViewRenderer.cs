// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xamarin.Forms.Inking.Views;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(
    typeof(InkCanvasView),
    typeof(Sketch360.XPlat.UWP.InkCanvasViewRenderer))]

namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// UWP InkCanvasView renderer using the native UWP InkCanvas
    /// </summary>
    public class InkCanvasViewRenderer : ViewRenderer<InkCanvasView, ScrollingInkCanvas>
    {
        #region Methods
        /// <summary>
        /// Create the UWP InkCanvas
        /// </summary>
        /// <param name="e">the element changed event arguments</param>
        protected override void OnElementChanged(ElementChangedEventArgs<InkCanvasView> e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                Control.InkCanvas.InkPresenter.StrokesCollected -= InkPresenter_StrokesCollected;
                Control.InkCanvas.InkPresenter.StrokesErased -= InkPresenter_StrokesErased;
                Control.ViewChanged -= ScrollingInkCanvas_ViewChanged;
            }

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var scrollingInkCanvas = new ScrollingInkCanvas();

                    scrollingInkCanvas.InkCanvas.InkPresenter.InputDeviceTypes = UWPInkPresenter.GetInputDeviceTypes(e.NewElement.InkPresenter.InputDeviceTypes);
                    scrollingInkCanvas.InkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
                    scrollingInkCanvas.InkCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
                    scrollingInkCanvas.ViewChanged += ScrollingInkCanvas_ViewChanged;

                    scrollingInkCanvas.InkCanvasView = e.NewElement;

                    e.NewElement.CanvasInvalidated += scrollingInkCanvas.Invalidated;

                    SetNativeControl(scrollingInkCanvas);

                    if (e.NewElement.InkPresenter is UWPInkPresenter uwp)
                    {
                        uwp.InkCanvas = scrollingInkCanvas.InkCanvas;
                    }
                }

                e.NewElement.IsControlEnabled = false;
            }
        }

        #endregion

        #region Implementation

        private void ScrollingInkCanvas_ViewChanged(object sender, ViewChangedEventArgs e)
        {
            Element.HorizontalOffset = e.ScrollViewer.HorizontalOffset;
            Element.VerticalOffset = e.ScrollViewer.VerticalOffset;
            Element.ZoomFactor = e.ScrollViewer.ZoomFactor;
        }


        private void InkPresenter_StrokesErased(Windows.UI.Input.Inking.InkPresenter sender, Windows.UI.Input.Inking.InkStrokesErasedEventArgs args)
        {
            var strokes = args.Strokes.ToXInkStrokeList();

            Element.InkPresenter.TriggerStrokesErased(strokes);
        }

        private void InkPresenter_StrokesCollected(Windows.UI.Input.Inking.InkPresenter sender, Windows.UI.Input.Inking.InkStrokesCollectedEventArgs args)
        {
            var strokes = args.Strokes.ToXInkStrokeList();

            Element.InkPresenter.TriggerStrokesCollected(strokes);
        }
        #endregion
    }
}
