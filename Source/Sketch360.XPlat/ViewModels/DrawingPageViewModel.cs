// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.Core.Support;
using Sketch360.XPlat.Interfaces;
using System.Windows.Input;
using Xamarin.Forms.Inking.Interfaces;

namespace Sketch360.XPlat.ViewModels
{
    /// <summary>
    /// Drawing page view model
    /// </summary>
    public class DrawingPageViewModel : BindableBase, IDrawingPageViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingPageViewModel"/> class
        /// </summary>
        /// <param name="undoManager"></param>
        public DrawingPageViewModel(IUndoManager undoManager)
        {
            UndoManager = undoManager;
        }

        /// <summary>
        /// Gets or sets the <see cref="InkCanvasView"/>
        /// </summary>
        public IInkCanvasView InkCanvasView { get; set; }

        /// <summary>
        /// Gets the <see cref="UndoManager"/>
        /// </summary>
        public IUndoManager UndoManager { get; }

        /// <summary>
        /// Gets or sets the Activate mode command
        /// </summary>
        public ICommand ActivateModeCommand { get; set; }

        public ICommand TouchDrawingCommand { get; set; }
    }
}
