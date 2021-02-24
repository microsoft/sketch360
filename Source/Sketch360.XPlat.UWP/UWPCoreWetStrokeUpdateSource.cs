// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Core;
using Xamarin.Forms.Inking;
using Xamarin.Forms.Inking.Interfaces;

namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// UWP CoreWetStrokeUpdateSource
    /// </summary>
    public sealed class UWPCoreWetStrokeUpdateSource : XCoreWetStrokeUpdateSource
    {
        #region Fields
        private readonly CoreWetStrokeUpdateSource _updateSource;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the UWPCoreWetStrokeUpdateSource class
        /// </summary>
        /// <param name="inkPresenter">the ink presenter</param>
        public UWPCoreWetStrokeUpdateSource(IInkPresenter inkPresenter) :
            base(inkPresenter)
        {
            if (inkPresenter is UWPInkPresenter uwpInkPresenter)
            {
                _updateSource = CoreWetStrokeUpdateSource.Create(uwpInkPresenter.InkCanvas.InkPresenter);

                _updateSource.WetStrokeCanceled += OnWetStrokeCanceled;
                _updateSource.WetStrokeCompleted += OnWetStrokeCompleted;
                _updateSource.WetStrokeStarting += OnWetStrokeStarting;
                _updateSource.WetStrokeStopping += OnWetStrokeStopping;
                _updateSource.WetStrokeContinuing += OnWetStrokeContinuing;
            }
        }
        #endregion

        #region Implementation
        private static void UpdateEventArgs(CoreWetStrokeUpdateEventArgs args, XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            switch (updateArgs.Disposition)
            {
                case XCoreWetStrokeDisposition.Canceled:
                    args.Disposition = CoreWetStrokeDisposition.Canceled;
                    break;

                case XCoreWetStrokeDisposition.Completed:
                    args.Disposition = CoreWetStrokeDisposition.Completed;
                    break;

                case XCoreWetStrokeDisposition.Inking:
                    args.Disposition = CoreWetStrokeDisposition.Inking;
                    break;
            }

            args.NewInkPoints.Clear();

            foreach (var item in updateArgs.NewInkPoints)
            {
                args.NewInkPoints.Add(new InkPoint(
                    new Windows.Foundation.Point(item.Position.X, item.Position.Y),
                    item.Pressure,
                    item.TiltX,
                    item.TiltY,
                    item.Timestamp));
            }
        }

        private static XCoreWetStrokeUpdateEventArgs CreateUpdateArgs(CoreWetStrokeUpdateEventArgs args)
        {
            var updateArgs = new XCoreWetStrokeUpdateEventArgs
            {
                PointerId = args.PointerId,
                NewInkPoints = (from item in args.NewInkPoints
                                select new XInkPoint(new Xamarin.Forms.Point(item.Position.X, item.Position.Y), item.Pressure, item.TiltX, item.TiltY, item.Timestamp)).ToList()
            };

            switch (args.Disposition)
            {
                case CoreWetStrokeDisposition.Canceled:
                    updateArgs.Disposition = XCoreWetStrokeDisposition.Canceled;
                    break;

                case CoreWetStrokeDisposition.Completed:
                    updateArgs.Disposition = XCoreWetStrokeDisposition.Completed;
                    break;

                case CoreWetStrokeDisposition.Inking:
                    updateArgs.Disposition = XCoreWetStrokeDisposition.Inking;
                    break;
            }

            return updateArgs;
        }

        private void OnWetStrokeContinuing(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            var updateArgs = CreateUpdateArgs(args);

            InvokeWetStrokeContinuing(updateArgs);

            UpdateEventArgs(args, updateArgs);
        }

        private void OnWetStrokeStopping(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            var updateArgs = CreateUpdateArgs(args);

            InvokeWetStrokeStopping(updateArgs);

            UpdateEventArgs(args, updateArgs);
        }

        private void OnWetStrokeStarting(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            XCoreWetStrokeUpdateEventArgs updateArgs = CreateUpdateArgs(args);

            InvokeWetStrokeStarting(updateArgs);

            UpdateEventArgs(args, updateArgs);
        }

        private void OnWetStrokeCompleted(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            XCoreWetStrokeUpdateEventArgs updateArgs = CreateUpdateArgs(args);

            InvokeWetStrokeCompleted(updateArgs);

            UpdateEventArgs(args, updateArgs);
        }

        private void OnWetStrokeCanceled(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            XCoreWetStrokeUpdateEventArgs updateArgs = CreateUpdateArgs(args);

            InvokeWetStrokeCanceled(updateArgs);

            UpdateEventArgs(args, updateArgs);
        }
        #endregion
    }
}
