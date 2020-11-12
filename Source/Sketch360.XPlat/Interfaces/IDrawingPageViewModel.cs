// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows.Input;
using Xamarin.Forms.Inking.Interfaces;

namespace Sketch360.XPlat.Interfaces
{
    public interface IDrawingPageViewModel
    {
        IInkCanvasView InkCanvasView { get; set; }

        IUndoManager UndoManager { get; }

        ICommand ActivateModeCommand { get; set; }

        ICommand TouchDrawingCommand { get; set; }
    }
}
