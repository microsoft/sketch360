// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Ink strokes erased event arguments
    /// </summary>
    public sealed class XInkStrokesErasedEventArgs : EventArgs
    {
        /// <summary>
        /// Xamarin XInkStrokesErasedEventArgs
        /// </summary>
        /// <param name="strokes"></param>
        internal XInkStrokesErasedEventArgs(IReadOnlyList<XInkStroke> strokes)
        {
            Strokes = strokes;
        }

        /// <summary>
        /// Gets the strokes
        /// </summary>
        public IReadOnlyList<XInkStroke> Strokes { get; }
    }
}
