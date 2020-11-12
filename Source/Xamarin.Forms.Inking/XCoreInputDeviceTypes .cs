// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Core input device types for inking
    /// </summary>
    [Flags]
    public enum XCoreInputDeviceTypes
    {
        /// <summary>
        /// No device type
        /// </summary>
        None = 0,

        /// <summary>
        /// Touch device
        /// </summary>
        Touch = 1,

        /// <summary>
        /// Pen device
        /// </summary>
        Pen = 2,

        /// <summary>
        /// Mouse device
        /// </summary>
        Mouse = 4
    }
}
