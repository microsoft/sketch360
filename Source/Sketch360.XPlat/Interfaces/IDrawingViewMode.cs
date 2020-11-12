// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xamarin.Forms.Inking.Interfaces;

namespace Sketch360.XPlat.Interfaces
{
    public interface IDrawingViewMode
    {
        IInkCanvasView InkCanvasView { get; set; }

        void Deactivate();

        void Activate();
    }
}
