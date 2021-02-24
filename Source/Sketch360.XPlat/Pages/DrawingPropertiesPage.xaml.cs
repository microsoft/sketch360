// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AppCenter.Analytics;
using Sketch360.XPlat.Views;
using Xamarin.Forms;

namespace Sketch360.XPlat.Pages
{
    /// <summary>
    /// Sketch properties page
    /// </summary>
    public partial class DrawingPropertiesPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the DrawingPropertiesPage class.
        /// </summary>
        public DrawingPropertiesPage()
        {
            InitializeComponent();

            ColorsView.ItemsSource = PaletteView.SystemPalette;
        }

        protected override void OnAppearing()
        {
            Analytics.TrackEvent("Drawing Properties page");

            base.OnAppearing();

            var sketchData = App.GetSketchData();

            ColorsView.SelectedItem = sketchData.BackgroundColor;

            SketchNameEntry.Text = sketchData.Name;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            var sketchData = App.GetSketchData();

            sketchData.Name = SketchNameEntry.Text;

            if (ColorsView.SelectedItem != null)
            {
                sketchData.BackgroundColor = (Color)ColorsView.SelectedItem;
            }
        }
    }
}