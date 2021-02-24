// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Xamarin.Forms.Inking.Support;
namespace Xamarin.Forms.Inking.Tests
{
    [TestClass]
    public class TestInkRenderer
    {
        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void CreatePathPointsNullStroke()
        {
            InkRenderer.CreatePathPoints(null);
        }

        [TestMethod]
        public void TestCreatePathPointsEmpty()
        {
            using var inkStroke = new XInkStroke();
            InkRenderer.CreatePathPoints(inkStroke);
        }

        [TestMethod]
        public void TestCreatePathPointsOnePoint()
        {
            var builder = new XInkStrokeBuilder();

            var inkPoints = new[] { new XInkPoint(new Point(0, 0), 1f, 0f, 0f, 0) };

            using var stroke = builder.CreateStrokeFromInkPoints(inkPoints);
            InkRenderer.CreatePathPoints(stroke);
        }
        [DataTestMethod]
        [DataRow(-12.0, -10.0, 0.0, 0.0, DisplayName = "Down Right")]
        [DataRow(12.0, 10.0, 0.0, 0.0, DisplayName = "Up Left")]
        public void TestCreatePathPoints(double x1, double y1, double x2, double y2)
        {
            var builder = new XInkStrokeBuilder();

            var inkPoints = new[]
            {
                new XInkPoint(new Point(x1, y1), 1f, 0f, 0f, 0),
                new XInkPoint(new Point(x2, y2), 1f, 0f, 0f, 0),
            };

            using var stroke = builder.CreateStrokeFromInkPoints(inkPoints);
            var points = InkRenderer.CreatePathPoints(stroke);

            foreach (var item in points)
            {
                System.Diagnostics.Debug.WriteLine($"{item.X}, {item.Y}");
            }
        }
    }
}
