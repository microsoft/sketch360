// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using Microsoft.AppCenter.Analytics;
using System;
using System.Globalization;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sketch360.XPlat.Pages
{
    /// <summary>
    /// Help page
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelpPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpPage"/> class.
        /// </summary>
        public HelpPage()
        {
            InitializeComponent();

            var lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToUpperInvariant();

            var localized = new[] { "DE", "FR" };

            var filename = "about.html";

            if (localized.Contains(lang))
            { 
                filename = string.Format("about.{0}.html", lang);
            }

            var source = new UrlWebViewSource
            {
                Url = DependencyService.Get<IBaseUrl>().GetBase() + filename
            };

            WebView.Source = source;
            WebView.Navigating += WebView_Navigating;
        }

        protected override void OnAppearing()
        {
            Analytics.TrackEvent("Help page");

            base.OnAppearing();
        }

        private void WebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (!e.Url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)) return;

            var options = new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.External,
                Flags = BrowserLaunchFlags.LaunchAdjacent
            };

            e.Cancel = true;

            _ = Browser.OpenAsync(e.Url, options);
        }
    }
}