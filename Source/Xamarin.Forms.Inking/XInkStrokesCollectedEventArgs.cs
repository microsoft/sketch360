// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Xamarin InkStrokeCollectedEventArgs
    /// </summary>
    public sealed class XInkStrokesCollectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the Strokes
        /// </summary>
        public IReadOnlyList<XInkStroke> Strokes { get; set; }
    }
}
