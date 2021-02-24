// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Inking.Support;

namespace Sketch360.XPlat.Views
{
    /// <summary>
    /// Palette view
    /// </summary>
    public partial class PaletteView : ContentView
    {
        public static IEnumerable<Color> SystemPalette
        {
            get
            {
                return new[]
                {
                    Color.Black,
                    Color.White,
                    Color.Beige, /// default background color
                    Color.FromHex("#ffd1d3d4"),
                    Color.FromHex("#ffa7a9ac"),
                    Color.FromHex("#ff808285"),
                    Color.FromHex("#ff58595b"),
                    Color.FromHex("#ffb31564"),
                    Color.FromHex("#ffe61b1b"),
                    Color.FromHex("#ffff5500"),
                    Color.FromHex("#ffffaa00"),
                    Color.FromHex("#ffffce00"),
                    Color.FromHex("#ffffe600"),
                    Color.FromHex("#ffa2e61b"),
                    Color.FromHex("#ff26e600"),
                    Color.FromHex("#ff008055"),
                    Color.FromHex("#ff00aacc"),
                    Color.FromHex("#ff004de6"),
                    Color.FromHex("#ff3d00b8"),
                    Color.FromHex("#ff6600cc"),
                    Color.FromHex("#ff600080"),
                    Color.FromHex("#fff7d7c4"),
                    Color.FromHex("#ffbb9167"),
                    Color.FromHex("#ff8e562e"),
                    Color.FromHex("#ff613d30"),
                    //Color.FromHex("#ffff80ff"),
                    Color.FromHex("#ffffc680"),
                    Color.FromHex("#ffffff80"),
                    Color.FromHex("#ff80ff9e"),
                    Color.FromHex("#ff80d6ff"),
                    Color.FromHex("#ffbcb3ff")
                };


            }
        }

        /// <summary>
        /// Gets or sets the pen size
        /// </summary>
        public float PenSize { get => (float)(PenSizes.SelectedItem is null ? 4.0f : PenSizes.SelectedItem); set => PenSizes.SelectedItem = value; }

        /// <summary>
        /// Gets or sets the pen color
        /// </summary>
        public Color PenColor { get => (Color)(ColorsView.SelectedItem is null ? Color.Black : ColorsView.SelectedItem); set => ColorsView.SelectedItem = value; }

        /// <summary>
        /// Gets the system pen sizes
        /// </summary>
        public static IEnumerable<float> SystemPenSizes => new[] { 1.0f, 2.0f, 4.0f, 8.0f, 16.0f, 32.0f, 64.0f, 128.0f };

        public PaletteView()
        {
            InitializeComponent();

            //var staticColors = typeof(Color).GetFields();
            //var colors = from item in staticColors
            //             let color = (Color)item.GetValue(null)
            //             orderby color.Hue, color.Luminosity, color.Saturation
            //             select color;


            ColorsView.ItemsSource = SystemPalette;


            PenSizes.ItemsSource = SystemPenSizes;

            ColorsView.SelectedItem = SystemPalette.First();
            PenSizes.SelectedItem = 4.0;
        }

        public event EventHandler<PaletteChangedEventArgs> PaletteChanged;

        private void OnOK(object sender, EventArgs e)
        {
            IsVisible = false;

            PaletteChanged?.Invoke(this, new PaletteChangedEventArgs(PenColor, PenSize));
        }

        private void OnDrawPenSize(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            if (sender is SKCanvasView view)
            {
                if (view.BindingContext is float width)
                {
                    e.Surface.Canvas.DrawPenSize(width, e.Info);
                }
            }
        }

        private void OnColorChanged(object sender, SelectionChangedEventArgs e)
        {
            PaletteChanged?.Invoke(this, new PaletteChangedEventArgs(PenColor, PenSize));
        }

        private void OnPenSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            PaletteChanged?.Invoke(this, new PaletteChangedEventArgs(PenColor, PenSize));

        }
    }
}