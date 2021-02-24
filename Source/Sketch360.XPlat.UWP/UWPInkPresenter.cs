// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.UWP;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Inking;
using Xamarin.Forms.Inking.Interfaces;
using Xamarin.Forms.Inking.Support;

[assembly: Xamarin.Forms.Dependency(typeof(UWPInkPresenter))]

namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// UWP InkPresenter
    /// </summary>
    public class UWPInkPresenter : IInkPresenter
    {
        private InkCanvas _inkCanvas;

        /// <summary>
        /// Initializes a new instance of the UWPInkPresenter class
        /// </summary>
        public UWPInkPresenter()
        {
            InputProcessingConfiguration = new XInkInputProcessingConfiguration();

            StrokeContainer = new UWPInkStrokeContainer();
        }

        /// <summary>
        /// Gets or sets the ink canvas
        /// </summary>
        public InkCanvas InkCanvas
        {
            get => _inkCanvas;

            set
            {
                _inkCanvas = value;

                (StrokeContainer as UWPInkStrokeContainer).NativeStrokeContainer = _inkCanvas.InkPresenter.StrokeContainer;
            }
        }

        /// <summary>
        /// Gets or sets the stroke container
        /// </summary>
        public Xamarin.Forms.Inking.Interfaces.IInkStrokeContainer StrokeContainer { get; set; }

        XCoreInputDeviceTypes _inputDeviceTypes = XCoreInputDeviceTypes.Pen;

        /// <summary>
        /// Gets or sets the input device types
        /// </summary>
        public XCoreInputDeviceTypes InputDeviceTypes
        {
            get
            {
                if (_inkCanvas != null)
                {
                    _inputDeviceTypes = GetXInputDeviceTypes(_inkCanvas.InkPresenter.InputDeviceTypes);
                }

                return _inputDeviceTypes;
            }
            set
            {
                _inputDeviceTypes = value;
                if (_inkCanvas != null)
                {
                    _inkCanvas.InkPresenter.InputDeviceTypes = GetInputDeviceTypes(value);
                }
            }

        }

        internal static XCoreInputDeviceTypes GetXInputDeviceTypes(CoreInputDeviceTypes inputDeviceTypes)
        {
            XCoreInputDeviceTypes deviceTypes = XCoreInputDeviceTypes.None;

            if (inputDeviceTypes.HasFlag(CoreInputDeviceTypes.Mouse))
            {
                deviceTypes |= XCoreInputDeviceTypes.Mouse;
            }

            if (inputDeviceTypes.HasFlag(CoreInputDeviceTypes.Pen))
            {
                deviceTypes |= XCoreInputDeviceTypes.Pen;
            }

            if (inputDeviceTypes.HasFlag(CoreInputDeviceTypes.Touch))
            {
                deviceTypes |= XCoreInputDeviceTypes.Touch;
            }

            return deviceTypes;
        }

        internal static CoreInputDeviceTypes GetInputDeviceTypes(XCoreInputDeviceTypes inputDeviceTypes)
        {
            CoreInputDeviceTypes deviceTypes = CoreInputDeviceTypes.None;

            if (inputDeviceTypes.HasFlag(XCoreInputDeviceTypes.Mouse))
            {
                deviceTypes |= CoreInputDeviceTypes.Mouse;
            }

            if (inputDeviceTypes.HasFlag(XCoreInputDeviceTypes.Pen))
            {
                deviceTypes |= CoreInputDeviceTypes.Pen;
            }

            if (inputDeviceTypes.HasFlag(XCoreInputDeviceTypes.Touch))
            {
                deviceTypes |= CoreInputDeviceTypes.Touch;
            }

            return deviceTypes;
        }

        /// <summary>
        /// Gets the ink input processing configuration
        /// </summary>
        public XInkInputProcessingConfiguration InputProcessingConfiguration { get; }

        /// <summary>
        /// Gets or sets the unprocessed input
        /// </summary>
        public XInkUnprocessedInput UnprocessedInput => throw new NotImplementedException();

        /// <summary>
        /// Gets or sets the wet stroke update source
        /// </summary>
        public XCoreWetStrokeUpdateSource WetStrokeUpdateSource { get; set; }

        /// <summary>
        /// strokes collected event handler
        /// </summary>
        public event TypedEventHandler<IInkPresenter, XInkStrokesCollectedEventArgs> StrokesCollected;

        /// <summary>
        /// Strokes erased event handler;
        /// </summary>
        public event TypedEventHandler<XInkPresenter, XInkStrokesErasedEventArgs> StrokesErased;

        /// <summary>
        /// Copy the default drawing attributes
        /// </summary>
        /// <returns></returns>
        public XInkDrawingAttributes CopyDefaultDrawingAttributes()
        {
            var attributes = InkCanvas.InkPresenter.CopyDefaultDrawingAttributes();

            return attributes.ToXInkDrawingAttributes();
        }

        /// <summary>
        /// Trigger the strokes collected event
        /// </summary>
        /// <param name="strokes">the strokes</param>
        public void TriggerStrokesCollected(IEnumerable<XInkStroke> strokes)
        {
            StrokesCollected?.Invoke(this, new XInkStrokesCollectedEventArgs
            {
                Strokes = strokes.ToList()
            });
        }

        /// <summary>
        /// Trigger the strokes erased event
        /// </summary>
        /// <param name="list">the strokes</param>
        public void TriggerStrokesErased(IReadOnlyList<XInkStroke> list)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the default drawing attributes
        /// </summary>
        /// <param name="attributes">the ink drawing attributes</param>
        public void UpdateDefaultDrawingAttributes(XInkDrawingAttributes attributes)
        {
            var attributes2 = attributes.ToInkDrawingAttributes();

            InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(attributes2);
        }
    }
}
