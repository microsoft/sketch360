// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Inking.Interfaces
{
    /// <summary>
    /// Ink stroke Container interface
    /// </summary>
    public interface IInkStrokeContainer : IDisposable
    {
        /// <summary>
        /// Ink changed event
        /// </summary>
        event EventHandler InkChanged;

        /// <summary>
        /// Gets the strokes
        /// </summary>
        /// <returns>a read only list of <see cref="XInkStroke"/> objects</returns>
        IReadOnlyList<XInkStroke> GetStrokes();

        /// <summary>
        /// Clear the stroke list
        /// </summary>
        void Clear();

        /// <summary>
        /// Add an ink stroke
        /// </summary>
        /// <param name="inkStroke">the ink stroke</param>
        void Add(XInkStroke inkStroke);

        /// <summary>
        /// Add a list of ink strokes
        /// </summary>
        /// <param name="strokes">the list of ink strokes</param>
        void Add(IEnumerable<XInkStroke> strokes);

        /// <summary>
        /// Remove an ink stroke
        /// </summary>
        /// <param name="item">the ink stroke to remove</param>
        void Remove(XInkStroke item);

        /// <summary>
        /// Set the ink strokes
        /// </summary>
        /// <param name="inkStrokes">a collection of ink strokes</param>
        void SetRange(IEnumerable<XInkStroke> inkStrokes);

        /// <summary>
        /// Remove the ink strokes
        /// </summary>
        /// <param name="strokesToRemove">the strokes to remove</param>
        void Remove(IReadOnlyList<XInkStroke> strokesToRemove);


        //void Deserialize(string json);

        //string Serialize();
    }
}
