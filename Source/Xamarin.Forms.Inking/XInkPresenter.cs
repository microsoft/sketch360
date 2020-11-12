// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Inking.Interfaces;
using Xamarin.Forms.Inking.Support;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Xamarain InkPresenter
    /// </summary>
    public sealed class XInkPresenter : IInkPresenter
    {
        private readonly XInkDrawingAttributes _defaultDrawingAttriubtes = new XInkDrawingAttributes();

        /// <summary>
        /// Initializes a new instance of the <see cref="XInkPresenter"/> class
        /// </summary>
        /// <param name="inkCanvasView">the <see cref="InkCanvasView"/></param>
        internal XInkPresenter(IInkCanvasView inkCanvasView)
        {
            InkCanvasView = inkCanvasView ?? throw new ArgumentNullException(nameof(inkCanvasView));

            UnprocessedInput = new XInkUnprocessedInput(this);

            StrokeContainer = new XInkStrokeContainer();
        }

        /// <summary>
        /// Strokes collected event
        /// </summary>
        public event TypedEventHandler<IInkPresenter, XInkStrokesCollectedEventArgs> StrokesCollected;

        /// <summary>
        /// Strokes erased event
        /// </summary>
        public event TypedEventHandler<XInkPresenter, XInkStrokesErasedEventArgs> StrokesErased;

        /// <summary>
        /// Gets or sets the stroke container
        /// </summary>
        public IInkStrokeContainer StrokeContainer { get; set; }

        /// <summary>
        /// Gets or sets the eraser size
        /// </summary>
        public double EraserSize { get; set; } = 8.0;

        /// <summary>
        /// Gets or sets the input device type from which input data is collected by the InkPresenter to construct and render an InkStroke. The default is Pen.
        /// </summary>
        public XCoreInputDeviceTypes InputDeviceTypes { get; set; } = XCoreInputDeviceTypes.Pen;

        /// <summary>
        /// Gets the ink canvas view
        /// </summary>
        public IInkCanvasView InkCanvasView { get; }

        /// <summary>
        /// Gets the unprocessed input
        /// </summary>
        public XInkUnprocessedInput UnprocessedInput { get; }

        /// <summary>
        /// Gets the input configuration
        /// </summary>
        public XInkInputConfiguration InputConfiguration { get; } = new XInkInputConfiguration();

        /// <summary>
        /// Gets the input processing configuration
        /// </summary>
        public XInkInputProcessingConfiguration InputProcessingConfiguration { get; } = new XInkInputProcessingConfiguration();

        /// <summary>
        /// Update the default drawing attributes
        /// </summary>
        /// <param name="attributes"></param>
        public void UpdateDefaultDrawingAttributes(XInkDrawingAttributes attributes)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));

            lock (_defaultDrawingAttriubtes)
            {
                _defaultDrawingAttriubtes.Color = attributes.Color;
                _defaultDrawingAttriubtes.Size = attributes.Size;
            }
        }

        /// <summary>
        /// Copy the default darwing attributes
        /// </summary>
        /// <returns>a copy of the default drawing attributes</returns>
        public XInkDrawingAttributes CopyDefaultDrawingAttributes()
        {
            lock (_defaultDrawingAttriubtes)
            {
                return _defaultDrawingAttriubtes.Copy();
            }
        }

        /// <summary>
        /// Gets or sets the wet stroke update source
        /// </summary>
        public XCoreWetStrokeUpdateSource WetStrokeUpdateSource { get; set; }

        /// <summary>
        /// Trigger the strokes erased event
        /// </summary>
        /// <param name="list">the list of strokes</param>
        public void TriggerStrokesErased(IReadOnlyList<XInkStroke> list)
        {
            StrokesErased?.Invoke(this, new XInkStrokesErasedEventArgs(list));
        }

        /// <summary>
        /// Trigger the strokes collected event handler
        /// </summary>
        /// <param name="strokes">the strokes</param>
        public void TriggerStrokesCollected(IEnumerable<XInkStroke> strokes)
        {
            StrokesCollected?.Invoke(this, new XInkStrokesCollectedEventArgs
            {
                Strokes = strokes.ToList()
            });
        }
    }
}
