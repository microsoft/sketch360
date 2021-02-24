// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Xamarin CoreWetStrokeUpdateEventArgs
    /// </summary>
    public sealed class XCoreWetStrokeUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the disposition
        /// </summary>
        public XCoreWetStrokeDisposition Disposition { get; set; }

        /// <summary>
        /// Gets the new ink points
        /// </summary>
        public IList<XInkPoint> NewInkPoints { get; set; }

        /// <summary>
        /// Gets the pointer id
        /// </summary>
        public uint PointerId { get; set; }
    }
}
