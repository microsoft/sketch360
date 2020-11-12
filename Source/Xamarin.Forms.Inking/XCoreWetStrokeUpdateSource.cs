// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xamarin.Forms.Inking.Interfaces;
using Xamarin.Forms.Inking.Support;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Xamarin CoreWetStrokeUpdateSource
    /// <![CDATA[https://docs.microsoft.com/en-us/uwp/api/windows.ui.input.inking.core.corewetstrokeupdatesource]]>
    /// </summary>
    public sealed class XCoreWetStrokeUpdateSource
    {
        #region Constructors
        private XCoreWetStrokeUpdateSource(IInkPresenter inkPresenter)
        {
            InkPresenter = inkPresenter;

            InkPresenter.WetStrokeUpdateSource = this;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the ink presenter
        /// </summary>
        public IInkPresenter InkPresenter { get; }
        #endregion

        #region Events
        /// <summary>
        /// Wet stroke canceled event
        /// </summary>
        public event TypedEventHandler<XCoreWetStrokeUpdateSource, XCoreWetStrokeUpdateEventArgs> WetStrokeCanceled;

        /// <summary>
        /// Wet stroke completed event
        /// </summary>
        public event TypedEventHandler<XCoreWetStrokeUpdateSource, XCoreWetStrokeUpdateEventArgs> WetStrokeCompleted;

        /// <summary>
        /// Wet stroke continuing event
        /// </summary>
        public event TypedEventHandler<XCoreWetStrokeUpdateSource, XCoreWetStrokeUpdateEventArgs> WetStrokeContinuing;

        /// <summary>
        /// Wet stroke starging event
        /// </summary>
        public event TypedEventHandler<XCoreWetStrokeUpdateSource, XCoreWetStrokeUpdateEventArgs> WetStrokeStarting;

        /// <summary>
        /// Wet stroke stopping event
        /// </summary>
        public event TypedEventHandler<XCoreWetStrokeUpdateSource, XCoreWetStrokeUpdateEventArgs> WetStrokeStopping;

        #endregion

        #region Methods
        /// <summary>
        /// Creates the wet stroke update source
        /// </summary>
        /// <param name="inkPresenter">the ink presenter</param>
        /// <returns>a new <see cref="XCoreWetStrokeUpdateSource"/> object</returns>
        public static XCoreWetStrokeUpdateSource Create(IInkPresenter inkPresenter)
        {
            if (inkPresenter == null) throw new ArgumentNullException(nameof(inkPresenter));

            return new XCoreWetStrokeUpdateSource(inkPresenter);
        }

        /// <summary>
        /// wet stroke cancelled 
        /// </summary>
        /// <param name="updateArgs">Core wet stroke update event args</param>
        internal void OnCancelled(XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            updateArgs.Disposition = XCoreWetStrokeDisposition.Canceled;

            WetStrokeCanceled?.Invoke(this, updateArgs);
        }

        /// <summary>
        /// On pointer Moved
        /// </summary>
        /// <param name="updateArgs"></param>
        internal void OnMoved(XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            WetStrokeContinuing?.Invoke(this, updateArgs);

            if (updateArgs.Disposition == XCoreWetStrokeDisposition.Completed)
            {
                WetStrokeCompleted?.Invoke(this, updateArgs);
            }
        }

        /// <summary>
        /// On pointer released
        /// </summary>
        /// <param name="updateArgs"></param>
        internal void OnReleased(XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            WetStrokeStopping?.Invoke(this, updateArgs);

            if (updateArgs.Disposition == XCoreWetStrokeDisposition.Canceled) return;

            WetStrokeCompleted?.Invoke(this, updateArgs);
        }

        /// <summary>
        /// On Pointer pressed
        /// </summary>
        /// <param name="updateArgs"></param>
        internal void OnPressed(XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            WetStrokeStarting?.Invoke(this, updateArgs);

            if (updateArgs.Disposition == XCoreWetStrokeDisposition.Canceled) return;

            WetStrokeContinuing?.Invoke(this, updateArgs);

            if (updateArgs.Disposition == XCoreWetStrokeDisposition.Completed)
            {
                WetStrokeCompleted?.Invoke(this, updateArgs);
            }
        }
        #endregion
    }
}
