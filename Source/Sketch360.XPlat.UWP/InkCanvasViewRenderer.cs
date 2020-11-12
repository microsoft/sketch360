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
            }

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var scrollingInkCanvas = new ScrollingInkCanvas();

                    scrollingInkCanvas.InkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch | Windows.UI.Core.CoreInputDeviceTypes.Mouse;
                    scrollingInkCanvas.InkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
                    scrollingInkCanvas.InkCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;

                    SetNativeControl(scrollingInkCanvas);

                    if (e.NewElement.InkPresenter is UWPInkPresenter uwp)
                    {
                        uwp.InkCanvas = scrollingInkCanvas.InkCanvas;
                    }
                }
            }
        }
        #endregion

        #region Implementation

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
