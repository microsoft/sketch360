// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.Interfaces;

namespace Sketch360.XPlat.Data
{
    /// <summary>
    /// Undo item base class
    /// </summary>
    public abstract class UndoItem : IUndoItem
    {
        /// <summary>
        /// Undo an operation
        /// </summary>
        public abstract void Undo();

        /// <summary>
        /// Redo an operation
        /// </summary>
        public abstract void Redo();
    }
}
