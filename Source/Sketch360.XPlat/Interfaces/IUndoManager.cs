// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Windows.Input;
 
namespace Sketch360.XPlat.Interfaces
{
    /// <summary>
    /// Undo manager interface
    /// </summary>
    public interface IUndoManager
    {
        ICommand UndoCommand { get; }
        ICommand RedoCommand { get; }

        event EventHandler Updated;

        void Add(IUndoItem item);

        /// <summary>
        /// Reset the undo manager, clearing the undo and redo stacks.
        /// </summary>
        void Reset();
    }
}
