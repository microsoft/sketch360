// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xamarin.Forms;
using Xamarin.Forms.Inking;
using Xamarin.Forms.Inking.Interfaces;

[assembly: Dependency(typeof(Sketch360.XPlat.UWP.CoreInking))]

namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// Core Inking functions
    /// </summary>
    public sealed class CoreInking : ICoreInking
    {
        /// <summary>
        /// Create a new Core wet stroke update source
        /// </summary>
        /// <param name="inkPresenter">the ink presenter</param>
        /// <returns>a new <see cref="UWPCoreWetStrokeUpdateSource"/> object</returns>
        public XCoreWetStrokeUpdateSource Create(IInkPresenter inkPresenter)
        {
            return new UWPCoreWetStrokeUpdateSource(inkPresenter);
        }
    }
}
