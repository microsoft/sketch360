// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Color = Xamarin.Forms.Color;
using Point = Xamarin.Forms.Point;

namespace Sketch360.Core.Support
{
    /// <summary>
    /// Bezier points for equirectangular guide
    /// </summary>
    public class BezierPoints
    {
        /// <summary>
        /// Gets or sets the color for the stencil lines
        /// </summary>
        public Color Color { get; set; }
        /// <summary>
        /// Gets or sets point 0
        /// </summary>
        public Point Point0 { get; set; }

        /// <summary>
        /// Gets or sets point 1
        /// </summary>
        public Point Point1 { get; set; }

        /// <summary>
        /// Gets or sets point 2
        /// </summary>
        public Point Point2 { get; set; }

        /// <summary>
        /// Gets or sets point 3
        /// </summary>
        public Point Point3 { get; set; }


        /// <summary>
        /// Gets or sets the curve points
        /// </summary>
        public List<Point> CurvePoints { get; set; }


    }
}
