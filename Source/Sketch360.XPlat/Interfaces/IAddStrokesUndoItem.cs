// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Xamarin.Forms.Inking;

namespace Sketch360.XPlat.Interfaces
{
    public interface IAddStrokesUndoItem : IUndoItem
    {
        ISketchData Container { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
        IList<XInkStroke> Strokes { get; set; }

        bool IsErase { get; set; }
    }
}
