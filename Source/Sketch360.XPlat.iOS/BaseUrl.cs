// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;
using Foundation;

[assembly: Xamarin.Forms.Dependency(typeof(Sketch360.XPlat.iOS.BaseUrl))]

namespace Sketch360.XPlat.iOS
{
    public class BaseUrl : IBaseUrl
    {
        public string GetBase()
        {
            return $"{NSBundle.MainBundle.BundlePath}/";
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