// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.Core.Support;
using Sketch360.XPlat.Data;
using Sketch360.XPlat.Interfaces;
using Sketch360.XPlat.Views;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace Sketch360.XPlat.ViewModels
{
    /// <summary>
    /// DrawingView ViewModel
    /// </summary>
    public class DrawingViewViewModel : INotifyPropertyChanged
    {
        private double _horizontalOffset;
        private double _verticalOffset;
        private ISketchData _sketchData;
        private Color _currentColor = Color.Black;
        private ImageSource _currentColorSource;
        private float _currentPenSize = 4.0f;

        /// <summary>
        /// Initializes a new instance of the DrawingViewViewModel
        /// </summary>
        public DrawingViewViewModel()
        {
            var colors = (from item in PaletteView.SystemPalette
                          select new NamedColor
                          {
                              Color = item,
                              Name = item.ToString()
                          }).ToList();

            Colors = PaletteView.SystemPalette;

            PenSizes = PaletteView.SystemPenSizes;
        }

        public EquirectangularStencil EquirectangularStencil { get; set; }

        public ICommand StencilCommand { get; set; }

        /// <summary>
        /// Property changed event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the colors
        /// </summary>
        public IEnumerable<Color> Colors { get; }

        /// <summary>
        /// Gets the pen sizes
        /// </summary>
        public IEnumerable<float> PenSizes { get; }

        /// <summary>
        /// Gets or sets the current pen size
        /// </summary>
        public float CurrentPenSize
        {
            get => _currentPenSize;
            set
            {
                if (_currentPenSize != value)
                {
                    _currentPenSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color CurrentColor
        {
            get => _currentColor;
            set
            {
                if (_currentColor != value || CurrentColorSource == null)
                {
                    _currentColor = value;

                    OnPropertyChanged();

                    var bitmap = new SKBitmap(114, 108);

                    using (var canvas = new SKCanvas(bitmap))
                    {
                        canvas.Clear(SKColors.LightGray);

                        using var paint = new SKPaint
                        {
                            Color = _currentColor.ToSKColor()
                        };
                        canvas.DrawCircle(bitmap.Width / 2, bitmap.Height / 2, (bitmap.Width / 2) - 12, paint);
                    }

                    var source = (SKBitmapImageSource)bitmap;

                    CurrentColorSource = source;
                }
            }
        }

        public ImageSource CurrentColorSource
        {
            get => _currentColorSource;
            private set
            {
                _currentColorSource = value;

                OnPropertyChanged();
            }
        }

        public double VerticalOffset
        {
            get => _verticalOffset;
            set
            {
                _verticalOffset = value;

                OnPropertyChanged();
            }
        }

        public double HorizontalOffset
        {
            get => _horizontalOffset;

            set
            {
                _horizontalOffset = value;

                OnPropertyChanged();
            }
        }

        public ISketchData SketchData
        {
            get => _sketchData; set
            {
                if (_sketchData != value)
                {
                    _sketchData = value;



                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SketchData)));
                }
            }
        }

        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
