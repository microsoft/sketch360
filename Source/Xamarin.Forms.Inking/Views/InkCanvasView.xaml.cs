// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Inking.Interfaces;
using Xamarin.Forms.Inking.Support;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Inking.Views
{
    /// <summary>
    /// InkCanvas View using SkiaSharp
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class InkCanvasView : ContentView, IDisposable, IInkCanvasView
    {
        #region Fields

        /// <summary>
        /// CanvasWidth dependency property
        /// </summary>
        public static readonly BindableProperty CanvasWidthProperty = BindableProperty.Create(
            nameof(CanvasWidth),
            typeof(double),
            typeof(InkCanvasView),
            2000.0);

        /// <summary>
        /// CanvasHeight dependency property
        /// </summary>
        public static readonly BindableProperty CanvasHeightProperty = BindableProperty.Create(
            nameof(CanvasHeight),
            typeof(double),
            typeof(InkCanvasView),
            1000.0);

        /// <summary>
        /// Horizontal offset dependency property
        /// </summary>
        public static readonly BindableProperty HorizontalOffsetProperty = BindableProperty.Create(
            nameof(HorizontalOffset),
            typeof(double),
            typeof(InkCanvasView),
            0.0,
            BindingMode.OneWay,
            null,
            OnCanvasOffsetChanged);

        /// <summary>
        /// VerticalOffset dependency property
        /// </summary>
        public static readonly BindableProperty VerticalOffsetProperty = BindableProperty.Create(
            nameof(VerticalOffsetProperty),
            typeof(double),
            typeof(InkCanvasView),
            0.0,
            BindingMode.OneWay,
            null,
            OnCanvasOffsetChanged);

        /// <summary>
        /// ZoomFactor dependency property
        /// </summary>
        public static readonly BindableProperty ZoomFactorProperty = BindableProperty.Create(
            nameof(ZoomFactor),
            typeof(double),
            typeof(InkCanvasView),
            1.0,
            BindingMode.TwoWay,
            null,
            OnZoomFactorChanged);

        private bool _isEnabled = true;

        private bool _isErasing;

        private readonly Dictionary<long, XInkStroke> _wetStrokes = new Dictionary<long, XInkStroke>();

        private SKBitmap _cachedInk;

        private readonly bool IsCaching = true;

        private Size _previousSize = Size.Zero;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the InkCanvasView class.
        /// </summary>
        public InkCanvasView()
        {
            var inkPresenter = DependencyService.Get<IInkPresenter>();

            if (inkPresenter == null)
            {
                InkPresenter = new XInkPresenter(this);
            }
            else
            {
                InkPresenter = inkPresenter;
            }

            InkPresenter.StrokeContainer.InkChanged += StrokeContainer_InkChanged;

            SizeChanged += OnSizeChanged;

            InitializeComponent();
        }

        #endregion

        #region Events

        /// <summary>
        /// Touch event
        /// </summary>
        public event EventHandler<SKTouchEventArgs> Touch;

        /// <summary>
        /// Before paint draw handler
        /// </summary>
        public event EventHandler<SKCanvas> Painting;

        /// <summary>
        /// After paint draw handler
        /// </summary>
        public event EventHandler<SKCanvas> Painted;

        /// <summary>
        /// Default drawing attributes changed event handler
        /// </summary>
        public event EventHandler<TypedEventArgs<XInkDrawingAttributes>> DefaultInkDrawingAttributesChanged;

        /// <summary>
        /// Zoom factor changed
        /// </summary>
        public event EventHandler<TypedEventArgs<double>> ZoomFactorChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the touch and rendering is enabled
        /// </summary>
        /// <remarks>this should be false when the platform supplies a native inking control (like UWP)</remarks>
        public bool IsControlEnabled
        {
            get => _isEnabled;

            set
            {
                _isEnabled = value;
                //WetInkView.EnableTouchEvents = value;
            }
        }
        /// <summary>
        /// Gets the wet strokes
        /// </summary>
        public IDictionary<long, XInkStroke> WetStrokes => _wetStrokes;

        /// <summary>
        /// Gets or sets the eraser size
        /// </summary>
        public double EraserSize { get; set; } = 8.0;

        /// <summary>
        /// Gets the ink presenter
        /// </summary>
        public IInkPresenter InkPresenter { get; }

        /// <summary>
        /// Gets the ink stroke container
        /// </summary>
        internal IInkStrokeContainer InkStrokeContainer => InkPresenter.StrokeContainer;

        /// <summary>
        /// Gets the density of each pixel on the canvas
        /// </summary>
        public double PixelDensity { get; private set; } = 2.5;

        /// <summary>
        /// Gets the pixed width
        /// </summary>
        public double PixelWidth => Width * PixelDensity;

        /// <summary>
        /// gets the pixel width
        /// </summary>
        public double PixelHeight => Height * PixelDensity;

        /// <summary>
        /// Gets or sets the zoom factor
        /// </summary>
        public double ZoomFactor
        {
            get => (double)GetValue(ZoomFactorProperty);
            set => SetValue(ZoomFactorProperty, value);
        }

        /// <summary>
        /// Gets or sets the canvas width
        /// </summary>
        public double CanvasWidth
        {
            get => (double)GetValue(CanvasWidthProperty);
            set => SetValue(CanvasWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the canvas height
        /// </summary>
        public double CanvasHeight
        {
            get => (double)GetValue(CanvasHeightProperty);
            set => SetValue(CanvasHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the horizontal offset
        /// </summary>
        public double HorizontalOffset
        {
            get => (double)GetValue(HorizontalOffsetProperty);
            set => SetValue(HorizontalOffsetProperty, value);
        }

        /// <summary>
        /// Gets or sets the vertical offset
        /// </summary>
        public double VerticalOffset
        {
            get => (double)GetValue(VerticalOffsetProperty);
            set => SetValue(VerticalOffsetProperty, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Dispose of the grid bitmap, ink strokes, and cached ink
        /// </summary>
        public void Dispose()
        {
            if (_wetStrokes != null)
            {
                foreach (var item in _wetStrokes.Values)
                {
                    item.Dispose();
                }
                _wetStrokes.Clear();
            }

            if (_cachedInk != null)
            {
                _cachedInk.Dispose();
                _cachedInk = null;
            }
        }

        /// <summary>
        /// Update the default drawing attributes
        /// </summary>
        /// <param name="attributes"></param>
        internal void UpdateDefaultDrawingAttributes(XInkDrawingAttributes attributes)
        {
            InkPresenter.UpdateDefaultDrawingAttributes(attributes);

            DefaultInkDrawingAttributesChanged?.Invoke(this, new TypedEventArgs<XInkDrawingAttributes>(attributes));
        }

        /// <summary>
        /// Invalidate the wet and dry canvas views
        /// </summary>
        /// <param name="wetInk">true to invalidate the wet ink</param>
        /// <param name="dryInk">true to invalidate the dry ink</param>
        public void InvalidateCanvas(bool wetInk, bool dryInk)
        {
            if (wetInk)
            {
                WetInkView.InvalidateSurface();
            }

            if (dryInk)
            {
                CanvasView.InvalidateSurface();
            }

            CanvasInvalidated?.Invoke(this, new InvalidateCanvasEventArgs(wetInk, dryInk));
        }

        /// <summary>
        /// Canvas invalidated event
        /// </summary>
        public event EventHandler<InvalidateCanvasEventArgs> CanvasInvalidated;



        /// <summary>
        /// Copy the default darwing attributes
        /// </summary>
        /// <returns>a copy of the default drawing attributes</returns>
        internal XInkDrawingAttributes CopyDefaultDrawingAttributes()
        {
            return InkPresenter.CopyDefaultDrawingAttributes();
        }

        #endregion

        #region Implementation

        private static void OnZoomFactorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is InkCanvasView view && oldValue is double oldZoom && newValue is double newZoom)
            {
                var delta = newZoom - oldZoom;

                var w = view.CanvasWidth * delta;
                var h = view.CanvasHeight * delta;

                var centerX = (view.PixelWidth * 0.5f + view.HorizontalOffset) / oldZoom;
                var centerY = (view.PixelHeight * 0.5f + view.VerticalOffset) / oldZoom;

                view.HorizontalOffset += w * (centerX / view.CanvasWidth);
                view.VerticalOffset += h * (centerY / view.CanvasHeight);

                view.CanvasView.InvalidateSurface();

                view.ZoomFactorChanged?.Invoke(view, new TypedEventArgs<double>(view.ZoomFactor));
            }
        }

        private static void OnCanvasOffsetChanged(BindableObject bindable, object oldValue, object newVale)
        {
            if (bindable is InkCanvasView view)
            {
                view.CanvasView.InvalidateSurface();
            }
        }

        private static XPointerDeviceType ToDeviceType(SKTouchDeviceType deviceType) =>
            deviceType switch
            {
                SKTouchDeviceType.Touch => XPointerDeviceType.Touch,
                SKTouchDeviceType.Mouse => XPointerDeviceType.Mouse,
                SKTouchDeviceType.Pen => XPointerDeviceType.Pen,
                _ => XPointerDeviceType.Touch,
            };

        // Calculate the distance between
        // point pt and the segment p1 --> p2.
        private static double FindDistanceToSegment(Point pt, Point p1, Point p2, out Point closest)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            var t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Point(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new Point(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new Point(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void OnDrawDryInk(object sender, SKPaintSurfaceEventArgs e)
        {
            if (_isEnabled)
            {

                PixelDensity = e.Info.Width / CanvasView.Width;

                var left = Convert.ToSingle(HorizontalOffset / ZoomFactor);
                var top = Convert.ToSingle(VerticalOffset / ZoomFactor);
                var right = Convert.ToSingle(left + e.Info.Width / ZoomFactor);
                var bottom = Convert.ToSingle(top + e.Info.Height / ZoomFactor);

                DrawInk(e.Surface.Canvas, new SKRect(left, top, right, bottom));
            }

            PaintForeground(e.Surface.Canvas);
        }


        private void DrawInk(SKCanvas canvas, SKRect bounds)
        {
            canvas.Translate(-Convert.ToSingle(HorizontalOffset), -Convert.ToSingle(VerticalOffset));

            canvas.Scale(Convert.ToSingle(ZoomFactor));

            PaintBackground(canvas);

            if (IsCaching)
            {
                if (_cachedInk == null)
                {
                    _cachedInk = new SKBitmap(Convert.ToInt32(CanvasWidth), Convert.ToInt32(CanvasHeight));

                    using var cacheCanvas = new SKCanvas(_cachedInk);
                    cacheCanvas.Clear(SKColors.Transparent);
                    cacheCanvas.Draw(InkPresenter.StrokeContainer.GetStrokes());
                }

                canvas.DrawBitmap(_cachedInk, new SKPoint(0, 0));
            }
            else
            {
                canvas.Draw(InkPresenter.StrokeContainer.GetStrokes(), bounds);
            }


        }

        /// <summary>
        /// paint the foreground
        /// </summary>
        /// <param name="canvas">the SkiaSharp canvas</param>
        public void PaintForeground(SKCanvas canvas)
        {
            Painted?.Invoke(this, canvas);
        }

        /// <summary>
        /// Paint the background
        /// </summary>
        /// <param name="canvas">the SkiaSharp canvas</param>
        public void PaintBackground(SKCanvas canvas)
        {
            Painting?.Invoke(this, canvas);
        }

        private static XCoreInputDeviceTypes ToInkDeviceType(SKTouchDeviceType skDeviceType) =>
            skDeviceType switch
            {
                SKTouchDeviceType.Touch => XCoreInputDeviceTypes.Touch,
                SKTouchDeviceType.Mouse => XCoreInputDeviceTypes.Mouse,
                SKTouchDeviceType.Pen => XCoreInputDeviceTypes.Pen,
                _ => XCoreInputDeviceTypes.Pen,
            };

        private void OnTouch(object sender, SKTouchEventArgs e)
        {
            // first try using the override 
            Touch?.Invoke(this, e);

            if (e.Handled)
            {

                // touch was already handled
                return;
            }

            var deviceType = ToInkDeviceType(e.DeviceType);

            if (InkPresenter.InputDeviceTypes.HasFlag(deviceType))
            {
                if (e.DeviceType == SKTouchDeviceType.Pen && e.MouseButton == SKMouseButton.Middle)
                {
                    OnErasing(e);
                }
                else
                {
                    switch (InkPresenter.InputProcessingConfiguration.Mode)
                    {
                        case XInkInputProcessingMode.Inking:
                            OnInking(e);
                            break;

                        case XInkInputProcessingMode.Erasing:
                            OnErasing(e);
                            break;

                        case XInkInputProcessingMode.None:
                            OnNone(e);
                            break;
                    }
                }
            }
            else
            {
                e.Handled = false;
            }
        }

        private void OnNone(SKTouchEventArgs args)
        {
            var position = GetCanvasPosition(args.Location);

            if (position.X < 0 || position.Y < 0 || position.X >= CanvasWidth || position.Y >= CanvasHeight)
            {
                args.Handled = false;
                return;
            }

            var pointerEventArgs = new XPointerEventArgs
            {
                CurrentPoint = new XPointerPoint
                {
                    PointerId = Convert.ToUInt32(args.Id),
                    Position = args.Location.ToFormsPoint(),
                    Timestamp = Convert.ToUInt64(DateTime.UtcNow.Ticks),
                    PointerDevice = new XPointerDevice
                    {
                        PointerDeviceType = ToDeviceType(args.DeviceType)
                    },
                    IsInContact = args.InContact
                }
            };

            InkPresenter.UnprocessedInput.ProcessTouch(args.ActionType.ToString(), pointerEventArgs);
        }

        private void OnErasing(SKTouchEventArgs args)
        {
            var position = GetCanvasPosition(args.Location);

            if (position.X < 0 || position.Y < 0 || position.X >= CanvasWidth || position.Y >= CanvasHeight)
            {
                _isErasing = false;

                return;
            }

            switch (args.ActionType)
            {
                case SKTouchAction.Pressed:
                    _isErasing = true;
                    args.Handled = true;
                    EraseAt(args.Location);
                    break;

                case SKTouchAction.Moved:
                    if (_isErasing)
                    {
                        args.Handled = true;
                        EraseAt(args.Location);
                    }
                    break;

                case SKTouchAction.Released:
                    _isErasing = false;
                    args.Handled = true;
                    break;

                default:
                    args.Handled = false;
                    break;
            }

        }

        private void OnInking(SKTouchEventArgs args)
        {
            var position = GetCanvasPosition(args.Location);

            if (position.X < 0 || position.Y < 0 || position.X >= CanvasWidth || position.Y >= CanvasHeight)
            {
                ReleaseInking(args, null);

                return;
            }

            var point = new XInkPoint(position, args.Pressure, 0.0f, 0.0f, Convert.ToUInt64(DateTime.UtcNow.Ticks));

            var points = new List<XInkPoint>(new[]
            {
                point
            });

            if (InkPresenter.WetStrokeUpdateSource == null)
            {
                OnDraw(args, points);
            }
            else
            {
                var updateArgs = new XCoreWetStrokeUpdateEventArgs
                {
                    NewInkPoints = points,
                    PointerId = Convert.ToUInt32(args.Id),
                    Disposition = XCoreWetStrokeDisposition.Inking
                };

                switch (args.ActionType)
                {
                    case SKTouchAction.Pressed:
                        InkPresenter.WetStrokeUpdateSource.OnPressed(updateArgs);
                        break;

                    case SKTouchAction.Moved:
                        InkPresenter.WetStrokeUpdateSource.OnMoved(updateArgs);
                        break;

                    case SKTouchAction.Released:
                        InkPresenter.WetStrokeUpdateSource.OnReleased(updateArgs);
                        break;

                    case SKTouchAction.Cancelled:
                        InkPresenter.WetStrokeUpdateSource.OnCancelled(updateArgs);
                        break;
                    default:
                        updateArgs.Disposition = XCoreWetStrokeDisposition.Canceled;
                        break;
                }

                if (updateArgs.Disposition == XCoreWetStrokeDisposition.Inking || updateArgs.Disposition == XCoreWetStrokeDisposition.Completed)
                {
                    OnDraw(args, points);
                }
            }
        }

        private void OnDraw(SKTouchEventArgs args, List<XInkPoint> points)
        {
            switch (args.ActionType)
            {
                case SKTouchAction.Pressed:
                    if (!WetStrokes.ContainsKey(args.Id))
                    {
                        var wetStroke = new XInkStroke
                        {
                            DrawingAttributes = CopyDefaultDrawingAttributes()
                        };

                        foreach (var item in points)
                        {
                            wetStroke.Add(item);
                        }

                        WetStrokes.Add(args.Id, wetStroke);

                        InvalidateCanvas(true, false);
                    }

                    args.Handled = true;

                    break;

                case SKTouchAction.Moved:
                    if (WetStrokes.TryGetValue(args.Id, out XInkStroke wetStroke2))
                    {
                        foreach (var item in points)
                        {
                            wetStroke2.Add(item);
                        }

                        InvalidateCanvas(true, false);

                        args.Handled = true;
                    }
                    break;

                case SKTouchAction.Released:
                    ReleaseInking(args, points);
                    break;

                case SKTouchAction.Cancelled:
                    if (WetStrokes.TryGetValue(args.Id, out XInkStroke wetStroke4))
                    {
                        WetStrokes.Remove(args.Id);

                        InkStrokeContainer.Add(wetStroke4);

                        InvalidateCanvas(true, true);

                        InkPresenter.TriggerStrokesCollected(new[] { wetStroke4 });

                        args.Handled = true;
                    }
                    break;
            }
        }

        private void ReleaseInking(SKTouchEventArgs args, List<XInkPoint> points)
        {
            if (WetStrokes.TryGetValue(args.Id, out XInkStroke wetStroke3))
            {
                if (points != null)
                {
                    foreach (var item in points)
                    {
                        wetStroke3.Add(item);
                    }
                }

                WetStrokes.Remove(args.Id);

                InkStrokeContainer.Add(wetStroke3);

                InvalidateCanvas(true, true);

                InkPresenter.TriggerStrokesCollected(new[] { wetStroke3 });

                args.Handled = true;
            }
        }

        private void EraseAt(SKPoint location)
        {
            var point = GetCanvasPosition(location);

            var strokes = InkPresenter.StrokeContainer.GetStrokes();

            var strokesToErase = new List<XInkStroke>();

            foreach (var stroke in strokes)
            {
                var inkPoints = stroke.GetInkPoints();

                if (inkPoints.Count == 1)
                {
                    var firstPoint = inkPoints[0];

                    var distance = point.Distance(firstPoint.Position) - (stroke.DrawingAttributes.Size / 4.0f);

                    //var eraseSize = EraserSize;

                    if (distance <= 0)
                    {
                        strokesToErase.Add(stroke);
                    }
                }
                else
                {
                    if (!stroke.BoundingRect.Contains(point)) continue;

                    var count = inkPoints.Count - 1;

                    for (var i = 0; i < count; i++)
                    {
                        var distance = FindDistanceToSegment(point, inkPoints.ElementAt(i).Position, inkPoints.ElementAt(i + 1).Position, out _);

                        if (distance <= EraserSize)
                        {
                            strokesToErase.Add(stroke);

                            break;
                        }
                    }
                }
            }

            if (strokesToErase.Any())
            {
                InkPresenter.StrokeContainer.Remove(strokesToErase.ToList());

                InkPresenter.TriggerStrokesErased(strokesToErase.ToList());
                InvalidateCanvas(false, true);
            }
        }

        private void OnDrawWetInk(object sender, SKPaintSurfaceEventArgs e)
        {
            if (!_isEnabled) return;

            var canvas = e.Surface.Canvas;

            canvas.Clear();

            canvas.Translate(-Convert.ToSingle(HorizontalOffset), -Convert.ToSingle(VerticalOffset));

            canvas.Scale(Convert.ToSingle(ZoomFactor));

            foreach (var stroke in _wetStrokes.Values)
            {
                canvas.Draw(stroke, false);
            }
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            if (!_previousSize.IsZero)
            {
                var deltaX = Width - _previousSize.Width;
                var deltaY = Height - _previousSize.Height;

                HorizontalOffset -= deltaX * 0.5 * PixelDensity;
                VerticalOffset -= deltaY * 0.5 * PixelDensity;
            }

            _previousSize = new Size(Width, Height);
        }

        private void StrokeContainer_InkChanged(object sender, EventArgs e)
        {
            _cachedInk?.Dispose();
            _cachedInk = null;
        }

        /// <summary>
        /// Gets an canvas point adjusted by the offset and zoom factor
        /// </summary>
        /// <param name="location">the screen point</param>
        /// <returns>the canvas point</returns>
        public Point GetCanvasPosition(SKPoint location) =>
            new Point(
                (location.X + HorizontalOffset) / ZoomFactor,
                (location.Y + VerticalOffset) / ZoomFactor);

        #endregion
    }
}