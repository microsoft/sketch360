// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Windows.UI.Xaml.Controls;

namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// View changed evnet arguments
    /// </summary>
    public sealed class ViewChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the ViewChangedEventArgs class.
        /// </summary>
        /// <param name="scrollViewer">the scroll viewer</param>
        /// <param name="isIntermediate">is this intermediate?</param>
        public ViewChangedEventArgs(ScrollViewer scrollViewer, bool isIntermediate)
        {
            ScrollViewer = scrollViewer;
            IsIntermediate = isIntermediate;
        }

        /// <summary>
        /// Gets the scroll viewer
        /// </summary>
        public ScrollViewer ScrollViewer { get; }

        /// <summary>
        /// Gets a value indicating whether the view change is intermediate
        /// </summary>
        public bool IsIntermediate { get; }
    }
}
