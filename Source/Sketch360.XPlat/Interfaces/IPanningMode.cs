// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Sketch360.XPlat.Interfaces
{
    public interface IPanningMode : IDrawingViewMode
    {
        double MinZoomFactor { get; set; }

        double MaxZoomFactor { get; set; }
    }
}
