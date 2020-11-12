// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AppCenter.Analytics;
using Sketch360.Core.Commands;
using Sketch360.XPlat.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Inking.Support;

namespace Sketch360.XPlat.Commands
{
    /// <summary>
    /// Command to share the current jpeg image
    /// </summary>
    public class ShareCommand : BusyCommand
    {
        /// <summary>
        /// Share the current sketch as a JPEG with EXIF data
        /// </summary>
        /// <param name="parameter">the parameter is not used</param>
        /// <returns>an async task</returns>
        protected override async Task ExecuteAsync(object parameter)
        {
            var data = App.GetSketchData();

            var title = string.IsNullOrWhiteSpace(data.Name) ? "Sketch" : data.Name;

            var filename = Path.Combine(FileSystem.CacheDirectory, title + ".jpeg");

            using (var stream = File.Create(filename))
            {
                InkRenderer.RenderImage(data.Width, data.Height, data.BackgroundColor, data.InkStrokes.ToList(), stream, data.Width);
            }

            var photoLibrary = DependencyService.Get<IPhotoLibrary>();

            photoLibrary.AddExifData(filename);

            var request = new ShareFileRequest(title + " #Sketch360", new ShareFile(filename));

            await Share.RequestAsync(request).ConfigureAwait(false);

            var properties = new Dictionary<string, string>
            {
                ["width"] = data.Width.ToString(CultureInfo.InvariantCulture),
                ["inkstrokes"] = data.InkStrokes.Count().ToString(CultureInfo.InvariantCulture),
                ["background"] = data.BackgroundColor.ToHex(),
                ["duration"] = data.Duration.TotalMinutes.ToString(CultureInfo.InvariantCulture)

            };

            Analytics.TrackEvent("Share", properties);
        }
    }
}
