// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AppCenter.Analytics;
using Sketch360.XPlat.Data;
using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace Sketch360.XPlat.Pages
{
    /// <summary>
    /// Settings page
    /// </summary>
    public partial class SettingsPage : ContentPage
    {
        public const string LandscapeDrawingRight = "LandscapeDrawingRight";
        public const string PortraitDrawingBottom = "PortraitDrawingBottom";
        public const string ExportHorizontalResolution = "ExportHorizontalResolution";
        public const string GridOpacity = "GridOpacity";

        private readonly List<ExportResolution> _resolutions = new List<ExportResolution>();

        public SettingsPage()
        {
            InitializeComponent();

            _resolutions.Add(new ExportResolution(2000, 1000));
            _resolutions.Add(new ExportResolution(2560, 1280));
            _resolutions.Add(new ExportResolution(3000, 1500));

            Resolutions.ItemsSource = _resolutions;

            LandscapeSwitch.IsToggled = Xamarin.Essentials.Preferences.Get(LandscapeDrawingRight, true);
            PortraitSwitch.IsToggled = Xamarin.Essentials.Preferences.Get(PortraitDrawingBottom, true);

            if (Xamarin.Essentials.Preferences.ContainsKey(ExportHorizontalResolution))
            {
                var resolution = Xamarin.Essentials.Preferences.Get(ExportHorizontalResolution, 0);

                Resolutions.SelectedItem = (from item in _resolutions
                                            where item.Width == resolution
                                            select item).FirstOrDefault();
            }

            if (Xamarin.Essentials.Preferences.ContainsKey(GridOpacity))
            {
                var opacity = Xamarin.Essentials.Preferences.Get(GridOpacity, 0d);
                GridOpacitySlider.Value = opacity;
            }
        }

        protected override void OnAppearing()
        {
            Analytics.TrackEvent("Settings page");

            base.OnAppearing();
        }

        private async void OnOk(object sender, EventArgs e)
        {
            if (App.Current.MainPage is Page page)
            {
                await page.Navigation.PopModalAsync(true).ConfigureAwait(false);
            }
        }

        private void OnLandscapeSwitchToggled(object sender, ToggledEventArgs e)
        {
            Xamarin.Essentials.Preferences.Set(LandscapeDrawingRight, LandscapeSwitch.IsToggled);
        }

        private void OnPortraintSwitchToggled(object sender, ToggledEventArgs e)
        {
            Xamarin.Essentials.Preferences.Set(PortraitDrawingBottom, PortraitSwitch.IsToggled);
        }

        private void OnResolutionSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is ExportResolution resolution)
            {
                Xamarin.Essentials.Preferences.Set(ExportHorizontalResolution, resolution.Width);
            }
        }

        private void OnOpacityChanged(object sender, ValueChangedEventArgs e)
        {
            Xamarin.Essentials.Preferences.Set(GridOpacity, e.NewValue / 100.0);
        }

        private async void OnLoadInitialSketch(object sender, EventArgs e)
        {
            var load = await DisplayAlert(
                Sketch360.XPlat.Resources.AppResources.LoadInitialSketch,
                Sketch360.XPlat.Resources.AppResources.LoadInitialSketchMessage,
                Sketch360.XPlat.Resources.AppResources.OK,
                Sketch360.XPlat.Resources.AppResources.Cancel).ConfigureAwait(true);

            if (load)
            {
                await (App.Current as App).LoadInitialSketchAsync().ConfigureAwait(true);

                await base.Navigation.PopAsync(true).ConfigureAwait(true);
            }
        }

        private void OnShowIntroduction(object sender, EventArgs e)
        {
            (App.Current as App).ShowCarouselPage();
        }
    }
}