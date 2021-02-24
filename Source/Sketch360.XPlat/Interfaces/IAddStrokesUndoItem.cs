// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Xamarin.Forms.Inking;

namespace Sketch360.XPlat.Interfaces
{
    public interface IAddStrokesUndoItem : IUndoItem
    {
        /// <summary>
        /// Gets or sets the container
        /// </summary>
        ISketchData Container { get; set; }


        /// <summary>
        /// Gets or sets the strokes
        /// </summary>
        IList<XInkStroke> Strokes { get; set; }

        bool IsErase { get; set; }
    }
}
