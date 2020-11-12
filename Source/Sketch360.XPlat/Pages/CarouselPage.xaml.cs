// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using CarouselView.FormsPlugin.Abstractions;
using Microsoft.AppCenter.Analytics;
using Sketch360.XPlat.Interfaces;
using Sketch360.XPlat.Resources;
using System;
using Xamarin.Forms;

namespace Sketch360.XPlat
{
    public partial class CarouselPage : ContentPage, ICarouselPage
    {
        public CarouselPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            Analytics.TrackEvent("Carousel page");

            base.OnAppearing();

        }

        public async void OnSkipLabelClicked(object sender, EventArgs e)
        {
            var splitPage = App.Container.Resolve<ISplitPage>();
            if (splitPage is Page page)
            {
                Navigation.InsertPageBefore(page, Navigation.NavigationStack[0]);
                await Application.Current.MainPage.Navigation.PopAsync().ConfigureAwait(false);
            }
        }

        private void OnPositionSelected(object sender, PositionSelectedEventArgs e)
        {
            var carousel = (CarouselViewControl)sender;
            var array = (Array)carousel.ItemsSource;

            SkipLabel.Text = e.NewValue == (array.Length - 1)
                ? AppResources.CarouselStart
                : AppResources.CarouselSkip;
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            // workaround for a bug in the carousel when rotating the screen
            if (Device.RuntimePlatform == Device.Android)
            {
                Carousel.Orientation = CarouselViewOrientation.Vertical;
                Carousel.Orientation = CarouselViewOrientation.Horizontal;
            }
        }
    }
}
