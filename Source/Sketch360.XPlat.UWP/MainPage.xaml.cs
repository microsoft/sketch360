// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// Xamarin Forms MainPage class
    /// </summary>
    public sealed partial class MainPage
    {
        /// <summary>
        /// Initializes a new instance of the MainPage class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            var app = new Sketch360.XPlat.App();

            LoadApplication(app);
        }
    }
}
