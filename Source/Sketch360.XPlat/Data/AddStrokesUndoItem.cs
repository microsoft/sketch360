// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Inking;

namespace Sketch360.XPlat.Data
{
    public sealed class AddStrokesUndoItem : UndoItem, IAddStrokesUndoItem, IDisposable
    {
        public bool IsErase { get; set; }

        public ISketchData Container { get; set; }


        /// <summary>
        /// Gets or sets the strokes
        /// </summary>
        /// <remarks>Ignore CA2227 as this needs to have public get and set for serialization</remarks>
        public IList<XInkStroke> Strokes { get; set; }

        public override void Undo()
        {
            if (IsErase)
            {
                AddStrokes();
            }
            else
            {
                RemoveStrokes();
            }

        }

        private void RemoveStrokes()
        {
            var strokeIds = from item in Strokes
                            select item.Id;

            var itemsToRemove = (from item in Container.InkStrokes
                                 where strokeIds.Contains(item.Id)
                                 select item).ToList();

            foreach (var item in itemsToRemove)
            {
                Container.Remove(item);
            }
        }

        public override void Redo()
        {
            if (IsErase)
            {
                RemoveStrokes();
            }
            else
            {
                AddStrokes();
            }
        }

        private void AddStrokes()
        {
            Container.Add(Strokes);
        }

        /// <summary>
        /// Dispose of the strokes.
        /// </summary>
        public void Dispose()
        {
            if (Strokes != null)
            {
                foreach (var item in Strokes)
                {
                    item.Dispose();
                }

                Strokes.Clear();
            }
        }
    }
}
