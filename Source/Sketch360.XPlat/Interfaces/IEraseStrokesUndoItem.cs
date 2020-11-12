// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.Data;
using System.Collections.Generic;

namespace Sketch360.XPlat.Interfaces
{
    /// <summary>
    /// Erase stroke undo item interface
    /// </summary>
    public interface IEraseStrokesUndoItem : IUndoItem
    {
        /// <summary>
        /// Gets or sets the sketch data
        /// </summary>
        ISketchData Container { get; set; }

        /// <summary>
        /// Gets the erase operations
        /// </summary>
        IList<EraseOperation> Operations { get; }
    }
}
