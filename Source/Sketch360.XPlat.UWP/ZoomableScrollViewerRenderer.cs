// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat;
using System;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;


[assembly: ExportRenderer(
    typeof(ZoomableScrollView),
    typeof(Sketch360.XPlat.UWP.ZoomableScrollViewerRenderer))]

namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// ZoomableScrollView UWP Renderer
    /// </summary>
    public class ZoomableScrollViewerRenderer : ScrollViewRenderer
    {
        #region Methods
        /// <summary>
        /// On element changed
        /// </summary>
        /// <param name="e">the element changed event arguments</param>
        protected override void OnElementChanged(ElementChangedEventArgs<ScrollView> e)
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
                Control.ZoomMode = ZoomMode.Enabled;

                ScrollViewer.SetHorizontalScrollMode(Control, Windows.UI.Xaml.Controls.ScrollMode.Auto);
                ScrollViewer.SetVerticalScrollMode(Control, Windows.UI.Xaml.Controls.ScrollMode.Auto);
                Control.VerticalScrollBarVisibility = Windows.UI.Xaml.Controls.ScrollBarVisibility.Auto;
                Control.HorizontalScrollBarVisibility = Windows.UI.Xaml.Controls.ScrollBarVisibility.Auto;

                Control.MinZoomFactor = 0.1f;
                Control.MaxZoomFactor = 20.0f;
            }
            else
            {
                Control.ZoomMode = ZoomMode.Disabled;
            }
        }
        #endregion
    }
}
