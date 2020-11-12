// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace Sketch360.XPlat
{
    /// <summary>
    /// Base URI interface
    /// </summary>
    public interface IBaseUrl
    {
        /// <summary>
        /// Gets the platform-dependent base URL
        /// </summary>
        /// <returns></returns>
        string GetBase();
        Stream GetDrawableImageStream(string filename);
    }
}
