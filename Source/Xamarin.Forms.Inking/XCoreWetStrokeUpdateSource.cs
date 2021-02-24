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
    public class XCoreWetStrokeUpdateSource
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the XCoreWetStrokeUpdateSource class.
        /// </summary>
        /// <param name="inkPresenter">the ink presenter</param>
        protected XCoreWetStrokeUpdateSource(IInkPresenter inkPresenter)
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

            InvokeWetStrokeCanceled(updateArgs);
        }

        /// <summary>
        /// Invoke the WetStrokeCanceled event
        /// </summary>
        /// <param name="updateArgs">the update arguments</param>
        protected void InvokeWetStrokeCanceled(XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            WetStrokeCanceled?.Invoke(this, updateArgs);
        }

        /// <summary>
        /// On pointer Moved
        /// </summary>
        /// <param name="updateArgs"></param>
        internal void OnMoved(XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            InvokeWetStrokeContinuing(updateArgs);

            if (updateArgs.Disposition == XCoreWetStrokeDisposition.Completed)
            {
                InvokeWetStrokeCompleted(updateArgs);
            }
        }

        /// <summary>
        /// Invoke the WetStrokeContinuing event
        /// </summary>
        /// <param name="updateArgs">the update arguments</param>
        protected void InvokeWetStrokeContinuing(XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            WetStrokeContinuing?.Invoke(this, updateArgs);
        }

        /// <summary>
        /// On pointer released
        /// </summary>
        /// <param name="updateArgs"></param>
        internal void OnReleased(XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            InvokeWetStrokeStopping(updateArgs);

            if (updateArgs.Disposition == XCoreWetStrokeDisposition.Canceled) return;

            InvokeWetStrokeCompleted(updateArgs);
        }

        /// <summary>
        /// Invoke the WetStrokeCompleted event
        /// </summary>
        /// <param name="updateArgs">the update arguments</param>
        protected void InvokeWetStrokeCompleted(XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            WetStrokeCompleted?.Invoke(this, updateArgs);
        }

        /// <summary>
        /// Invoke the WetStrokeStopping event
        /// </summary>
        /// <param name="updateArgs">the update arguments</param>
        protected void InvokeWetStrokeStopping(XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            WetStrokeStopping?.Invoke(this, updateArgs);
        }

        /// <summary>
        /// Invoke the wet stroke starting event
        /// </summary>
        /// <param name="updateArgs">the update arggs</param>
        protected void InvokeWetStrokeStarting(XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            WetStrokeStarting?.Invoke(this, updateArgs);
        }

        /// <summary>
        /// On Pointer pressed
        /// </summary>
        /// <param name="updateArgs"></param>
        internal void OnPressed(XCoreWetStrokeUpdateEventArgs updateArgs)
        {
            InvokeWetStrokeStarting(updateArgs);

            if (updateArgs.Disposition == XCoreWetStrokeDisposition.Canceled) return;

            InvokeWetStrokeContinuing(updateArgs);

            if (updateArgs.Disposition == XCoreWetStrokeDisposition.Completed)
            {
                InvokeWetStrokeCompleted(updateArgs);
            }
        }
        #endregion
    }
}
