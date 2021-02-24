// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Ink stroke builder
    /// </summary>
    public sealed class XInkStrokeBuilder
    {
        XInkDrawingAttributes _attributes = new XInkDrawingAttributes();

        /// <summary>
        /// Creates a strokes from ink points
        /// </summary>
        /// <param name="inkPoints">the ink points</param>
        /// <returns>a new ink stroke</returns>
        public XInkStroke CreateStrokeFromInkPoints(IEnumerable<XInkPoint> inkPoints)
        {
            if (inkPoints == null) throw new ArgumentNullException(nameof(inkPoints));

            var stroke = new XInkStroke
            {
                DrawingAttributes = _attributes.Copy()
            };

            stroke.AddRange(inkPoints);

            return stroke;
        }

        /// <summary>
        /// Sets the default drawing attributes
        /// </summary>
        /// <param name="attributes"></param>
        public void SetDefaultDrawingAttributes(XInkDrawingAttributes attributes)
        {
            _attributes = attributes;
        }
    }
}
