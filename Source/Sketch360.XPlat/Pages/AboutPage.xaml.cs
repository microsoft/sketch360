// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.Resources;
using System;
using System.Globalization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Sketch360.XPlat.Pages
{
    /// <summary>
    /// About page
    /// </summary>
    public partial class AboutPage : ContentPage
    {
        const string PrivacyUrl = "https://go.microsoft.com/fwlink/?LinkId=521839";
        const string GridByUrl = "https://aka.ms/AAancza";
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPage"/> class.
        /// </summary>
        public AboutPage()
        {
            InitializeComponent();

            VersionLabel.Text = string.Format(CultureInfo.CurrentCulture,
                AppResources.VersionFormat,
                VersionTracking.CurrentVersion);
        }

        protected override void OnAppearing()
        {
            Microsoft.AppCenter.Analytics.Analytics.TrackEvent("About page");

            base.OnAppearing();
        }

        private async void OnPrivacy(object sender, EventArgs e)
        {
            await Launcher.OpenAsync(
                new Uri(PrivacyUrl)).ConfigureAwait(false);
        }

        private async void OnGridBy(object sender, EventArgs e)
        {
            await Launcher.OpenAsync(
               new Uri(GridByUrl)).ConfigureAwait(false);
        }
    }
}