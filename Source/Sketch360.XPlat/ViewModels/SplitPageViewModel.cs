// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.Core.Support;
using Sketch360.XPlat.Interfaces;
using System;
using System.Windows.Input;
using Xamarin.Forms.Inking.Interfaces;

namespace Sketch360.XPlat.ViewModels
{
    public enum DisplayMode
    {
        Split,
        Drawing,
        View360
    }

    public class SplitPageViewModel : BindableBase, ISplitPageViewModel
    {
        DisplayMode mode;
        bool isLandscape;
        bool isSpanned;

        public SplitPageViewModel(IUndoManager undoManager)
        {
            mode = DisplayMode.Split;
            UndoManager = undoManager ?? throw new ArgumentNullException(nameof(undoManager));
        }

        public ICommand ActivateModeCommand { get; set; }

        public ICommand TouchDrawingCommand { get; set; }

        public IUndoManager UndoManager { get; }

        public IInkCanvasView InkCanvasView { get; set; }

        public DisplayMode Mode
        {
            get => mode;
            set
            {
                mode = value;
                OnPropertyChanged();
            }
        }

        public bool IsLandscape
        {
            get => isLandscape;
            set
            {
                isLandscape = value;
                OnPropertyChanged();
            }
        }

        public bool IsSpanned
        {
            get => isSpanned;
            set
            {
                isSpanned = value;
                OnPropertyChanged();
            }
        }
    }
}
