// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Xamarin.Forms.Inking.Support;

namespace Xamarin.Forms.Inking.Interfaces
{
    /// <summary>
    /// Ink presenter interface
    /// </summary>
    public interface IInkPresenter
    {
        /// <summary>
        /// Gets or sets the stroke container
        /// </summary>
        IInkStrokeContainer StrokeContainer { get; set; }

        /// <summary>
        /// Gets or sets the input device types
        /// </summary>
        XCoreInputDeviceTypes InputDeviceTypes { get; set; }

        /// <summary>
        /// gets the input processing configuration
        /// </summary>
        XInkInputProcessingConfiguration InputProcessingConfiguration { get; }

        /// <summary>
        /// Gets the unprocessed input
        /// </summary>
        XInkUnprocessedInput UnprocessedInput { get; }

        /// <summary>
        /// Gets the wet stroke update source
        /// </summary>
        XCoreWetStrokeUpdateSource WetStrokeUpdateSource { get; set; }

        /// <summary>
        /// strokes collected event
        /// </summary>
        event TypedEventHandler<IInkPresenter, XInkStrokesCollectedEventArgs> StrokesCollected;

        /// <summary>
        /// strokes erased event
        /// </summary>
        event TypedEventHandler<XInkPresenter, XInkStrokesErasedEventArgs> StrokesErased;

        /// <summary>
        /// Copy the default drawing attributes
        /// </summary>
        /// <returns>the current default drawing attributes</returns>
        XInkDrawingAttributes CopyDefaultDrawingAttributes();

        /// <summary>
        /// Trigger the strokes collected event
        /// </summary>
        /// <param name="strokes">the strokes</param>
        void TriggerStrokesCollected(IEnumerable<XInkStroke> strokes);

        /// <summary>
        /// Trigger the strokes erased event
        /// </summary>
        /// <param name="list">the strokes to erase</param>
        void TriggerStrokesErased(IReadOnlyList<XInkStroke> list);

        /// <summary>
        /// Update the default drawing attributes
        /// </summary>
        /// <param name="attributes"></param>
        void UpdateDefaultDrawingAttributes(XInkDrawingAttributes attributes);
    }
}
