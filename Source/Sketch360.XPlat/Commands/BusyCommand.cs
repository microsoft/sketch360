// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AppCenter.Crashes;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sketch360.Core.Commands
{
    /// <summary>
    /// a command that is busy while executing
    /// </summary>
    public abstract class BusyCommand : ICommand
    {
        private bool _isBusy;

        /// <summary>
        /// the can exeute changed event handler
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Can the command execute?
        /// </summary>
        /// <param name="parameter">the parameter</param>
        /// <returns>true if the command is not busy</returns>
        public bool CanExecute(object parameter)
        {
            if (_isBusy) return false;

            return OnCanExecute(parameter);
        }

        internal void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Can the command execute? (override to provide additional checks)
        /// </summary>
        /// <param name="parameter">the paramter</param>
        /// <returns>true</returns>
        protected virtual bool OnCanExecute(object parameter)
        {
            return true;
        }


        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="parameter">the parameter</param>
        public async void Execute(object parameter)
        {
            _isBusy = true;

            CanExecuteChanged?.Invoke(this, new EventArgs());

            try
            {
                await ExecuteAsync(parameter).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing {GetType().Name} command: {e.Message}");

                Crashes.TrackError(e);
            }
            _isBusy = false;

            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Execute override
        /// </summary>
        /// <param name="parameter">the parameter</param>
        /// <returns>an async task</returns>
        protected abstract Task ExecuteAsync(object parameter);
    }
}
