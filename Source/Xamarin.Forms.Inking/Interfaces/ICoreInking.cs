// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Xamarin.Forms.Inking.Interfaces
{
    /// <summary>
    /// Core Inking interface
    /// </summary>
    public interface ICoreInking
    {
        /// <summary>
        /// Create a core wet stroke update source
        /// </summary>
        /// <param name="inkPresenter">the ink presenter</param>
        /// <returns>a new core wet stroke update source</returns>
        XCoreWetStrokeUpdateSource Create(IInkPresenter inkPresenter);
    }
}
