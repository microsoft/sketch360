// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CoreWetStrokeDisposition = Xamarin.Forms.Inking.XCoreWetStrokeDisposition;
using CoreWetStrokeUpdateEventArgs = Xamarin.Forms.Inking.XCoreWetStrokeUpdateEventArgs;
using CoreWetStrokeUpdateSource = Xamarin.Forms.Inking.XCoreWetStrokeUpdateSource;
using InkCanvas = Xamarin.Forms.Inking.Views.InkCanvasView;
using InkPoint = Xamarin.Forms.Inking.XInkPoint;
using Point = Xamarin.Forms.Point;

namespace Sketch360.Core.Support
{
    /// <summary>
    /// Equirectangular stencil
    /// </summary>
    public sealed class EquirectangularStencil : BindableBase, IDisposable
    {
        #region Enums
        /// <summary>
        /// The anchor type
        /// </summary>
        public enum AnchorType
        {
            /// <summary>
            /// No anchor
            /// </summary>
            None,
            /// <summary>
            /// the back left anchor
            /// </summary>
            Back0,
            /// <summary>
            /// the left anchor
            /// </summary>
            Left,
            /// <summary>
            /// the front anchor
            /// </summary>
            Front,
            /// <summary>
            /// the right anchor
            /// </summary>
            Right,
            /// <summary>
            /// the back right anchor
            /// </summary>
            Back1,

            /// <summary>
            /// snap to vertical lines
            /// </summary>
            VerticalLines
        }
        #endregion

        #region Fields
        private readonly CoreWetStrokeUpdateSource _source;
        private readonly InkCanvas _inkCanvas;
        private readonly double _height;
        private bool _isActive;
        private AnchorType _anchor;
        private BezierPoints _curveBelow;
        private double _curveBelowPercent;
        private bool _lowerHalf;
        private bool _isPositive;
        private readonly ObservableCollection<BezierPoints> _bezierPoints = new ObservableCollection<BezierPoints>();
        private readonly Dictionary<AnchorType, double> _anchorXValues = new Dictionary<AnchorType, double>();
        private EquirectangularStencilMode _mode = EquirectangularStencilMode.None;
        private double _vertical;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the EquirectangularStencil class.
        /// </summary>
        /// <param name="inkCanvas">the ink canvas</param>
        public static async Task<EquirectangularStencil> CreateAsync(InkCanvas inkCanvas)
        {
            var stencil = new EquirectangularStencil(inkCanvas);

            await stencil.LoadBezierPointsAsync().ConfigureAwait(false);

            //_inkCanvas.SizeChanged += _inkCanvas_SizeChanged;

            return stencil;
        }

        private EquirectangularStencil(InkCanvas inkCanvas)
        {
            if (inkCanvas is null) throw new ArgumentNullException(nameof(inkCanvas));

            _inkCanvas = inkCanvas;

            _anchorXValues[AnchorType.Back0] = 0;
            _anchorXValues[AnchorType.Left] = 100;
            _anchorXValues[AnchorType.Front] = 200;
            _anchorXValues[AnchorType.Right] = 300;
            _anchorXValues[AnchorType.Back1] = 400;

            _source = CoreWetStrokeUpdateSource.Create(inkCanvas.InkPresenter);

            _height = inkCanvas.CanvasHeight;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the stencil mode
        /// </summary>
        public EquirectangularStencilMode Mode
        {
            get => _mode;

            set
            {
                if (SetProperty(ref _mode, value))
                {
                    switch (value)
                    {
                        case EquirectangularStencilMode.None:
                            IsActive = false;
                            break;

                        case EquirectangularStencilMode.FrontBackLines:
                            Anchor = AnchorType.Front;
                            IsActive = true;
                            break;

                        case EquirectangularStencilMode.LeftRightLines:
                            Anchor = AnchorType.Left;
                            IsActive = true;
                            break;

                        case EquirectangularStencilMode.VerticalLines:
                            Anchor = AnchorType.VerticalLines;
                            IsActive = true;
                            break;
                    }

                    //UpdateAnchorVisuals();
                }
            }
        }

        /// <summary>
        /// Gets the stencil curves
        /// </summary>
        public IEnumerable<BezierPoints> StencilCurves { get => _bezierPoints; }

        /// <summary>
        /// Gets or sets a value indicating whether the stencil is active
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                if (value != _isActive)
                {
                    _isActive = value;

                    if (value)
                    {
                        //_inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen;
                        //_inkCanvas.Holding += InkCanvas_Holding;

                        _source.WetStrokeStarting += Source_WetStrokeStarting;
                        _source.WetStrokeStopping += Source_WetStrokeStopping;
                        _source.WetStrokeCanceled += Source_WetStrokeCanceled;
                        _source.WetStrokeCompleted += Source_WetStrokeCompleted;
                        _source.WetStrokeContinuing += Source_WetStrokeContinuing;

                        //_inkCanvas.KeyDown += _inkCanvas_KeyDown;

                        //_inkCanvas.PreviewKeyDown += _inkCanvas_PreviewKeyDown;
                    }
                    else
                    {
                        //_inkCanvas.Holding -= InkCanvas_Holding;

                        _source.WetStrokeStarting -= Source_WetStrokeStarting;
                        _source.WetStrokeStopping -= Source_WetStrokeStopping;
                        _source.WetStrokeCanceled -= Source_WetStrokeCanceled;
                        _source.WetStrokeCompleted -= Source_WetStrokeCompleted;
                        _source.WetStrokeContinuing -= Source_WetStrokeContinuing;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the anchor
        /// </summary>
        public AnchorType Anchor
        {
            get => _anchor; set
            {
                if (_anchor != value)
                {
                    _anchor = value;

                    //AnchorChanged?.Invoke(this, new TypedEventArgs<AnchorType>(Anchor));

                    //ShowAnchor();
                }
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Anchor changed event
        /// </summary>
        //public event EventHandler<TypedEventArgs<AnchorType>> AnchorChanged;
        #endregion

        #region Methods
        /// <summary>
        /// Disconnect the events
        /// </summary>
        public void Dispose()
        {
            IsActive = false;
        }
        #endregion

        #region Implementation

        private async Task LoadBezierPointsAsync()
        {
            string resourceID = "Sketch360.XPlat.Assets.Bezier Curves.csv";

            var assembly = GetType().GetTypeInfo().Assembly;

            var lines = new List<string>();

            using (var stream = assembly.GetManifestResourceStream(resourceID))
            {
                using var reader = new StreamReader(stream);
                while (!reader.EndOfStream)
                {
                    lines.Add(await reader.ReadLineAsync().ConfigureAwait(false));
                }
            }

            _bezierPoints.Clear();

            var index = 0.0;

            var increment = 100.0 / 9.0;

            foreach (var line in lines.Skip(1))
            {

                var numbers = from item in line.Split(',')
                              select double.Parse(item, CultureInfo.InvariantCulture);

                var curve = new BezierPoints
                {
                    Point0 = new Point(0, 100),
                    Point1 = new Point(
                        numbers.ElementAt(0),
                        numbers.ElementAt(1)),
                    Point2 = new Point(numbers.ElementAt(2),
                        numbers.ElementAt(3)),
                    Point3 = new Point(100, index * increment)
                };

                index++;

                _bezierPoints.Add(curve);

                UpdateCurvePoints();
            }
        }

        private void UpdateCurvePoints()
        {
            double steps = 1000;

            foreach (var curve in _bezierPoints)
            {
                curve.CurvePoints = new List<Point>();

                for (double t = 0; t < 1; t += 1.0 / steps)
                {
                    var point = Casteljau.GetBezierPoint(curve.Point0, curve.Point1, curve.Point2, curve.Point3, t);

                    curve.CurvePoints.Add(new Point(
                        point.X * _inkCanvas.Height / 100.0,
                        point.Y * _inkCanvas.Height / 100.0));
                }
            }
        }


        private static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        ///// <summary>
        ///// This shoudl be the longitude
        ///// </summary>
        ///// <param name="lambdaM"></param>
        ///// <param name="theta"></param>
        ///// <param name="phi0"></param>
        ///// <returns></returns>
        //private static double Lambda(double lambdaM, double theta, double phi0)
        //{
        //    return lambdaM + Math.Atan(Math.Tan(theta) / Math.Tan(phi0));
        //}

        ///// <summary>
        ///// this should be the latitude
        ///// </summary>
        ///// <param name="phi0">the Phi 0 in radians</param>
        ///// <param name="theta">theta in radians</param>
        ///// <returns></returns>
        //private static double Phi(double phi0, double theta)
        //{
        //    var phi = Math.Atan(Math.Sqrt(Math.Pow(Math.Tan(phi0), 2) + Math.Pow(Math.Tan(theta), 2)));

        //    return phi;
        //}

        private void Source_WetStrokeContinuing(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            if (Anchor == AnchorType.VerticalLines)
            {
                AddVerticalLinePoints(args);

                return;
            }

            if (_curveBelow == null)
            {
                return;
            }

            AddPointsOnCurve(args);
        }

        private void AddVerticalLinePoints(CoreWetStrokeUpdateEventArgs args)
        {
            if (!args.NewInkPoints.Any())
            {
                return;
            }

            var lastPoint = args.NewInkPoints.Last();

            args.NewInkPoints.Clear();

            var newPoint = new Point(_vertical, lastPoint.Position.Y);

            var inkPoint = new InkPoint(newPoint, lastPoint.Pressure, lastPoint.TiltX, lastPoint.TiltY, lastPoint.Timestamp);

            args.NewInkPoints.Add(inkPoint);

            args.Disposition = CoreWetStrokeDisposition.Inking;
        }

        private void AddPointsOnCurve(CoreWetStrokeUpdateEventArgs args)
        {
            if (args.NewInkPoints.Any())
            {
                var lastPoint = args.NewInkPoints.Last();

                args.NewInkPoints.Clear();

                var halfHeight = _height / 2.0;

                Point normalizedPoint;

                if (lastPoint.Position.Y > halfHeight)
                {
                    normalizedPoint = new Point(100 * lastPoint.Position.X / halfHeight - _anchorXValues[Anchor], 200 - (100 * lastPoint.Position.Y / halfHeight));
                }
                else
                {
                    normalizedPoint = new Point(100 * lastPoint.Position.X / halfHeight - _anchorXValues[Anchor], 100 * lastPoint.Position.Y / halfHeight);
                }

                if (_isPositive)
                {
                    if (normalizedPoint.X < 0)
                    {
                        // point started as positive but new point is negative
                        return;
                    }
                }
                else if (normalizedPoint.X >= 0)
                {
                    // point started out as negative but new point is positive
                    return;
                }

                //}
                var belowPoint = GetClosestBezierPoint(normalizedPoint, _curveBelow);
                var curveAboveIndex = _bezierPoints.IndexOf(_curveBelow) - 1;

                if (curveAboveIndex >= 0)
                {
                    var abovePoint = GetClosestBezierPoint(normalizedPoint, _bezierPoints[curveAboveIndex]);

                    double xAvg = abovePoint.X + (belowPoint.X - abovePoint.X) * (1.0 - _curveBelowPercent) + _anchorXValues[Anchor];

                    var yAvg = abovePoint.Y + (belowPoint.Y - abovePoint.Y) * (1.0 - _curveBelowPercent);

                    var x = (_height / 2) * xAvg / 100.0;

                    double y = (_height / 2) * yAvg / 100.0;

                    if (_lowerHalf)
                    {
                        y = _height - y;
                    }

                    var newPoint = new Point(x, y);

                    var inkPoint = new InkPoint(newPoint, lastPoint.Pressure, lastPoint.TiltX, lastPoint.TiltY, lastPoint.Timestamp);

                    args.NewInkPoints.Add(inkPoint);

                    args.Disposition = CoreWetStrokeDisposition.Inking;
                }
            }
            else
            {
                args.Disposition = CoreWetStrokeDisposition.Completed;
            }
        }

        private void Source_WetStrokeCompleted(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            if (Anchor == AnchorType.VerticalLines)
            {
                AddVerticalLinePoints(args);

                args.Disposition = CoreWetStrokeDisposition.Completed;
            }
            else
            {
                AddPointsOnCurve(args);
                args.Disposition = CoreWetStrokeDisposition.Completed;
            }
        }

        private void Source_WetStrokeCanceled(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
        }

        private void Source_WetStrokeStopping(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            if (Mode == EquirectangularStencilMode.None)
            {
                args.Disposition = CoreWetStrokeDisposition.Canceled;

                return;
            }
            else if (Mode == EquirectangularStencilMode.VerticalLines)
            {
                AddVerticalLinePoints(args);

                return;
            }

            AddPointsOnCurve(args);

            args.Disposition = CoreWetStrokeDisposition.Completed;
        }

        private void Source_WetStrokeStarting(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {

            var firstPoint = args.NewInkPoints.First();

            if (Anchor == AnchorType.None)
            {
                args.Disposition = CoreWetStrokeDisposition.Inking;

                return;
            }
            else if (Anchor == AnchorType.VerticalLines)
            {
                args.NewInkPoints.Add(firstPoint);

                args.Disposition = CoreWetStrokeDisposition.Inking;

                _vertical = firstPoint.Position.X;

                return;
            }

            _curveBelow = null;

            var point = firstPoint.Position;
            var halfHeight = _height / 2.0;

            Point normalizedPoint;

            if (point.Y > halfHeight)
            {
                _lowerHalf = true;
                normalizedPoint = new Point(100 * point.X / halfHeight - _anchorXValues[Anchor], 200 - (100 * point.Y / halfHeight));
            }
            else
            {
                _lowerHalf = false;
                normalizedPoint = new Point(100 * point.X / halfHeight - _anchorXValues[Anchor], 100 * point.Y / halfHeight);
            }

            if (normalizedPoint.X > 200 && Anchor == AnchorType.Left)
            {
                normalizedPoint.X -= 200;
                Anchor = AnchorType.Right;
            }
            else if (normalizedPoint.X < -200 && Anchor == AnchorType.Right)
            {
                normalizedPoint.X += 200;
                Anchor = AnchorType.Left;
            }
            else if (normalizedPoint.X > 200 || normalizedPoint.Y < -200)
            {
                args.Disposition = CoreWetStrokeDisposition.Canceled;

                return;
            }

            _isPositive = normalizedPoint.X >= 0;

            var previousCurvePoint = new Point();
            //var index = 0;

            foreach (var curve in _bezierPoints)
            {
                //var t = point.X / halfHeight;

                Point curvePoint = GetClosestBezierPoint(normalizedPoint, curve);

                var curveY = halfHeight * curvePoint.Y / 100;

                var pointAngle = Math.Atan2(100 - normalizedPoint.Y, Math.Abs(normalizedPoint.X));

                var curveAngle = Math.Atan2(100 - curvePoint.Y, Math.Abs(curvePoint.X));

                if (curvePoint.Y > normalizedPoint.Y && curveAngle < pointAngle)
                {
                    var curveX = halfHeight * curvePoint.X / 100;
                    // this curve is below the point

                    _curveBelow = curve;

                    //System.Diagnostics.Debug.WriteLineIf(index == 0, $"Normalized index 0: {normalizedPoint}");

                    var totalDistance = Distance(previousCurvePoint.X,
                        previousCurvePoint.Y, curvePoint.X, curvePoint.Y);
                    // normalized distance
                    var belowDistance = Distance(
                                            normalizedPoint.X,
                                            normalizedPoint.Y,
                                            curvePoint.X,
                                            curvePoint.Y);
                    _curveBelowPercent = belowDistance / totalDistance;

                    args.NewInkPoints.Clear();
                    args.NewInkPoints.Add(firstPoint);

                    args.Disposition = CoreWetStrokeDisposition.Inking;

                    break;
                }

                previousCurvePoint = curvePoint;

                //index++;
            }
        }

        private static Point GetClosestBezierPoint(Point normalizePoint, BezierPoints curve)
        {
            if (curve == null) throw new ArgumentNullException(nameof(curve));

            var regularPoint = normalizePoint;

            if (normalizePoint.X < -100)
            {
                regularPoint.X = 200 + normalizePoint.X;
            }
            else if (normalizePoint.X < 0)
            {
                regularPoint.X = -normalizePoint.X;
            }
            else if (normalizePoint.X > 100)
            {
                regularPoint.X = 200.0 - normalizePoint.X;
            }

            var minDistance = double.MaxValue;

            double minT = 0;

            for (double t = 0.0; t < 1.0; t += 0.001)
            {
                var pointOnCurve = Casteljau.GetBezierPoint(curve.Point0,
                    curve.Point1, curve.Point2, curve.Point3, t);

                var distance = Distance(regularPoint.X, regularPoint.Y, pointOnCurve.X, pointOnCurve.Y);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    minT = t;
                }

                //System.Diagnostics.Debug.WriteLine($"{t}: {pointOnCurve} = {distance}");
            }

            var curvePoint = Casteljau.GetBezierPoint(curve.Point0,
                curve.Point1, curve.Point2, curve.Point3, minT);

            if (normalizePoint.X < -100)
            {
                curvePoint.X -= 200;
            }
            else if (normalizePoint.X < 0)
            {
                curvePoint.X = -curvePoint.X;
            }
            else if (normalizePoint.X > 100)
            {
                curvePoint.X = 200 - curvePoint.X;
            }

            return curvePoint;
        }
        #endregion
    }
}
