// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;

[assembly: Xamarin.Forms.Dependency(typeof(Sketch360.XPlat.Droid.BaseUrl))]

namespace Sketch360.XPlat.Droid
{
    public class BaseUrl : IBaseUrl
    {
        public string GetBase()
        {
            return "file:///android_asset/";
        }

        public Stream GetDrawableImageStream(string filename)
        {
            string resourceID = "Sketch360.XPlat.Droid.Resources.drawable." + filename;

            var assembly = typeof(BaseUrl).GetTypeInfo().Assembly;

            //var names = assembly.GetManifestResourceNames();

            return assembly.GetManifestResourceStream(resourceID);

        }
    }
}