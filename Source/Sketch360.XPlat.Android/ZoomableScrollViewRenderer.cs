using Android.Content;
using Android.Widget;
using Sketch360.XPlat;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(
    typeof(ZoomableScrollView),
    typeof(Sketch360.XPlat.Droid.ZoomableScrollViewerRenderer))]

namespace Sketch360.XPlat.Droid
{
    /// <summary>
    /// ZoomableScrollView UWP Renderer
    /// </summary>
    public class ZoomableScrollViewerRenderer : ScrollViewRenderer
    {
        public ZoomableScrollViewerRenderer(Context context) :
            base(context)
        {

        }
        #region Methods
        /// <summary>
        /// On element changed
        /// </summary>
        /// <param name="e">the element changed event arguments</param>
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                e.OldElement.PropertyChanged -= NewElement_PropertyChanged;
            }

            if (e.NewElement != null)
            {
                if (e.NewElement is IZoomableScrollView zoomable)
                {
                    e.NewElement.PropertyChanged += NewElement_PropertyChanged;

                    UpdateZoomMode(zoomable);
                }
            }
        }
        #endregion

        #region Implementation
        private void NewElement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IZoomableScrollView.IsZoomEnabled))
            {
                UpdateZoomMode(sender as IZoomableScrollView);
            }
        }

        private void UpdateZoomMode(IZoomableScrollView zoomableScrollView)
        {
            if (zoomableScrollView.IsZoomEnabled)
            {
            }
            else
            {
            }
        }
        #endregion
    }
}
