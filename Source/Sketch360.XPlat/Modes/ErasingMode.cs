// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.Interfaces;
using Xamarin.Forms.Inking;

namespace Sketch360.XPlat.Modes
{
    public class ErasingMode : PanningMode, IErasingMode
    {
        public override void Activate()
        {
            base.Activate();

            InkCanvasView.InkPresenter.InputProcessingConfiguration.Mode = XInkInputProcessingMode.Erasing;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            InkCanvasView.InkPresenter.InputProcessingConfiguration.Mode = XInkInputProcessingMode.None;
        }
    }
}
