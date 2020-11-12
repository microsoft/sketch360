// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xamarin.Forms;
using Xamarin.Forms.Inking;

namespace Sketch360.XPlat.Data
{
    public class EraseOperation
    {
        public string StrokeId { get; set; }

        public int Index { get; set; }

        public Point Point { get; set; }

        public XInkStroke NewStroke { get; set; }
    }
}
