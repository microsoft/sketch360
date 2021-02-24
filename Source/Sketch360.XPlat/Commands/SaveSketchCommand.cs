// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AppCenter.Analytics;
using Sketch360.Core.Commands;
using Sketch360.XPlat.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Inking.Support;

namespace Sketch360.XPlat
{
    /// <summary>
    /// Save sketch command
    /// </summary>
    public class SaveSketchCommand : BusyCommand, IExportImageCommand
    {
        public Page Page { get; set; }

        /// <summary>
        /// Save the sketch file
        /// </summary>
        /// <param name="parameter">the parameter is not used</param>
        /// <returns>an async task</returns>
        protected override async Task ExecuteAsync(object parameter)
        {
            var photoLibrary = DependencyService.Get<IPhotoLibrary>();

            var json = (App.Current as App).SerializeSketchData();

            using var stream = new MemoryStream();

            var sketchData = App.GetSketchData();

            using (var zipStream = new ZipOutputStream(stream))
            {
                await AddJsonFileAsync(json, zipStream).ConfigureAwait(false);

                AddJpegImage(sketchData, zipStream);
            }

            var now = DateTime.Now;

            var filename = string.Format(
                CultureInfo.InvariantCulture,
                "{0}.sketch360",
                string.IsNullOrWhiteSpace(sketchData.Name) ? "Sketch" : sketchData.Name);

            var chars = Path.GetInvalidFileNameChars();

            int index;

            var filenameChars = filename.ToCharArray();

            do
            {
                index = filename.IndexOfAny(chars);

                if (index >= 0)
                {
                    filenameChars[index] = '_';

                    filename = new string(filenameChars);
                }
            }
            while (index != -1);

            bool result = await photoLibrary.SaveSketchAsync(stream.ToArray(), "Sketch360", filename, Page).ConfigureAwait(true);

            if (result && App.Current.MainPage is Page page)
            {
                await page.DisplayAlert(
                    Resources.AppResources.SketchSaved,
                    string.Format(CultureInfo.CurrentCulture,
                    Resources.AppResources.SketchSavedMessageFormat,
                    filename),
                    Resources.AppResources.OK).ConfigureAwait(false);
            }

            var properties = new Dictionary<string, string>
            {
                ["saved"] = result.ToString(CultureInfo.InvariantCulture),
                ["inkstrokes"] = sketchData.InkStrokes.Count().ToString(CultureInfo.InvariantCulture),
                ["background"] = sketchData.BackgroundColor.ToHex(),
                ["duration"] = sketchData.Duration.TotalMinutes.ToString(CultureInfo.InvariantCulture)
            };

            Analytics.TrackEvent("Sketch Saved", properties);
        }

        private static async Task AddJsonFileAsync(string json, ZipOutputStream zipStream)
        {
            var jsonEntry = new ZipEntry("sketch360.json")
            {
                DateTime = DateTime.UtcNow
            };

            zipStream.PutNextEntry(jsonEntry);

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            await writer.WriteAsync(json).ConfigureAwait(false);

            await writer.FlushAsync().ConfigureAwait(false);

            memoryStream.Seek(0, SeekOrigin.Begin);

            var buffer = new byte[4096];

            StreamUtils.Copy(memoryStream, zipStream, buffer);
        }

        private static void AddJpegImage(ISketchData sketchData, ZipOutputStream zipStream)
        {
            var jpegEntry = new ZipEntry("sketch.jpeg")
            {
                DateTime = DateTime.UtcNow
            };

            zipStream.PutNextEntry(jpegEntry);

            using var memoryStream = new MemoryStream();
            InkRenderer.RenderImage(sketchData.Width, sketchData.Height, sketchData.BackgroundColor,
                sketchData.InkStrokes.ToList(), memoryStream, sketchData.Width);

            memoryStream.Seek(0, SeekOrigin.Begin);

            var buffer = new byte[4096];

            StreamUtils.Copy(memoryStream, zipStream, buffer);
        }
    }
}
