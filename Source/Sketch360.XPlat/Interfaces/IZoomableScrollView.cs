// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Sketch360.XPlat
{
    /// <summary>
    /// Zoomable scrollview interface
    /// </summary>
    public interface IZoomableScrollView
    {
        /// <summary>
        /// Gets or sets a value indicating whether the scrollview is zoomable
        /// </summary>
        bool IsZoomEnabled { get; set; }
    }
}
