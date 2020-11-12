// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Xamarin InkDrawingAttributes
    /// </summary>
    public class XInkDrawingAttributes
    {
        /// <summary>
        /// Gets or sets the ink stroke color (should invalidate the paint)
        /// </summary>
        public Color Color { get; set; } = Color.Black;

        /// <summary>
        /// Gets or sets the pen width (should invalidate the paint)
        /// </summary>
        public float Size { get; set; } = 4.0f;

        /// <summary>
        /// Gets the pen tip shape
        /// </summary>
        public XPenTipShape PenTip { get; set; } = XPenTipShape.Circle;

        /// <summary>
        /// Gets or sets the ink kind
        /// </summary>
        public XInkDrawingAttributesKind Kind { get; set; } = XInkDrawingAttributesKind.Default;

        /// <summary>
        /// Gets or sets a value that indicates whether the pressure of the contact on the digitizer surface is ignored when you draw an <see cref="XInkStroke"/>.
        /// </summary>
        public bool IgnorePressure { get; set; }

        /// <summary>
        /// Copy the ink drawing attributes
        /// </summary>
        /// <returns>a copy of the ink drawing attributes</returns>
        public XInkDrawingAttributes Copy()
        {
            return new XInkDrawingAttributes
            {
                Color = Color,
                Size = Size,
                PenTip = PenTip,
                Kind = Kind,
                IgnorePressure = IgnorePressure
            };
        }
    }
}
