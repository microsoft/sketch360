// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xamarin.Forms;

namespace Sketch360.XPlat
{
    /// <summary>
    /// Zoomable ScrollView
    /// </summary>
    public class ZoomableScrollView : ScrollView, IZoomableScrollView
    {
        private bool _isZoomEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether the scrollview is zoomable
        /// </summary>
        public bool IsZoomEnabled
        {
            get => _isZoomEnabled;

            set
            {
                if (_isZoomEnabled != value)
                {
                    _isZoomEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}