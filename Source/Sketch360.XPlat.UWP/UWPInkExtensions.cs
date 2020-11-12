using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Xamarin.Forms.Inking;
using Xamarin.Forms.Platform.UWP;

namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// Extensions to convert between UWP Inking and Xamarin.Forms.Inking classes
    /// </summary>
    public static class UWPInkExtensions
    {
        /// <summary>
        /// Convert a UWP InkStroke list to a XInkStroke List
        /// </summary>
        /// <param name="strokes">a collection of UWP ink strokes</param>
        /// <returns>a collection of Xamarin Ink strokes</returns>
        public static IReadOnlyList<XInkStroke> ToXInkStrokeList(this IEnumerable<InkStroke> strokes)
        {
            var xInkStrokes = from stroke in strokes
                              let attributes = stroke.DrawingAttributes
                              let points = stroke.GetInkPoints()
                              select new XInkStroke
                              {
                                  DrawingAttributes = attributes.ToXInkDrawingAttributes(),
                                  Id = stroke.Id.ToString(CultureInfo.InvariantCulture),
                                  Points = (from point in points
                                            select new XInkPoint
                                            {
                                                Position = new Xamarin.Forms.Point(point.Position.X, point.Position.Y),
                                                Pressure = point.Pressure,
                                                Timestamp = point.Timestamp,
                                                TiltX = point.TiltX,
                                                TiltY = point.TiltY
                                            }).ToList()
                              };

            return xInkStrokes.ToList();
        }

        /// <summary>
        /// Convert a Xamarin Ink Stroke to a UWP InkStroke
        /// </summary>
        /// <param name="xInkStroke">a Xamarin Ink Stroke</param>
        /// <returns>a UWP Ink Stroke</returns>
        public static InkStroke ToInkStroke(this XInkStroke xInkStroke)
        {
            if (xInkStroke == null) throw new ArgumentNullException(nameof(xInkStroke));

            var builder = new InkStrokeBuilder();

            var inkPoints = from item in xInkStroke.GetInkPoints()
                            select new InkPoint(new Point(item.Position.X, item.Position.Y), item.Pressure);

            if (!inkPoints.Any())
            {
                return null;
            }

            var stroke = builder.CreateStrokeFromInkPoints(inkPoints, Matrix3x2.Identity);

            stroke.DrawingAttributes = xInkStroke.DrawingAttributes.ToInkDrawingAttributes();

            return stroke;
        }


        /// <summary>
        /// Convert a UWP ink drawing attributes to Xamarin 
        /// </summary>
        /// <param name="attributes">a UWP ink drawing attributes</param>
        /// <returns>a Xamarin ink drawing attributes</returns>
        public static XInkDrawingAttributes ToXInkDrawingAttributes(this InkDrawingAttributes attributes)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));

            return new XInkDrawingAttributes
            {
                Color = attributes.Color.ToFormsColor(),
                IgnorePressure = attributes.IgnorePressure,
                Kind = attributes.Kind == Windows.UI.Input.Inking.InkDrawingAttributesKind.Default ? XInkDrawingAttributesKind.Default : XInkDrawingAttributesKind.Pencil,
                PenTip = attributes.PenTip == Windows.UI.Input.Inking.PenTipShape.Circle ? XPenTipShape.Circle : XPenTipShape.Rectangle,
                Size = Convert.ToSingle(attributes.Size.Height)
            };
        }

        /// <summary>
        /// Convert a Xamarin Ink drawing attributes to UWP
        /// </summary>
        /// <param name="attributes">Xamarin Ink Drawing attributes</param>
        /// <returns>Xamarin ink drawing attributes</returns>
        public static InkDrawingAttributes ToInkDrawingAttributes(this XInkDrawingAttributes attributes)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));

            InkDrawingAttributes attributes2;

            if (attributes.Kind == XInkDrawingAttributesKind.Pencil)
            {
                attributes2 = InkDrawingAttributes.CreateForPencil();
            }
            else
            {
                attributes2 = new InkDrawingAttributes();
            }

            attributes2.Color = attributes.Color.ToWindowsColor();
            attributes2.IgnorePressure = attributes.IgnorePressure;
            attributes2.Size = new Windows.Foundation.Size(Convert.ToDouble(attributes.Size), Convert.ToDouble(attributes.Size));
            attributes2.PenTip = attributes.PenTip == XPenTipShape.Circle ? PenTipShape.Circle : PenTipShape.Rectangle;

            return attributes2;
        }
    }
}
