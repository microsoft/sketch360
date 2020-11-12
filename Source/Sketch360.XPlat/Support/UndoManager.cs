// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.Data;
using Sketch360.XPlat.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Sketch360.XPlat.Support
{
    /// <summary>
    /// Undo manager
    /// </summary>
    public sealed class UndoManager : IUndoManager, IDisposable
    {
        #region Fields
        private readonly Stack<UndoItem> _redoStack = new Stack<UndoItem>();
        //private readonly Stack<UndoItem> _undoStack = new Stack<UndoItem>();
        private readonly List<UndoItem> _undoStack = new List<UndoItem>();
        private bool _isBusy;
        const double UndoRedoDelayInSeconds = 0.25;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the UndoManager class
        /// </summary>
        public UndoManager()
        {
            UndoCommand = new Command(OnUndo, CanUndo);
            RedoCommand = new Command(OnRedo, CanRedo);
        }
        #endregion

        public event EventHandler Updated;

        #region Properties
        /// <summary>
        /// Gets or sets the maxiumum number of undo levels
        /// </summary>
        public int MaximumUndoLevels { get; set; } = 16;
        /// <summary>
        /// Gets the undo command
        /// </summary>
        public ICommand UndoCommand { get; }

        /// <summary>
        /// Gets the redo command
        /// </summary>f_
        public ICommand RedoCommand { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the command manager is busy
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy; set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;

                    UpdateUndoCommands();
                }
            }
        }
        #endregion

        #region Methods
        ////internal void Reset()
        ////{
        ////    _redoStack.Clear();
        ////    _undoStack.Clear();

        ////    UpdateUndoCommands();
        ////}
        public void Add(IUndoItem item)
        {
            PushUndo(item);

            ClearRedoStack();

            UpdateUndoCommands();
        }

        private void PushUndo(IUndoItem item)
        {
            _undoStack.Add(item as UndoItem);

            while (_undoStack.Count > MaximumUndoLevels)
            {
                var first = _undoStack.First();

                if (first is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                _undoStack.Remove(first);
            }
        }

        private void ClearRedoStack()
        {
            foreach (var redoItem in _redoStack.OfType<IDisposable>())
            {
                redoItem.Dispose();
            }

            _redoStack.Clear();
        }

        /// <summary>
        /// Clear the undo and redo stacks
        /// </summary>
        public void Reset()
        {
            foreach (var item in _undoStack.OfType<IDisposable>())
            {
                item.Dispose();
            }

            _undoStack.Clear();

            ClearRedoStack();

            UpdateUndoCommands();
        }

        internal void SaveState(Dictionary<string, object> pageState)
        {
            pageState[nameof(_redoStack)] = _redoStack;
            pageState[nameof(_undoStack)] = _undoStack;
        }

        internal void LoadState(Dictionary<string, object> pageState)
        {
            Reset();

            LoadUndoStack(pageState);

            LoadRedoStack(pageState);
        }

        #endregion

        #region Implementation
        private bool CanUndo()
        {
            return _undoStack.Any() && !IsBusy;
        }

        private async void OnUndo()
        {
            IsBusy = true;

            var undoItem = _undoStack.Last();

            _undoStack.Remove(undoItem);

            undoItem.Undo();

            _redoStack.Push(undoItem);

            Updated?.Invoke(this, new EventArgs());

            await Task.Delay(TimeSpan.FromSeconds(UndoRedoDelayInSeconds)).ConfigureAwait(true);

            IsBusy = false;
        }

        private bool CanRedo()
        {
            return _redoStack.Any() && !_isBusy;
        }

        private async void OnRedo()
        {
            //System.Diagnostics.Debug.WriteLine("Start Redo");

            IsBusy = true;

            if (_redoStack.Any())
            {
                var stroke = _redoStack.Pop();

                stroke.Redo();

                PushUndo(stroke);

                Updated?.Invoke(this, new EventArgs());

                await Task.Delay(TimeSpan.FromSeconds(UndoRedoDelayInSeconds)).ConfigureAwait(true);

                IsBusy = false;
            }

            //System.Diagnostics.Debug.WriteLine("End Redo");

        }

        private void UpdateUndoCommands()
        {
            (RedoCommand as Command).ChangeCanExecute();
            (UndoCommand as Command).ChangeCanExecute();
        }


        private void LoadUndoStack(Dictionary<string, object> pageState)
        {
            if (pageState.TryGetValue(nameof(_undoStack), out object undoStack))
            {
                var stack = undoStack as Stack<UndoItem>;

                var items = stack.Reverse();

                foreach (var item in items)
                {
                    PushUndo(item);
                }

                UpdateUndoCommands();
            }
        }

        private void LoadRedoStack(Dictionary<string, object> pageState)
        {
            if (pageState.TryGetValue(nameof(_redoStack), out object redoStack))
            {
                var stack = redoStack as Stack<UndoItem>;

                var items = stack.Reverse();

                foreach (var item in items)
                {
                    //await item.DeserializeStrokeAsync();

                    _redoStack.Push(item);
                }

                UpdateUndoCommands();
            }
        }

        /// <summary>
        /// Dispose of the undo and redo items.
        /// </summary>
        public void Dispose()
        {
            foreach (var undoItem in _undoStack)
            {
                if (undoItem is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _undoStack.Clear();

            foreach (var redoItem in _redoStack)
            {
                if (redoItem is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _redoStack.Clear();
        }
        #endregion
    }
}
