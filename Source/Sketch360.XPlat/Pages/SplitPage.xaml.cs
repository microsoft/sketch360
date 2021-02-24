// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Sketch360.XPlat.Interfaces;
using Sketch360.XPlat.Modes;
using Sketch360.XPlat.Pages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.DualScreen;
using Xamarin.Forms.Inking;
using Xamarin.Forms.Inking.Support;

namespace Sketch360.XPlat
{
    /// <summary>
    /// Split page for Microsoft Surface Duo/Neo
    /// </summary>
    public partial class SplitPage : ContentPage, ISplitPage
    {
        private readonly ISplitPageViewModel _viewModel;
        private readonly DualScreenInfo _dualScreenInfo;
        private TwoPaneViewWideModeConfiguration wideModePreference;
        private TwoPaneViewTallModeConfiguration tallModePreference;

        public SplitPage(ISplitPageViewModel splitPageViewModel)
        {
            _viewModel = splitPageViewModel ?? throw new ArgumentNullException(nameof(splitPageViewModel));

            _viewModel.ActivateModeCommand = new Command<string>(ActivateMode);
            _viewModel.TouchDrawingCommand = new Command(OnTouchDrawing);

            _viewModel.UndoManager.Updated += UndoManagerUpdated;

            InitializeComponent();

            _viewModel.InkCanvasView = InkCanvas.GetInkCanvasView();

            MenuView.NewSketchCommand = new Command(OnNewSketch);
            MenuView.Page = this;
            BindingContext = _viewModel;
            _dualScreenInfo = new DualScreenInfo(ControlPanel);

            ActivateMode(Mode.Drawing);
        }

        private void OnDualScreenInfoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_dualScreenInfo.SpanningBounds.Length > 1)
            {
                FlexControls.WidthRequest = _dualScreenInfo.SpanningBounds[0].Width;
                sliderContainer.WidthRequest = _dualScreenInfo.SpanningBounds[1].Width - _dualScreenInfo.HingeBounds.Width;
                sliderContainer.Padding = new Thickness(_dualScreenInfo.HingeBounds.Width, 0, 0, 0);
            }
            else
            {
                FlexControls.WidthRequest = -1;
                sliderContainer.WidthRequest = -1;
                sliderContainer.Padding = new Thickness(0);
            }
            UpdateSize();
        }

        private async void OnNewSketch()
        {
            InkCanvas.NewSketch();

            await SaveSketchAsync().ConfigureAwait(true);

            await UpdateWebViewAsync().ConfigureAwait(false);

            Analytics.TrackEvent("New Sketch");
        }

        private static bool IsSpanned
        {
            get
            {
                try
                {
                    if (DualScreenInfo.Current == null) return false;
                    return DualScreenInfo.Current.SpanMode == TwoPaneViewMode.Tall || DualScreenInfo.Current.SpanMode == TwoPaneViewMode.Wide;
                }
                catch (Exception e)
                {
                    Crashes.TrackError(e);

                    return false;
                }
            }
        }

        private void UpdateTwoPaneView()
        {
            if (Xamarin.Essentials.Preferences.ContainsKey(SettingsPage.LandscapeDrawingRight))
            {
                bool isRight = Xamarin.Essentials.Preferences.Get(SettingsPage.LandscapeDrawingRight, false);
                InkCanvas.SetDrawingToolbarPosition(isRight);
            }
        }

        private async void InkStrokeContainer_StrokesErased(object sender, Xamarin.Forms.Inking.XInkStrokesErasedEventArgs e)
        {
            var undoItem = App.Container.Resolve<IAddStrokesUndoItem>();

            undoItem.Container = App.GetSketchData();
            undoItem.Strokes = e.Strokes.ToList();
            undoItem.IsErase = true;

            _viewModel.UndoManager.Add(undoItem);

            App.GetSketchData().InkStrokes = InkCanvas.GetInkCanvasView().InkPresenter.StrokeContainer.GetStrokes();

            await UpdateWebViewAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Update the sketch data
        /// </summary>
        /// <param name="sketchData">the sketch data</param>
        public async void SketchDataUpdated(ISketchData sketchData)
        {
            _ = sketchData ?? throw new ArgumentNullException(nameof(sketchData));

            // Title = sketchData.Name;

            var inkCanvasView = InkCanvas.GetInkCanvasView();
            Dispatcher.BeginInvokeOnMainThread(delegate
            {
                inkCanvasView.BackgroundColor = sketchData.BackgroundColor;
                InkCanvas.CanvasSize = new Size(sketchData.Width, sketchData.Height);
                InkCanvas.GetInkCanvasView().InkPresenter.StrokeContainer.Clear();
                InkCanvas.GetInkCanvasView().InkPresenter.StrokeContainer.Add(sketchData.InkStrokes);

                inkCanvasView.InvalidateCanvas(false, true);
            });

            _viewModel.UndoManager.Reset();

            await UpdateWebViewAsync().ConfigureAwait(false);
        }

        private async void UndoManagerUpdated(object sender, EventArgs e)
        {
            InkCanvas.GetInkCanvasView().InkPresenter.StrokeContainer.SetRange(App.GetSketchData().InkStrokes);

            InkCanvas.GetInkCanvasView().InvalidateCanvas(false, true);

            await SaveSketchAsync().ConfigureAwait(false);

            await UpdateWebViewAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// detach the events when disappering
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var inkCanvasView = InkCanvas.GetInkCanvasView();
            inkCanvasView.InkPresenter.StrokesCollected -= OnStrokesCollected;
            inkCanvasView.InkPresenter.StrokesErased -= InkStrokeContainer_StrokesErased;
            DualScreenInfo.Current.PropertyChanged -= FormsWindow_PropertyChanged;
            _dualScreenInfo.PropertyChanged -= OnDualScreenInfoPropertyChanged;
        }



        /// <summary>
        /// Attach events and initialize the sketch data when appearing
        /// </summary>
        protected override async void OnAppearing()
        {
            Analytics.TrackEvent("Split page");

            base.OnAppearing();

            MenuContainerPane1.InputTransparent = true;
            MenuViewRootLayout.InputTransparent = true;

            if (Xamarin.Essentials.Preferences.Get(SettingsPage.LandscapeDrawingRight, false))
                WideModePreference = TwoPaneViewWideModeConfiguration.RightLeft;
            else
                WideModePreference = TwoPaneViewWideModeConfiguration.LeftRight;

            if (Xamarin.Essentials.Preferences.Get(SettingsPage.PortraitDrawingBottom, false))
                TallModePreference = TwoPaneViewTallModeConfiguration.BottomTop;
            else
                TallModePreference = TwoPaneViewTallModeConfiguration.TopBottom;

            UpdateSize();

            DualScreenInfo.Current.PropertyChanged += FormsWindow_PropertyChanged;
            _dualScreenInfo.PropertyChanged += OnDualScreenInfoPropertyChanged;
            var sketchData = App.GetSketchData();

            if (sketchData != null)
            {
                Title = sketchData.Name;
            }

            var inkCanvasView = InkCanvas.GetInkCanvasView();

            if (sketchData != null)
            {
                inkCanvasView.BackgroundColor = sketchData.BackgroundColor;
                inkCanvasView.InkPresenter.StrokeContainer.Clear();
                inkCanvasView.InkPresenter.StrokeContainer.Add(sketchData.InkStrokes);
            }

            inkCanvasView.InkPresenter.StrokesCollected += OnStrokesCollected;
            inkCanvasView.InkPresenter.StrokesErased += InkStrokeContainer_StrokesErased;

            UpdateTwoPaneView();

            await UpdateWebViewAsync().ConfigureAwait(false);

            if (InkCanvas.GetInkCanvasView().InkPresenter.InputDeviceTypes.HasFlag(XCoreInputDeviceTypes.Touch))
            {
                TouchDrawingButton.BackgroundColor = Color.LightGray;
            }
            else
            {
                TouchDrawingButton.BackgroundColor = Color.Transparent;
            }
        }

        private void FormsWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DualScreenInfo.SpanMode) ||
                e.PropertyName == nameof(DualScreenInfo.IsLandscape))
            {
                UpdateSize();
            }
        }


        void MoveMenuToRootLayout()
        {
            if (ControlPanelRootLayout.Content == null)
            {
                ControlPanelRootLayout.Content = ControlPanel;
                MenuViewRootLayout.Content = MenuView;

                MenuContainerPane1.Content = null;
                ControlPanelPane1.Content = null;
            }
        }

        void MoveMenuToTwoPaneView()
        {
            if (ControlPanelRootLayout.Content != null)
            {
                ControlPanelRootLayout.Content = null;
                MenuViewRootLayout.Content = null;
                MenuContainerPane1.Content = MenuView;
                ControlPanelPane1.Content = ControlPanel;
            }
        }


        private void UpdateSize()
        {
            if (_dualScreenInfo.IsLandscape && !IsSpanned && wideModePreference == TwoPaneViewWideModeConfiguration.RightLeft)
            {
                MoveMenuToTwoPaneView();
                BackButton.IsVisible = false;

            }
            else if (!_dualScreenInfo.IsLandscape && !IsSpanned && tallModePreference != TwoPaneViewTallModeConfiguration.TopBottom)
            {
                MoveMenuToRootLayout();
                BackButton.IsVisible = false;
            }
            else
            {
                BackButton.IsVisible = (_viewModel.Mode == ViewModels.DisplayMode.View360);

                MoveMenuToTwoPaneView();
            }

            _viewModel.IsLandscape = _dualScreenInfo.IsLandscape;
            _viewModel.IsSpanned = IsSpanned;

            string stateName;
            if (_viewModel.Mode == ViewModels.DisplayMode.Drawing)
                stateName = "DrawingState";
            else if (_viewModel.Mode == ViewModels.DisplayMode.View360)
                stateName = "View360State";
            else if (IsSpanned)
                stateName = "Spanned";
            else if (_dualScreenInfo.IsLandscape)
                stateName = "SplitLandscape";
            else
                stateName = "SplitPortrait";

            VisualStateManager.GoToState(TwoPaneView, stateName);


        }

        private async Task UpdateWebViewAsync()
        {
            var sketchData = App.GetSketchData();

            if (sketchData != null)
            {
                using (var stream = new MemoryStream())
                {

                    InkRenderer.RenderImage(
                        (int)InkCanvas.CanvasSize.Width,
                        (int)InkCanvas.CanvasSize.Height,
                        sketchData.BackgroundColor,
                        sketchData.InkStrokes.ToList(),
                        stream,
                        (int)InkCanvas.CanvasSize.Width);

                    stream.Seek(0, SeekOrigin.Begin);

                    InkCanvas.UpdateImageStream(stream);
                }

                //var totalMemory = GC.GetTotalMemory(false);

                // Collect all generations of memory.
                GC.Collect();

                //System.Diagnostics.Debug.WriteLine("Memory Collected after Rendering ink:   {0:N0}",
                //                  totalMemory- GC.GetTotalMemory(true));
            }

            var webView = FullPreview;// IsSpanned ? FullPreview : SplitPreview;

            await webView.UpdateWebViewAsync().ConfigureAwait(false);
        }

        private async void OnStrokesCollected(object sender, XInkStrokesCollectedEventArgs e)
        {
            var strokes = this.InkCanvas.GetInkCanvasView().InkPresenter.StrokeContainer.GetStrokes();
            App.GetSketchData().InkStrokes = strokes;
            var width = this.InkCanvas.CanvasSize.Width;
            var height = this.InkCanvas.CanvasSize.Height;

            var webView = FullPreview;//IsSpanned ? FullPreview : SplitPreview;

            await UpdateWebViewAsync().ConfigureAwait(true);

            var inkPoints = e.Strokes[e.Strokes.Count - 1].GetInkPoints();

            if (!inkPoints.Any()) return;

            var position = inkPoints[inkPoints.Count - 1].Position;

            var alpha = -(position.X / width) * (2 * Math.PI);
            var beta = Math.PI * (height - position.Y) / height;

            webView.SetPosition(alpha, beta);

            await SaveSketchAsync().ConfigureAwait(true);

            var undoItem = App.Container.Resolve<IAddStrokesUndoItem>();

            undoItem.Container = App.GetSketchData();
            undoItem.Strokes = e.Strokes.ToList();

            _viewModel.UndoManager.Add(undoItem);
        }

        private async Task SaveSketchAsync()
        {
            App.GetSketchData().InkStrokes = this.InkCanvas.GetInkCanvasView().InkPresenter.StrokeContainer.GetStrokes();

            await (App.Current as App).SaveSketchDataAsync().ConfigureAwait(false);
        }

        private void OnShowPalette(object sender, EventArgs e)
        {
            FullPaletteView.IsVisible = true;
            var attributes = InkCanvas.GetInkCanvasView().InkPresenter.CopyDefaultDrawingAttributes();

            FullPaletteView.PenSize = attributes.Size;
            FullPaletteView.PenColor = attributes.Color;
        }

        private void OnPaletteChanged(object sender, PaletteChangedEventArgs e)
        {
            var attributes = InkCanvas.GetInkCanvasView().InkPresenter.CopyDefaultDrawingAttributes();

            attributes.Size = e.PenWidth;
            attributes.Color = e.PenColor;

            InkCanvas.GetInkCanvasView().InkPresenter.UpdateDefaultDrawingAttributes(attributes);
        }

        private void ActivateMode(string modeName)
        {
            if (Enum.TryParse<Mode>(modeName, out var mode))
                ActivateMode(mode);
        }

        private void ActivateMode(Mode mode)
        {
            InkCanvas.Mode = mode;
            Dispatcher.BeginInvokeOnMainThread(delegate
            {
                VisualStateManager.GoToState(TwoPaneView, mode.ToString());
            });
        }

        private void OnTouchDrawing()
        {
            var inkPresenter = InkCanvas.GetInkCanvasView().InkPresenter;

            bool isTouch = false;

            if (inkPresenter.InputDeviceTypes.HasFlag(XCoreInputDeviceTypes.Touch))
            {
                inkPresenter.InputDeviceTypes = XCoreInputDeviceTypes.Pen;

                TouchDrawingButton.BackgroundColor = Color.Transparent;


            }
            else
            {
                inkPresenter.InputDeviceTypes = XCoreInputDeviceTypes.Pen | XCoreInputDeviceTypes.Touch | XCoreInputDeviceTypes.Mouse;

                TouchDrawingButton.BackgroundColor = Color.LightGray;

                isTouch = true;
            }

            var properties = new Dictionary<string, string> { ["Enabled"] = isTouch.ToString(CultureInfo.InvariantCulture) };

            Analytics.TrackEvent("Touch drawing", properties);
        }

        private void OnPalette(object sender, EventArgs e)
        {
            var paletteView = FullPaletteView;

            paletteView.IsVisible = true;
            var attributes = InkCanvas.GetInkCanvasView().InkPresenter.CopyDefaultDrawingAttributes();

            paletteView.PenSize = attributes.Size;
            paletteView.PenColor = attributes.Color;
        }

        private void OnDrawingView(object sender, EventArgs e)
        {
            _viewModel.Mode = _viewModel.Mode == ViewModels.DisplayMode.Drawing
                ? ViewModels.DisplayMode.Split
                : ViewModels.DisplayMode.Drawing;

            FullPreview.IsHingeZoomingEnabled = false;

            UpdateSize();
        }

        private void OnView360(object sender, EventArgs e)
        {
            if (_viewModel.Mode == ViewModels.DisplayMode.View360)
            {
                _viewModel.Mode = ViewModels.DisplayMode.Split;
            }
            else
            {
                FullPreview.IsHingeZoomingEnabled = true;
                _viewModel.Mode = ViewModels.DisplayMode.View360;
            }

            UpdateSize();
        }

        private async void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
        {
            await UpdateWebViewAsync().ConfigureAwait(false);
        }

        private void OnMenu(object sender, EventArgs e)
        {
            MenuView.IsVisible = !MenuView.IsVisible;
            MenuContainerPane1.InputTransparent = !MenuView.IsVisible;
            MenuViewRootLayout.InputTransparent = !MenuView.IsVisible;
        }

        private void OnZoomSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            InkCanvas.GetInkCanvasView().ZoomFactor = e.NewValue;
        }

        private void OnBack(object sender, EventArgs e)
        {
            if (_viewModel.Mode != ViewModels.DisplayMode.Split)
            {
                _viewModel.Mode = ViewModels.DisplayMode.Split;

                FullPreview.IsHingeZoomingEnabled = false;

                UpdateSize();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (_viewModel.Mode != ViewModels.DisplayMode.Split)
            {
                _viewModel.Mode = ViewModels.DisplayMode.Split;

                FullPreview.IsHingeZoomingEnabled = false;

                UpdateSize();
                return true;
            }

            return base.OnBackButtonPressed();
        }

        public TwoPaneViewWideModeConfiguration WideModePreference
        {
            get => wideModePreference;
            set
            {
                wideModePreference = value;
                OnPropertyChanged(nameof(WideModePreference));
            }
        }

        public TwoPaneViewTallModeConfiguration TallModePreference
        {
            get => tallModePreference;
            set
            {
                tallModePreference = value;
                OnPropertyChanged(nameof(TallModePreference));
            }
        }
    }
}