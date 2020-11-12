// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ICSharpCode.SharpZipLib.Zip;
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
            var customFileType =
                   new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                   {
                    { DevicePlatform.Android, new[] { "application/octet-stream" } },
                    { DevicePlatform.UWP, new[] { "application/octet-stream"} },
                   });

            var options = new PickOptions
            {
                PickerTitle = "Please select a Sketch 360 File",
                FileTypes = customFileType,
            };

            var fileData = await Xamarin.Essentials.FilePicker.PickAsync(options).ConfigureAwait(true);

            if (fileData == null) return;
            using var stream = await fileData.OpenReadStreamAsync().ConfigureAwait(true);
            using var zipFile = new ZipFile(stream);
            var entry = zipFile.GetEntry("sketch360.json");

            using var jsonStream = zipFile.GetInputStream(entry);
            using var reader = new StreamReader(jsonStream);
            var json = await reader.ReadToEndAsync().ConfigureAwait(true);

            (App.Current as App).LoadSketch(json);
        }
    }
}
