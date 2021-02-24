// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Invalidate canvas event arguments
    /// </summary>
    public sealed class InvalidateCanvasEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the InvalidatCanvasEventArgs class
        /// </summary>
        /// <param name="wetInk"></param>
        /// <param name="dryInk"></param>
        public InvalidateCanvasEventArgs(bool wetInk, bool dryInk)
        {
            WetInk = wetInk;
            DryInk = dryInk;
        }

        /// <summary>
        /// Gets a value indicating whether to invalidate the wet ink
        /// </summary>
        public bool WetInk { get; }

        /// <summary>
        /// Gets a value indicating whether to invalidate the dry ink
        /// </summary>
        public bool DryInk { get; }
    }
}
