// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Xamarin PointerEventArgs
    /// </summary>
    public sealed class XPointerEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current point
        /// </summary>
        public XPointerPoint CurrentPoint { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether the poitner event was handled
        /// </summary>
        public bool Handled { get; set; }
    }
}
