// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Sketch360.Core.Support;
using Sketch360.XPlat.Interfaces;
using Sketch360.XPlat.Modes;
using Sketch360.XPlat.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.DualScreen;
using Xamarin.Forms.Inking;
using Xamarin.Forms.Inking.Support;
using Xamarin.Forms.Inking.Views;

namespace Sketch360.XPlat.Views
{
    /// <summary>
    /// Drawing view
    /// </summary>
    public sealed partial class DrawingView : ContentView, IDisposable
    {
        private const double PeekWidth = 100.0;

        #region Fields

        /// <summary>
        /// The IsDrawingToolbarVisible dependency property (bool)
        /// </summary>
        public static readonly BindableProperty IsDrawingToolbarVisibleProperty = BindableProperty.Create(nameof(IsDrawingToolbarVisible),
            typeof(bool), typeof(DrawingView), false, default, default, OnDrawingToolbarVisibilyChanged);

        //private IHingeService _hingeService = DependencyService.Get<IHingeService>();
        private Size _canvasSize;
        private readonly DrawingViewViewModel _viewModel = new DrawingViewViewModel();
        private readonly Dictionary<Mode, IDrawingViewMode> _modes = new Dictionary<Mode, IDrawingViewMode>();
        private readonly IUndoManager _undoManager;
        private Mode _mode;
        //private string _modeName;
        //private readonly int _hingeAngle;
        //private int _hingeAngleStep = 5;
        //private readonly int _initialHingeAngle;
        private string _state;
        //private int _initialHingeIndex;
        private bool _isStencilMode;
        private SKBitmap _grid;
        private MemoryStream _imageStream;
        private SKBitmap _imageBitmap;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DrawingView class
        /// </summary>
        public DrawingView()
        {
            InitializeComponent();

            _undoManager = App.Container.Resolve<IUndoManager>();

            _modes[Mode.Drawing] = App.Container.Resolve<IDrawingMode>();
            _modes[Mode.Erasing] = App.Container.Resolve<IErasingMode>();
            _modes[Mode.Panning] = App.Container.Resolve<IPanningMode>();
            foreach (var pair in _modes)
            {
                pair.Value.InkCanvasView = InkCanvasView;
            }

            _viewModel.StencilCommand = new Command<string>(OnStencil);

            BindingContext = _viewModel;

            //InkCanvasView.SizeChanged += InkCanvasView_SizeChanged;
            //InkCanvasView.WetInkTouched += InkCanvasView_WetInkTouched;
            //MenuView.NewSketchCommand = new RelayCommand<object>(OnNewSketch);

            Mode = Mode.Panning;

            InkCanvasView.DefaultInkDrawingAttributesChanged += InkCanvasView_DefaultInkDrawingAttributesChanged;
            InkCanvasView.InkPresenter.InputDeviceTypes = XCoreInputDeviceTypes.Pen;

            LoadGrid();
        }

        public void SetDrawingToolbarPosition(bool onRight)
        {
            var stateName = onRight ? "RightToolbar" : "LeftToolbar";

            VisualStateManager.GoToState(LayoutRoot, stateName);
        }

        private async void OnStencil(string mode)
        {
            if (_viewModel.EquirectangularStencil == null)
            {
                _viewModel.EquirectangularStencil = await EquirectangularStencil.CreateAsync(InkCanvasView).ConfigureAwait(false);
            }

            switch (mode)
            {
                case "LeftRightLines":
                    _viewModel.EquirectangularStencil.Mode = EquirectangularStencilMode.LeftRightLines;
                    break;

                case "FrontBackLines":
                    _viewModel.EquirectangularStencil.Mode = EquirectangularStencilMode.FrontBackLines;
                    break;

                case "VerticalLines":
                    _viewModel.EquirectangularStencil.Mode = EquirectangularStencilMode.VerticalLines;
                    break;

                default:
                    _viewModel.EquirectangularStencil.Mode = EquirectangularStencilMode.None;
                    System.Diagnostics.Debug.WriteLine($"Unknown stencil mode: {mode}");
                    break;
            }

            VisualStateManager.GoToState(LayoutRoot, "Stencil" + mode);

            InkCanvasView.InvalidateCanvas(false, true);
        }

        /// <summary>
        /// Update the visual state when spanned/rotated
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        internal void UpdateOrientation(double width, double height)
        {
            var stateName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}",
                IsSpanned ? "Spanned" : "NotSpanned",
                width >= height ? "Landscape" : "Portrait");

            VisualStateManager.GoToState(LayoutRoot, stateName);
        }

        private void InkCanvasView_DefaultInkDrawingAttributesChanged(object sender, Xamarin.Forms.Inking.Support.TypedEventArgs<XInkDrawingAttributes> e)
        {
            if (e.Value.Color != null)
            {
                _viewModel.CurrentColor = e.Value.Color;

                //ColorPicker.ScrollTo(e.Value.Color, default, ScrollToPosition.Center);
            }

            _viewModel.CurrentPenSize = e.Value.Size;
        }
        #endregion

        #region Events
        //public event EventHandler ShowPalette;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether the drawing toolbar is visible
        /// </summary>
        public bool IsDrawingToolbarVisible
        {
            get
            {
                return (bool)GetValue(IsDrawingToolbarVisibleProperty);
            }
            set
            {
                SetValue(IsDrawingToolbarVisibleProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the drawing view mode
        /// </summary>
        public Mode Mode
        {
            get => _mode;
            set
            {
                _modes[_mode].Deactivate();
                _mode = value;
                _modes[_mode].Activate();
            }
        }

        public Size CanvasSize
        {
            get => _canvasSize;

            set
            {
                _canvasSize = value;

                OnPropertyChanged();

                InkCanvasView.CanvasWidth = _canvasSize.Width;
                InkCanvasView.CanvasHeight = _canvasSize.Height;
                //HorizontalScroller.Maximum = _canvasSize.Width;
            }
        }

        
        /// <summary>
        /// Gets or sets the image stream
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public void UpdateImageStream(MemoryStream imageStream)
        {
            if (_imageBitmap != null)
            {
                _imageBitmap.Dispose();

                _imageBitmap = null;
            }

            if (imageStream == null) return;

            SKBitmap newBitmap = null;
            try
            {
                newBitmap = SKBitmap.Decode(imageStream);

                _imageBitmap = newBitmap;

                newBitmap = null;
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
            }
            finally
            {
                newBitmap?.Dispose();
            }

            GC.Collect();
        }

        public InkCanvasView GetInkCanvasView()
        {
            return this.InkCanvasView;
        }

        #endregion

        #region Methods

        /// <summary>
        /// create a new sketch
        /// </summary>
        public void NewSketch()
        {
            var undoItem = App.Container.Resolve<IAddStrokesUndoItem>();
            undoItem.Container = App.GetSketchData();
            undoItem.Strokes = new List<XInkStroke>(InkCanvasView.InkPresenter.StrokeContainer.GetStrokes());
            undoItem.IsErase = true;

            _undoManager.Add(undoItem);

            InkCanvasView.InkPresenter.StrokeContainer.Clear();

            InkCanvasView.InvalidateCanvas(false, true);
        }

        #endregion

        #region Implementation

        private static void OnDrawingToolbarVisibilyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DrawingView view)
            {
                view.DrawingToolbarContainer.IsVisible = view.IsDrawingToolbarVisible;
            }
        }


        //private void OnHingeUpdated(object sender, HingeEventArgs e)
        //{
        //    _hingeAngle = e.Angle;

        //    return;

        //    //var delta = Convert.ToInt32(Math.Round(Convert.ToDouble(_initialHingeAngle - _hingeAngle) / _hingeAngleStep));

        //    //switch (_state)
        //    //{
        //    //    case "None":
        //    //        break;
        //    //    case "SelectingPenSize":
        //    //        {
        //    //            var newIndex = Math.Min(Math.Max(0, _initialHingeIndex + delta), PaletteView.SystemPenSizes.Length - 1);
        //    //            var newSize = PaletteView.SystemPenSizes[newIndex];
        //    //            var attributes = InkCanvasView.CopyDefaultDrawingAttributes();
        //    //            attributes.Size = newSize;
        //    //            InkCanvasView.UpdateDefaultDrawingAttributes(attributes);
        //    //        }
        //    //        break;
        //    //    case "SelectingColor":
        //    //        {
        //    //            var newIndex = Math.Min(Math.Max(0, _initialHingeIndex + delta), PaletteView.SystemPalette.Count() - 1);
        //    //            var newColor = PaletteView.SystemPalette.ElementAt(newIndex);
        //    //            var attributes = InkCanvasView.CopyDefaultDrawingAttributes();
        //    //            attributes.Color = newColor;
        //    //            InkCanvasView.UpdateDefaultDrawingAttributes(attributes);
        //    //        }
        //    //        break;
        //    //}

        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private static bool IsSpanned
        {
            get
            {
                try
                {
                    return Xamarin.Forms.DualScreen.DualScreenInfo.Current.SpanMode == TwoPaneViewMode.Tall || Xamarin.Forms.DualScreen.DualScreenInfo.Current.SpanMode == TwoPaneViewMode.Wide;
                }
                catch (Exception e)
                {
                    Crashes.TrackError(e);
                    return false;
                }
            }
        }

        private void OnInkCanvasSizeChanged(object sender, EventArgs e)
        {
            //var bounds = DualScreenInfo.Current.SpanningBounds;

            //// this means the screen is still adjusting to the span so just leave it alone
            //if (bounds.Length > 1 && bounds[1].Width < InkCanvasView.Width)
            //    return;

            //var newZoomFactor = InkCanvasView.PixelWidth / InkCanvasView.CanvasWidth;

            //InkCanvasView.ZoomFactor = newZoomFactor;
        }

        private void OpenColorPicker()
        {
            _state = "SelectingColor";
            PenSizesView.IsVisible = false;
            ColorPicker.IsVisible = true;
        }

        private void OnSelectColor(object sender, EventArgs e)
        {
            if (!ColorPicker.IsVisible)
                OpenColorPicker();
            else
                CloseColorPicker();

            //if (VisualStateManager.GoToState(LayoutRoot, _state))
            {
                //_initialHingeAngle = _hingeAngle;

                //await ColorPicker.TranslateTo(100, 0, default, Easing.SinOut);

                //var attributes = InkCanvasView.CopyDefaultDrawingAttributes();

                //_viewModel.CurrentColor = attributes.Color;

                //_initialHingeIndex = Math.Max(0, _viewModel.Colors.ToList().IndexOf(attributes.Color));


                //ColorsListView.SelectedItem = color;

                //ColorsListView.ScrollTo(color, ScrollToPosition.Center, true);
            }
        }

        private void OnReleasedToggle(object sender, EventArgs e)
        {
            //_state = "None";

            //VisualStateManager.GoToState(LayoutRoot, _state);
        }

        void OpenSelectPenSize()
        {
            ColorPicker.IsVisible = false;
            PenSizesView.IsVisible = true;
            _state = "SelectingPenSize";
        }

        void CloseColorPicker()
        {
            if (_state == "SelectingColor")
            {
                ColorPicker.IsVisible = false;
                _state = "None";
            }
        }

        void CloseSelectPenSize()
        {
            if (_state == "SelectingPenSize")
            {
                _state = "None";
                this.PenSizesView.IsVisible = false;
            }
        }

        private void OnSelectPenSize(object sender, EventArgs e)
        {
            //if (VisualStateManager.GoToState(LayoutRoot, _state))
            {
                if (!this.PenSizesView.IsVisible)
                    OpenSelectPenSize();
                else
                    CloseSelectPenSize();

                //_initialHingeAngle = _hingeAngle;
            }
        }

        private void OnColorSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_state == "SelectingColor")
            {
                CloseColorPicker();
                VisualStateManager.GoToState(LayoutRoot, _state);

                var attributes = InkCanvasView.InkPresenter.CopyDefaultDrawingAttributes();

                if (attributes.Color != _viewModel.CurrentColor)
                {
                    attributes.Color = _viewModel.CurrentColor;

                    InkCanvasView.InkPresenter.UpdateDefaultDrawingAttributes(attributes);

                    var properties = new Dictionary<string, string>
                    {
                        ["Color"] = _viewModel.CurrentColor.ToHex()
                    };

                    Analytics.TrackEvent("Pen color selected", properties);
                }
            }
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

        #endregion

        private void OnPenSizeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_state == "SelectingPenSize")
            {
                CloseSelectPenSize();

                VisualStateManager.GoToState(LayoutRoot, _state);

                var attributes = InkCanvasView.InkPresenter.CopyDefaultDrawingAttributes();

                if (attributes.Size != _viewModel.CurrentPenSize)
                {
                    attributes.Size = _viewModel.CurrentPenSize;

                    InkCanvasView.InkPresenter.UpdateDefaultDrawingAttributes(attributes);

                    var properties = new Dictionary<string, string>
                    {
                        ["Size"] = _viewModel.CurrentPenSize.ToString(CultureInfo.InvariantCulture)
                    };

                    Analytics.TrackEvent("Pen size selected", properties);
                }
            }
        }

        private async void OnStencil(object sender, EventArgs e)
        {
            _isStencilMode = !_isStencilMode;

            if (_isStencilMode)
            {
                if (_viewModel.EquirectangularStencil == null)
                {
                    _viewModel.EquirectangularStencil = await EquirectangularStencil.CreateAsync(InkCanvasView).ConfigureAwait(true);
                }

                _viewModel.EquirectangularStencil.Mode = EquirectangularStencilMode.LeftRightLines;

                var stateName = "Stencil" + _viewModel.EquirectangularStencil.Mode.ToString();

                VisualStateManager.GoToState(LayoutRoot, stateName);

            }
            else
            {
                VisualStateManager.GoToState(LayoutRoot, "StencilNone");

                if (_viewModel.EquirectangularStencil != null)
                {
                    _viewModel.EquirectangularStencil.Mode = EquirectangularStencilMode.None;
                }
            }

            InkCanvasView.InvalidateCanvas(false, true);

            var properties = new Dictionary<string, string>
            {
                ["Enabled"] = _isStencilMode.ToString(CultureInfo.InvariantCulture)
            };

            Analytics.TrackEvent("Enable Stencils", properties);
        }

        private void LoadGrid()
        {
            string resourceID = "Sketch360.XPlat.Assets.Equirectangular Grid.png";

            var assembly = GetType().GetTypeInfo().Assembly;

            using var stream = assembly.GetManifestResourceStream(resourceID);
            _grid = SKBitmap.Decode(stream);
        }

        private SKPaint _backgroundPaint;
        private SKBitmap _backgroundBitmap;
        private Color _backgroundColor;

        public void DrawBackground(Color backgroundColor, SKCanvas canvas, SKRect bounds)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));

            if (backgroundColor != _backgroundColor)
            {
                _backgroundPaint?.Dispose();
                _backgroundBitmap.Dispose();
                _backgroundPaint = null;
                _backgroundBitmap = null;

                _backgroundColor = backgroundColor;
            }

            if (_backgroundPaint == null)
            {
                _backgroundBitmap = new SKBitmap(128, 128);

                using (var bitmapCanvas = new SKCanvas(_backgroundBitmap))
                {
                    //bitmapCanvas.Clear(backgroundColor.ToSKColor());
                    using var colorShader = SKShader.CreateColor(backgroundColor.ToSKColor());
                    using var perlinShader = SKShader.CreatePerlinNoiseFractalNoise(0.8f, 0.8f, 1, 1, new SKPointI(64, 64));
                    using var paint = new SKPaint
                    {
                        Color = backgroundColor.ToSKColor(),
                        Shader = SKShader.CreateCompose(colorShader, perlinShader),
                        IsAntialias = true,
                        IsStroke = false
                    };
                    bitmapCanvas.DrawRect(new SKRect(0, 0, 128, 128), paint);
                }

                _backgroundPaint = new SKPaint
                {
                    Shader = SKShader.CreateBitmap(_backgroundBitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat),
                    IsStroke = false,
                    IsAntialias = true
                };
            }

            canvas.Clear(Color.Transparent.ToSKColor());

            canvas.DrawRect(bounds, _backgroundPaint);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1801", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private void OnBeforePaint(object sender_, SKCanvas canvas)
        {
            try
            {
                InkRenderer.DrawBackground(InkCanvasView.BackgroundColor, canvas, new SKRect(0, 0, Convert.ToSingle(InkCanvasView.CanvasWidth),
                    Convert.ToSingle(InkCanvasView.CanvasHeight)));

                var destRect = new SKRect(0, 0, Convert.ToSingle(InkCanvasView.CanvasWidth), Convert.ToSingle(InkCanvasView.CanvasHeight));

                using (var paint = new SKPaint
                {
                    Color = SKColors.Black.WithAlpha(50)
                })
                {
                    canvas.DrawBitmap(_grid, destRect, paint);
                }

                if (_imageBitmap != null)
                {
                    canvas.DrawBitmap(
                    _imageBitmap,
                    new SKRect(0, 0, (float)PeekWidth, (float)InkCanvasView.CanvasHeight),
                    new SKRect((float)InkCanvasView.CanvasWidth, 0, (float)(InkCanvasView.CanvasWidth + PeekWidth), (float)InkCanvasView.CanvasHeight));
                    canvas.DrawBitmap(

                    _imageBitmap,
                    new SKRect((float)(InkCanvasView.CanvasWidth - PeekWidth), 0, (float)InkCanvasView.CanvasWidth, (float)InkCanvasView.CanvasHeight),
                    new SKRect(-(float)PeekWidth, 0.0f, 0.0f, (float)InkCanvasView.CanvasHeight));
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
            }
        }

        /// <summary>
        /// Dispose of the grid
        /// </summary>
        public void Dispose()
        {
            if (_grid != null)
            {
                _grid.Dispose();

                _grid = null;
            }

            if (_imageStream != null)
            {
                _imageStream.Dispose();
                _imageStream = null;
            }

            if (_imageBitmap != null)
            {
                _imageBitmap.Dispose();

                _imageBitmap = null;
            }

            if (_backgroundBitmap != null)
            {
                _backgroundBitmap.Dispose();
                _backgroundBitmap = null;
            }

            if (_backgroundPaint != null)
            {
                _backgroundPaint.Dispose();
                _backgroundPaint = null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private void OnAfterPaint(object sender_, SKCanvas e)
        {
            if (_viewModel.EquirectangularStencil == null) return;

            switch (_viewModel.EquirectangularStencil.Mode)
            {
                case EquirectangularStencilMode.FrontBackLines:
                    DrawFrontBackLines(e);
                    break;

                case EquirectangularStencilMode.LeftRightLines:
                    DrawLeftRightLines(e);
                    break;

                case EquirectangularStencilMode.VerticalLines:
                    DrawVerticalLines(e);
                    break;
            }

        }

        private void DrawLeftRightLines(SKCanvas e)
        {
            e.Scale(5.0f);

            e.Scale(-1, 1);

            e.Translate(-100.0f, 0.0f);

            DrawStencilCurves(e, Color.Blue);

            e.Scale(-1, 1);

            DrawStencilCurves(e, Color.Blue);

            e.Translate(200.0f, 0.0f);

            e.Scale(-1, 1);

            DrawStencilCurves(e, Color.Blue);

            e.Scale(-1, 1);

            DrawStencilCurves(e, Color.Blue);

            e.Translate(-200.0f, 200.0f);

            e.Scale(-1, -1);

            DrawStencilCurves(e, Color.Blue);

            e.Scale(-1, 1);

            DrawStencilCurves(e, Color.Blue);

            e.Translate(200.0f, 0);

            e.Scale(-1, 1);

            DrawStencilCurves(e, Color.Blue);

            e.Scale(-1, 1);

            DrawStencilCurves(e, Color.Blue);
        }

        private void DrawStencilCurves(SKCanvas e, Color color)
        {
            foreach (var item in _viewModel.EquirectangularStencil.StencilCurves)
            {
                using var path = new SKPath();
                path.MoveTo(item.Point0.ToSKPoint());

                path.CubicTo(item.Point1.ToSKPoint(), item.Point2.ToSKPoint(), item.Point3.ToSKPoint());

                using var paint = new SKPaint
                {
                    Color = color.ToSKColor().WithAlpha(64),
                    IsStroke = true,
                    StrokeWidth = 1.0f,
                };
                e.DrawPath(path, paint);
            }
        }

        private void DrawFrontBackLines(SKCanvas e)
        {
            e.Scale(5.0f);

            DrawStencilCurves(e, Color.Red);

            e.Translate(200.0f, 0.0f);

            e.Scale(-1, 1);

            DrawStencilCurves(e, Color.Red);

            e.Scale(-1, 1);

            DrawStencilCurves(e, Color.Red);

            e.Translate(200.0f, 0.0f);

            e.Scale(-1, 1);

            DrawStencilCurves(e, Color.Red);

            e.Translate(400, 200);

            e.Scale(-1, -1);

            DrawStencilCurves(e, Color.Red);

            e.Translate(200, 0);

            e.Scale(-1, 1);

            DrawStencilCurves(e, Color.Red);

            e.Scale(-1, 1);

            DrawStencilCurves(e, Color.Red);

            e.Translate(200.0f, 0.0f);

            e.Scale(-1, 1);

            DrawStencilCurves(e, Color.Red);

        }

        private void DrawVerticalLines(SKCanvas e)
        {
            var step = Convert.ToSingle(InkCanvasView.CanvasWidth) / 36.0f;

            using var paint = new SKPaint
            {
                Color = Color.Green.ToSKColor().WithAlpha(127),
                IsStroke = true,
                StrokeWidth = 3.0f
            };
            for (float x = 0; x <= Convert.ToSingle(InkCanvasView.CanvasWidth); x += step)
            {
                e.DrawLine(x, 0, x, Convert.ToSingle(InkCanvasView.CanvasHeight), paint);
            }
        }
    }
}