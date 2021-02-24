// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AppCenter.Analytics;
using Sketch360.Core.Commands;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Sketch360.XPlat.Commands
{
    /// <summary>
    /// Open sketch command
    /// </summary>
    public class OpenSketchCommand : BusyCommand
    {
        /// <summary>
        /// Open a .sketch360 file
        /// </summary>
        /// <param name="parameter">the parameter is not used.</param>
        /// <returns>an async task</returns>
        protected override async Task ExecuteAsync(object parameter)
        {
            /// Once we stop getting opens with octet-stream, we will remove the file type and only open x-sketch360
            var customFileType =
                   new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                   {
                    //{ DevicePlatform.Android, new[] { "application/x-sketch360" } },
                    { DevicePlatform.Android, new[] { "application/octet-stream", "application/x-sketch360" } },
                    { DevicePlatform.UWP, new[] { ".sketch360"} },
                   });

            var options = new PickOptions
            {
                PickerTitle = "Please select a Sketch 360 File",
                FileTypes = customFileType,
            };

            var fileData = await Xamarin.Essentials.FilePicker.PickAsync(options).ConfigureAwait(true);

            if (fileData == null) return;

            if (System.IO.Path.GetExtension(fileData.FileName) == ".sketch360")
            {
                using var stream = await fileData.OpenReadAsync().ConfigureAwait(true);

                using var zipFile = new ZipFile(stream);
                var entry = zipFile.GetEntry("sketch360.json");

                using var jsonStream = zipFile.GetInputStream(entry);
                using var reader = new StreamReader(jsonStream);
                var json = await reader.ReadToEndAsync().ConfigureAwait(true);

                (App.Current as App).LoadSketch(json);
            }

            var properties = new Dictionary<string, string>
            {
                ["filename"] = fileData.FileName,
                ["contentType"] = fileData.ContentType
            };

            Analytics.TrackEvent("Sketch Opened", properties);
        }
    }
}
