// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Sketch360.Core.Support
{
    /// <summary>
    /// Equirectangular stencil modes
    /// </summary>
    public enum EquirectangularStencilMode
    {
        /// <summary>
        /// Equirectangular stencil is off
        /// </summary>
        None,

        /// <summary>
        /// Snap to vertical lines
        /// </summary>
        VerticalLines,

        /// <summary>
        /// snap to front/back vanishing points
        /// </summary>
        FrontBackLines,

        /// <summary>
        /// snap to left/right vanishing points
        /// </summary>
        LeftRightLines,

        /// <summary>
        /// Snap to line defined by two points
        /// </summary>
        TwoPoint,
    }
}
