// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if WINDOWS_UWP
using Windows.Foundation;
#else
using Xamarin.Forms;
#endif

namespace Sketch360.Core.Support
{
    /// <summary>
    /// Equirectangular point calculator
    /// </summary>
    /// <remarks>
    /// Equirectangular tools (Eq A Snap) by António B. Araújo", <![CDATA[http://www.univ-ab.pt/~aaraujo/eqasnap.html]]>
    /// </remarks>
    public static class Equirectangular
    {
        /// <summary>
        /// default increment is 1 degree
        /// </summary>
        const double DefaultIncrement = Math.PI / 180.0;

        #region Methods
        /// <summary>
        /// Gets the apex of geodesic given two vertices
        /// </summary>
        /// <param name="vertex1">the first vertex</param>
        /// <param name="vertex2">the second vertex</param>
        /// <returns>the apex of the geodesic</returns>
        /// <exception cref="ArgumentException">if vertex1 or vertex2 is invalid</exception>
        /// <exception cref="ArgumentException">if vertex1 is equal to vertex2</exception>
        /// <example>
        /// Code to get the apex of a geodesic given two points on a 600x300 grid.
        /// <code>
        /// var p1 = new Point(100,100);
        /// var p2 = new Point(134, 164);
        /// var height = 300.0;
        /// var v1 = Vertex.CreateVertex(p1, height);
        /// var v2 = Vertex.CreateVertex(p2, height);
        /// var apex = Equirectangular.ApexOf(v1, v2);
        /// </code>
        /// </example>
        public static Vertex ApexOf(Vertex vertex1, Vertex vertex2)
        {
            #region Input Validation
            if (!vertex1.IsValid) throw new ArgumentException("vertex1 is invalid", nameof(vertex1));

            if (!vertex2.IsValid) throw new ArgumentException("vertex2 is invalid", nameof(vertex2));

            if (vertex1 == vertex2) throw new ArgumentException("vertex1 and vertex2 cannot be equal.", nameof(vertex2));
            #endregion

            var l1 = vertex1.Azimuth;
            var l2 = vertex2.Azimuth;
            var p1 = vertex1.Elevation;
            var p2 = vertex2.Elevation;

            // calculates the plane of the geodesic:

            var a = -Math.Cos(p2) * Math.Sin(l2) * Math.Sin(p1) + Math.Cos(p1) * Math.Sin(l1) * Math.Sin(p2);
            var b = -Math.Cos(p1) * Math.Cos(l1) * Math.Sin(p2) + Math.Cos(p2) * Math.Cos(l2) * Math.Sin(p1);
            var c = Math.Cos(p1) * Math.Cos(p2) * Math.Sin(l2 - l1);

            // obtains the apex on the plane

            var lm = Math.Atan2(-Math.Sign(c) * b, -Math.Sign(c) * a);
            var pm = Math.Atan(Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2)) / Math.Abs(c));

            return new Vertex { Azimuth = lm, Elevation = pm };
        }

        /// <summary>
        /// Gets the elevation of a vertex in radians on the curve defined by apex at a specific azimuth
        /// </summary>
        /// <param name="apex">the apex of the geodesic curve</param>
        /// <param name="azimuth">the azimuth (x) in radians</param>
        /// <returns>the elevation (y) in radians from -Pi/2 looking down to Pi/2 looking up</returns>
        /// <exception cref="ArgumentException">if the axex is invalid</exception>
        /// <exception cref="ArgumentOutOfRangeException">if the azimuth is less than -Pi or greater than Pi</exception>
        /// <example>
        /// Code to get the elevation (y) of a point on a geodesic given its azimuth (x).
        /// <code>
        /// var p1 = new Point(100,100);
        /// var p2 = new Point(134, 164);
        /// var height = 300.0;
        /// var v1 = Vertex.CreateVertex(p1, height);
        /// var v2 = Vertex.CreateVertex(p2, height);
        /// var apex = Equirectangular.ApexOf(v1, v2);
        /// // get the elevation of a point that is 90 degrees to the right
        /// var elevation = Equirectangular.GetElevation(apex, Math.Pi /2);
        /// </code>
        /// </example>
        public static double GetElevation(Vertex apex, double azimuth)
        {
            #region Validation
            if (!apex.IsValid) throw new ArgumentException("invlid apex", nameof(apex));

            if (azimuth < -Math.PI || azimuth > Math.PI) throw new ArgumentOutOfRangeException(nameof(azimuth));
            #endregion

            var phi = apex.Elevation;

            var lambda = apex.Azimuth;

            var elevation = Math.Atan(Math.Tan(phi) * Math.Cos(azimuth - lambda));

            return elevation;
        }

        /// <summary>
        /// Gets the points for a geodesic curve with apexPoint at its apex
        /// </summary>
        /// <param name="apexPoint">the apex point of the curve</param>
        /// <param name="pixelHeight">the pixel height of the grid</param>
        /// <param name="increment">the x increment in radians (default is one degree)</param>
        /// <returns>a list of points on the grid</returns>
        /// <exception cref="ArgumentException">if the apexPoint is invalid</exception>
        /// <exception cref="ArgumentOutOfRangeException">if the pixelHeight is less than or equal to 0.</exception>
        /// <example>
        /// Code to get the the points of a polyline defining a geodesic.
        /// <code>
        /// var p1 = new Point(100,100);
        /// var p2 = new Point(134, 164);
        /// var height = 300.0;
        /// var v1 = Vertex.CreateVertex(p1, height);
        /// var v2 = Vertex.CreateVertex(p2, height);
        /// var apex = Equirectangular.ApexOf(v1, v2);
        /// var points = Equirectangular.GetPoints(apex, height);
        /// // plot the points on a grid 600x300
        /// </code>
        /// </example>        
        public static IReadOnlyList<Point> GetPoints(Vertex apexPoint, double pixelHeight, double increment = DefaultIncrement)
        {
            #region Validation
            if (!apexPoint.IsValid) throw new ArgumentException("invalid", nameof(apexPoint));

            if (pixelHeight <= 0) throw new ArgumentOutOfRangeException(nameof(pixelHeight));
            #endregion

            var minimum = -Math.PI;
            var maximum = Math.PI;
            var points = new List<Point>();

            for (var i = minimum; i <= maximum; i += increment)
            {
                var elevation = Equirectangular.GetElevation(apexPoint, i);

                var vertex = new Vertex { Azimuth = i, Elevation = elevation };

                Debug.Assert(vertex.IsValid);

                points.Add(vertex.GetPoint(pixelHeight));
            }

            return points;
        }
        #endregion
    }

    /// <summary>
    /// A vertex on a sphere in equirectangular coordinates
    /// </summary>
    /// <remarks>From equations supplied by António Bandeira Araújo.</remarks>
    [DebuggerDisplay("Azimuth= {Azimuth} ({System.Math.Round(Azimuth * 180 / System.Math.PI)}°) , Elevation={Elevation} ({System.Math.Round(Elevation * 180 / System.Math.PI)}°)")]
    public struct Vertex
    {
        #region Properties
        /// <summary>
        /// The elevation from -Pi/2 looking down to Pi/2 looking up in radians (Y in equirectangular)
        /// </summary>
        public double Elevation;

        /// <summary>
        /// The azimuth from -Pi (looking back to the left) to Pi (looking back to the right) in radians (X in equirectangular)
        /// </summary>
        public double Azimuth;

        /// <summary>
        /// returns a value indicating that the Azimuth and Elevation values are in the valid range
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (double.IsNaN(Elevation))
                {
                    System.Diagnostics.Debug.WriteLine($"Elevation is not a number");
                    return false;
                }
                if (Elevation < -Math.PI / 2)
                {
                    System.Diagnostics.Debug.WriteLine($"Elevation is less than -PI/2: {Elevation}");
                    return false;
                }

                if (Elevation > Math.PI / 2)
                {
                    System.Diagnostics.Debug.WriteLine($"Elevation is greater than PI/2: {Elevation}");
                    return false;
                }

                if (double.IsNaN(Azimuth))
                {
                    System.Diagnostics.Debug.WriteLine($"Azimuth is not a number");

                    return false;
                }

                if (Azimuth < -Math.PI)
                {
                    System.Diagnostics.Debug.WriteLine($"Azimuth is less than -PI: {Azimuth}");
                    return false;
                }

                if (Azimuth > Math.PI)
                {
                    System.Diagnostics.Debug.WriteLine($"Azimuth is greater than PI: {Azimuth}");
                    return false;
                }

                return true;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Create a vertex given a point and grid pixel height
        /// </summary>
        /// <param name="point">the point in the pixel space</param>
        /// <param name="pixelHeight">the pixel space height ( the width is 2x the height)</param>
        /// <returns>a vertex</returns>
        public static Vertex CreateVertex(Point point, double pixelHeight)
        {
            //Clip the point to x:0-pixelHeight * 2, y: 0-pixelHeight
            point.X = Math.Min(Math.Max(0, point.X), pixelHeight * 2);
            point.Y = Math.Min(Math.Max(0, point.Y), pixelHeight);

            var vertex = new Vertex
            {
                Elevation = (Math.PI / 2) - (point.Y / pixelHeight) * Math.PI,
                Azimuth = (Math.PI * 2) * (point.X / (pixelHeight * 2)) - Math.PI
            };

            return vertex;
        }

        /// <summary>
        /// Gets a pixel coordinate of a spherical vertex given a pixel height of a equirectangular grid
        /// </summary>
        /// <param name="pixelHeight"></param>
        /// <returns>the point between 0 and pixelHeight, inclusive</returns>
        /// <exception cref="ArgumentOutOfRangeException">if pixelHeight is less than or equal to 0.</exception>
        public Point GetPoint(double pixelHeight)
        {
            if (pixelHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pixelHeight), $"pixelHeight must be greater than 0: {pixelHeight}");
            }

            var x = pixelHeight * Azimuth / Math.PI + pixelHeight;

            var y = (pixelHeight) * (-Elevation + (Math.PI / 2)) / Math.PI;

            Debug.Assert(x >= 0 && x <= pixelHeight * 2);
            Debug.Assert(y >= 0 && y <= pixelHeight);

            return new Point(x, y);
        }

        /// <summary>
        /// determines if one Vertex equals another
        /// </summary>
        /// <param name="obj">another vertex</param>
        /// <returns>true if the Azimuth and Elevation values are the same</returns>
        public override bool Equals(object obj)
        {
            if (obj is Vertex vertex)
            {
                return this.Elevation == vertex.Elevation && this.Azimuth == vertex.Azimuth;
            }

            return false;
        }

        /// <summary>
        /// Compare two vertices
        /// </summary>
        /// <param name="left">the left vertex</param>
        /// <param name="right">the right vertex</param>
        /// <returns>true if they are the same value</returns>
        public static bool operator ==(Vertex left, Vertex right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compare two vertices for ineqality
        /// </summary>
        /// <param name="left">the left vertex</param>
        /// <param name="right">the right vertex</param>
        /// <returns>true if the vertices are not equal</returns>
        public static bool operator !=(Vertex left, Vertex right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Gets a hash code for the vertex
        /// </summary>
        /// <returns>an integer hash code</returns>
        public override int GetHashCode()
        {
            return Elevation.GetHashCode() * 3 + Azimuth.GetHashCode() * 5;
        }
        #endregion
    }
}
