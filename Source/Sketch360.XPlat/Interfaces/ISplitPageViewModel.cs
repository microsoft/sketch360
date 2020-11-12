// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.ViewModels;
using System.Windows.Input;
using Xamarin.Forms.Inking.Interfaces;

namespace Sketch360.XPlat.Interfaces
{
    public interface ISplitPageViewModel
    {
        DisplayMode Mode { get; set; }
        bool IsLandscape { get; set; }
        bool IsSpanned { get; set; }
        ICommand ActivateModeCommand { get; set; }

        IUndoManager UndoManager { get; }

        IInkCanvasView InkCanvasView { get; set; }
        ICommand TouchDrawingCommand { get; set; }
    }
}
