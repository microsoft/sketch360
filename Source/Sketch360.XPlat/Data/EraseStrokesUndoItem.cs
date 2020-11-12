// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Sketch360.XPlat.Data
{
    /// <summary>
    /// Erase stroke undo item
    /// </summary>
    public class EraseStrokesUndoItem : UndoItem, IEraseStrokesUndoItem
    {
        private readonly List<EraseOperation> _operations = new List<EraseOperation>();

        /// <summary>
        /// Gets the operations
        /// </summary>
        public IList<EraseOperation> Operations => _operations;

        /// <summary>
        /// Gets or sets the sketch data
        /// </summary>
        public ISketchData Container { get; set; }

        /// <summary>
        /// Redo the operation (erase strokes again)
        /// </summary>
        public override void Redo()
        {
            _operations.ForEach(RedoOperation);
        }

        /// <summary>
        /// Undo the operation: un-erase the strokes
        /// </summary>
        public override void Undo()
        {
            _operations.ForEach(UndoOperation);
        }

        private void RedoOperation(EraseOperation operation)
        {
            var stroke = (from item in Container.InkStrokes
                          where item.Id == operation.StrokeId
                          select item).FirstOrDefault();

            if (stroke == null) return;

            if (operation.Index == 0)
            {
                // trim off the beginning
               // stroke.Points.RemoveAt(0);
            }
            else
            {
              //  var newPoints = stroke.Points.Take(operation.Index);

               // stroke.Points = new List<Point>(newPoints);

                if (operation.NewStroke != null)
                {
                    Container.Add(new[] { operation.NewStroke });
                }
            }
        }

        private void UndoOperation(EraseOperation operation)
        {
            var stroke = (from item in Container.InkStrokes
                          where item.Id == operation.StrokeId
                          select item).FirstOrDefault();

            if (stroke == null) return;

            //stroke.Points.Add(operation.Point);
            if (operation.NewStroke != null)
            {
                //stroke.Points.AddRange(operation.NewStroke.Points);
                Container.Remove(operation.NewStroke);
            }
        }
    }
}
