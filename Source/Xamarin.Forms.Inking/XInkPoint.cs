// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Xamarin InkPoint
    /// </summary>
    public sealed class XInkPoint
    {
        /// <summary>
        /// Initializes a new instance of the XInkPoint class.
        /// </summary>
        /// <remarks>Called from JSON Serialization</remarks>
        public XInkPoint()
        {
            Timestamp = Convert.ToUInt64(DateTimeOffset.UtcNow.Ticks);
        }

        /// <summary>
        /// Initializes a new instance of the XInkPoint class.
        /// </summary>
        public XInkPoint(Point position, float pressure,
            float tiltX, float tiltY, ulong timeStamp)
        {
            Position = position;
            Pressure = pressure;
            TiltX = tiltX;
            TiltY = tiltY;
            Timestamp = timeStamp;
        }

        /// <summary>
        /// Gets or sets the pressure
        /// </summary>
        public float Pressure { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the position
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public ulong Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the X tilt
        /// </summary>
        public float TiltX { get; set; }

        /// <summary>
        /// Gets or sets the Y tilt
        /// </summary>
        public float TiltY { get; set; }
    }
}
