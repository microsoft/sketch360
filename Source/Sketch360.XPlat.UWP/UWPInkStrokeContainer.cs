// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI.Input.Inking;
using Xamarin.Forms.Inking;

[assembly: Xamarin.Forms.Dependency(typeof(Sketch360.XPlat.UWP.UWPInkStrokeContainer))]


namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// UWP Ink Stroke Container
    /// </summary>
    public sealed class UWPInkStrokeContainer : Xamarin.Forms.Inking.Interfaces.IInkStrokeContainer
    {
        #region Events
        /// <summary>
        /// ink changed event
        /// </summary>
        public event EventHandler InkChanged;
        #endregion

        #region Properties
        /// <summary>
        /// the native UWP stroke container
        /// </summary>
        public IInkStrokeContainer NativeStrokeContainer { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Add a stroke
        /// </summary>
        /// <param name="inkStroke">the ink stroke to add</param>
        public void Add(XInkStroke inkStroke)
        {
            var newStroke = inkStroke.ToInkStroke();

            if (newStroke == null) return;

            NativeStrokeContainer.AddStroke(newStroke);

            InkChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Add a collection of ink strokes
        /// </summary>
        /// <param name="strokes">the ink strokes to add</param>
        public void Add(IEnumerable<XInkStroke> strokes)
        {
            AddStrokes(strokes);

            InkChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Clear the strokes
        /// </summary>
        public void Clear()
        {
            DeleteStrokes();

            InkChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Clear the strokes
        /// </summary>
        public void Dispose()
        {
            Clear();
            InkChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Get the strokes
        /// </summary>
        /// <returns>a read-only list of the strokes</returns>
        public IReadOnlyList<XInkStroke> GetStrokes()
        {
            var strokes = NativeStrokeContainer.GetStrokes();

            return strokes.ToXInkStrokeList();
        }

        /// <summary>
        /// Remove an ink stroke
        /// </summary>
        /// <param name="item">the stroke to remove</param>
        public void Remove(XInkStroke item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            foreach (var stroke in NativeStrokeContainer.GetStrokes())
            {
                if (stroke.Id.ToString(CultureInfo.InvariantCulture) == item.Id)
                {
                    stroke.Selected = true;
                }
            }

            NativeStrokeContainer.DeleteSelected();

            InkChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Remove the strokes
        /// </summary>
        /// <param name="strokesToRemove">the strokes to remove</param>
        public void Remove(IReadOnlyList<XInkStroke> strokesToRemove)
        {
            var ids = from item in strokesToRemove
                      select item.Id;

            foreach (var stroke in NativeStrokeContainer.GetStrokes())
            {
                if (ids.Contains(stroke.Id.ToString(CultureInfo.InvariantCulture)))
                {
                    stroke.Selected = true;
                }
            }

            NativeStrokeContainer.DeleteSelected();

            InkChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Set the strokes
        /// </summary>
        /// <param name="inkStrokes">the new ink strokes</param>
        public void SetRange(IEnumerable<XInkStroke> inkStrokes)
        {
            DeleteStrokes();

            AddStrokes(inkStrokes);

            InkChanged?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Implementation

        /// <summary>
        /// Add a collection of strokes.
        /// </summary>
        /// <param name="strokes">the strokes to add</param>
        private void AddStrokes(IEnumerable<XInkStroke> strokes)
        {
            if (strokes == null) throw new ArgumentNullException(nameof(strokes));

            if (NativeStrokeContainer == null) return;

            var strokesToAdd = from item in strokes
                               let newStroke = item.ToInkStroke()
                               where newStroke != null
                               select newStroke;

            foreach (var item in strokesToAdd)
            {
                NativeStrokeContainer.AddStroke(item);
            }

            InkChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Delete the strokes
        /// </summary>
        private void DeleteStrokes()
        {
            if (NativeStrokeContainer == null) return;

            foreach (var item in NativeStrokeContainer.GetStrokes())
            {
                item.Selected = true;
            }

            NativeStrokeContainer.DeleteSelected();
        }
        #endregion
    }
}
