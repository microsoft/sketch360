// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Point = Xamarin.Forms.Point;

namespace Sketch360.Core.Support
{
    /// <summary>
    /// de Cateljau algorithm for bezier points
    /// </summary>
    public static class Casteljau
    {
        /// <summary>
        /// From <![CDATA[https://www.daniweb.com/programming/software-development/threads/389117/drawing-bezier-curves-using-de-casteljau-algorithm-in-c-open-gl]]>
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <param name="D"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Point GetBezierPoint(Point A, Point B, Point C, Point D, double t)
        {
            var P = new Point
            {
                X = Math.Pow((1 - t), 3) * A.X + 3 * t * Math.Pow((1 - t), 2) * B.X + 3 * (1 - t) * Math.Pow(t, 2) * C.X + Math.Pow(t, 3) * D.X,
                Y = Math.Pow((1 - t), 3) * A.Y + 3 * t * Math.Pow((1 - t), 2) * B.Y + 3 * (1 - t) * Math.Pow(t, 2) * C.Y + Math.Pow(t, 3) * D.Y
            };
            //P.z = Math.Pow((1 - t), 3) * A.z + 3 * t * pow((1 - t), 2) * B.z + 3 * (1 - t) * pow(t, 2) * C.z + pow(t, 3) * D.z;

            return P;
        }
    }
}
