// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(Xamarin.Forms.WebView), typeof(Sketch360.XPlat.Droid.CustomWebViewRenderer))]

namespace Sketch360.XPlat.Droid
{
    /// <summary>
    /// Custom WebView renderer that allows local file URLs
    /// </summary>
    public class CustomWebViewRenderer : WebViewRenderer
    {
        public CustomWebViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
                Control.Settings.AllowUniversalAccessFromFileURLs = true;
        }
    }
}