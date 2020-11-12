// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Xamarin PointerPoint
    /// </summary>
    /// <remarks>Similar to Windows.UI.Input.PointerPoint: <![CDATA[https://docs.microsoft.com/en-us/uwp/api/Windows.UI.Input.PointerPoint?view=winrt-19041]]></remarks>
    public sealed class XPointerPoint
    {
        /// <summary>
        /// Gets the position
        /// </summary>
        public Point Position { get; internal set; }

        /// <summary>
        /// Gets the timestamp
        /// </summary>
        public ulong Timestamp { get; internal set; }

        /// <summary>
        /// Gets the pointer id
        /// </summary>
        public uint PointerId { get; internal set; }

        /// <summary>
        /// gets the pointer device
        /// </summary>
        public XPointerDevice PointerDevice { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the pointer is in contact
        /// </summary>
        public bool IsInContact { get; internal set; }
    }
}
