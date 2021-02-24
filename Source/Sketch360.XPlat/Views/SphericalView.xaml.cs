// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Inking.Support;

namespace Sketch360.XPlat.Views
{
    /// <summary>
    /// Spherical view
    /// </summary>
    public partial class SphericalView : ContentView
    {
        private double _zoomLevel;
        private bool _navigated;

        /// <summary>
        /// Initializes a new instance of the SphericalView class.
        /// </summary>
        public SphericalView()
        {
            InitializeComponent();

            string url = "spherical.html";
            if (Device.RuntimePlatform != Device.iOS)
                url = $"spherical.html?viewMode=None&lang={CultureInfo.CurrentCulture.TwoLetterISOLanguageName}";

            var source = new UrlWebViewSource
            {
                Url = DependencyService.Get<IBaseUrl>().GetBase() + url
            };

            Preview.Navigated += Preview_Navigated;
            Preview.Source = source;

            //Xamarin.Forms.DualScreen.DualScreenInfo.Current.HingeAngleChanged += Current_HingeAngleChanged;
        }


        /// <summary>
        /// Gets or sets a value indicating whether the hinge can be used for zooming the 360 view.
        /// </summary>
        public bool IsHingeZoomingEnabled { get; internal set; }

        private void Current_HingeAngleChanged(object sender, Xamarin.Forms.DualScreen.HingeAngleChangedEventArgs e)
        {
            if (Xamarin.Forms.DualScreen.DualScreenInfo.Current.SpanMode == Xamarin.Forms.DualScreen.TwoPaneViewMode.Wide && IsHingeZoomingEnabled)
            {
                var span = 30;
                var minimum = 180 - (span / 2.0);
                // the pressure sensor simulates hinge sensor changes with values from 0 to 1100.
                // we are only interested in 180 degrees +- 15 degrees
                var maximum = 180 + (span / 2.0);

                var zoomRange = 1.9 - 0.1;

                var anglePercentage = (Convert.ToDouble(e.HingeAngleInDegrees) - minimum) / (maximum - minimum);

                var zoomLevel = 0.1 + (zoomRange * anglePercentage);
                //var range = 0.1 to 1.9; 

                if (_zoomLevel != zoomLevel)
                {
                    SetZoomLevel(zoomLevel);

                    _zoomLevel = zoomLevel;
                }
            }
        }

        /// <summary>
        /// Sets the position
        /// </summary>
        /// <param name="alpha">horizontal position</param>
        /// <param name="beta">vertical position</param>
        public void SetPosition(double alpha, double beta)
        {
            Preview.Eval($"setPosition2({alpha},{beta});");
        }

        /// <summary>
        /// Gets the current zoom level
        /// </summary>
        /// <returns>an async task with the zoom level</returns>
        public async Task<double> GetZoomLevelAsync()
        {
            var value = await Preview.EvaluateJavaScriptAsync("getZoomLevel();").ConfigureAwait(true);

            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Sets the zoom level
        /// </summary>
        /// <param name="value">the zoom level %</param>
        public void SetZoomLevel(double value)
        {
            Preview.Eval($"setZoomLevel({value});");
        }


        /// <summary>
        /// Updates the web view with the current sketch data
        /// </summary>
        /// <returns>an async task</returns>
        public async Task UpdateWebViewAsync()
        {
            if (!_navigated) return;

            var sketchData = App.GetSketchData();

            if (sketchData == null) return;

            await Task.Run(delegate
            {
                var base64 = InkRenderer.RenderImage(sketchData.Width, sketchData.Height, sketchData.InkStrokes, sketchData.BackgroundColor, 1024);

                var script = $"imageUpdated('{base64}');";

                if (Dispatcher == null) return;

                Dispatcher.BeginInvokeOnMainThread(async delegate
                {
                    try
                    {
                        if (Preview == null) return;

                        await Preview.EvaluateJavaScriptAsync(script).ConfigureAwait(false);
                        // There is a limit on the length of strings for the Preview.Eval() method.
                    }
                    catch (Exception e)
                    {
                        var properties = new Dictionary<string, string>
                        {
                            ["ScriptLength"] = script.Length.ToString(CultureInfo.InvariantCulture)
                        };

                        Crashes.TrackError(e, properties);
                    }
                });

                GC.Collect();

            }).ConfigureAwait(false);
        }
        private async void Preview_Navigated(object sender, WebNavigatedEventArgs e)
        {
            await UpdateWebViewAsync().ConfigureAwait(false);
        }

        private async void OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            _navigated = true;

            await UpdateWebViewAsync().ConfigureAwait(false);
        }
    }
}