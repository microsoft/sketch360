// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AppCenter.Analytics;
using Sketch360.XPlat.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Inking.Interfaces;
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
        #endregion

        #region Fields
        private readonly CoreWetStrokeUpdateSource _source;
        private readonly double _height;
        private bool _isActive;
        private EquirectangularStencilMode _mode = EquirectangularStencilMode.None;
        private double _vertical;
        private Vertex _apex;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the EquirectangularStencil class.
        /// </summary>
        /// <param name="inkCanvas"> the ink canvas</param>
        public EquirectangularStencil(InkCanvas inkCanvas)
        {
            var coreInking = DependencyService.Get<ICoreInking>();

            if (coreInking == null)
            {
                _source = CoreWetStrokeUpdateSource.Create(inkCanvas.InkPresenter);
            }
            else
            {
                _source = coreInking.Create(inkCanvas.InkPresenter);
            }

            _height = inkCanvas.CanvasHeight;
        }

        #endregion

        #region Properties

        public Vertex Apex
        {
            get => _apex;

            set
            {
                SetProperty(ref _apex, value);
            }
        }

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
                            IsActive = true;
                            break;

                        case EquirectangularStencilMode.LeftRightLines:
                            IsActive = true;
                            break;

                        case EquirectangularStencilMode.VerticalLines:
                            IsActive = true;
                            break;

                        case EquirectangularStencilMode.TwoPoint:
                            IsActive = true;
                            break;
                    }
                }
            }
        }

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

        #endregion

        #region Events
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


        private void Source_WetStrokeContinuing(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            if (Mode == EquirectangularStencilMode.VerticalLines)
            {
                AddVerticalLinePoints(args);

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

                var newPoints = (from point in args.NewInkPoints
                                 let vertex = Vertex.CreateVertex(point.Position, _height) // a spherical vertex at the point
                                 let elevation = Equirectangular.GetElevation(Apex, vertex.Azimuth) // gets the elevation given the curve's apex
                                 let newVertex = new Vertex { Elevation = elevation, Azimuth = vertex.Azimuth } // create a new vertex
                                 let newPosition = newVertex.GetPoint(_height) // get the screen point for that vertex
                                 select new InkPoint(newPosition, point.Pressure, point.TiltX, point.TiltY, point.Timestamp)).ToList();

                args.NewInkPoints.SetRange(newPoints);

                args.Disposition = CoreWetStrokeDisposition.Inking;
            }
            else
            {
                args.Disposition = CoreWetStrokeDisposition.Completed;
            }
        }

        private void Source_WetStrokeCompleted(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            AddPointsOnCurve(args);

            var properties = new Dictionary<string, string>
            {
                [nameof(Mode)] = this.Mode.ToString()
            };

            Analytics.TrackEvent("Stencil", properties);

            args.Disposition = CoreWetStrokeDisposition.Completed;
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
            if (!args.NewInkPoints.Any())
            {
                args.Disposition = CoreWetStrokeDisposition.Completed;
                return;
            }

            var firstPoint = args.NewInkPoints.First();

            switch (Mode)
            {
                case EquirectangularStencilMode.None:
                    args.Disposition = CoreWetStrokeDisposition.Inking;
                    break;

                case EquirectangularStencilMode.VerticalLines:
                    args.NewInkPoints.Add(firstPoint);
                    args.Disposition = CoreWetStrokeDisposition.Inking;
                    _vertical = firstPoint.Position.X;
                    break;
                case EquirectangularStencilMode.LeftRightLines:
                    {
                        var point = new Point(_height / 2, _height / 2);
                        var anchorPoint = Vertex.CreateVertex(point, _height);

                        // the first point cannot be coincident with the anchor point  
                        firstPoint = (from item in args.NewInkPoints
                                      where item.Position != point
                                      select item).FirstOrDefault();

                        if (firstPoint == null)
                        {
                            args.Disposition = CoreWetStrokeDisposition.Canceled;

                            return;
                        }

                        var vertexPoint = Vertex.CreateVertex(firstPoint.Position, _height);

                        _apex = Equirectangular.ApexOf(anchorPoint, vertexPoint);

                        AddPointsOnCurve(args);
                    }
                    break;

                case EquirectangularStencilMode.FrontBackLines:
                    {
                        var point = new Point(_height, _height / 2);

                        // the first point cannot be coincident with the anchor point  
                        firstPoint = (from item in args.NewInkPoints
                                      where item.Position != point
                                      select item).FirstOrDefault();

                        if (firstPoint == null)
                        {
                            args.Disposition = CoreWetStrokeDisposition.Canceled;

                            return;
                        }

                        var anchorPoint = Vertex.CreateVertex(point, _height);

                        var vertexPoint = Vertex.CreateVertex(firstPoint.Position, _height);

                        _apex = Equirectangular.ApexOf(anchorPoint, vertexPoint);

                        AddPointsOnCurve(args);
                    }
                    break;

                case EquirectangularStencilMode.TwoPoint:
                    AddPointsOnCurve(args);
                    break;
            }
        }

        #endregion
    }
}
