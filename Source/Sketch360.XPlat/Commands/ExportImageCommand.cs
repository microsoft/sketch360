// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

namespace Sketch360.XPlat.Commands
{
    /// <summary>
    /// Export image command
    /// </summary>
    public class ExportImageCommand : BusyCommand, IExportImageCommand
    {
        /// <summary>
        /// Gets or sets the page
        /// </summary>
        public Page Page { get; set; }

        /// <summary>
        /// Export the image in a background task
        /// </summary>
        /// <param name="parameter">the parameter is not used</param>
        /// <returns>an async task</returns>
        protected override async Task ExecuteAsync(object parameter)
        {
            var data = App.GetSketchData();

            using var stream = new MemoryStream();
            InkRenderer.RenderImage(data.Width, data.Height, data.BackgroundColor, data.InkStrokes.ToList(), stream, data.Width);

            stream.Seek(0, SeekOrigin.Begin);

            var array = stream.ToArray();
            //https://github.com/Studyxnet/FilePicker-Plugin-for-Xamarin-and-Windows/
            var photoLibrary = DependencyService.Get<IPhotoLibrary>();
            var now = DateTime.Now;
            var filename = string.Format(
                CultureInfo.InvariantCulture,
                Resources.AppResources.DateFilenameFormat,
                string.IsNullOrWhiteSpace(data.Name) ? "Sketch360" : data.Name,
                now.Year,
                now.Month,
                now.Day,
                now.Hour,
                now.Minute,
                now.Second,
                "jpg");

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

            var saved = await photoLibrary.SavePhotoAsync(array, string.Empty, filename, Page).ConfigureAwait(false);

            var introSketch = "No";

            var colors = 0;

            if (data.InkStrokes.Any())
            {
                var firstStrokeStart = data.InkStrokes.First().StrokeStartTime;

                if (firstStrokeStart.HasValue && firstStrokeStart.Value.Date == new DateTime(2020, 7, 29))
                {
                    // Intro Sketch has 254 strokes and was started on 7/29/2020
                    if (data.InkStrokes.Count() == 254)
                    {
                        introSketch = "Yes";
                    }
                    else
                    {
                        introSketch = "Modified";
                    }
                }

                colors = (from inkStroke in data.InkStrokes
                          group inkStroke by inkStroke.DrawingAttributes.Color
                    into c
                          select c.Key).Count();
            }

            var properties = new Dictionary<string, string>
            {
                ["saved"] = saved.ToString(CultureInfo.InvariantCulture),
                ["width"] = data.Width.ToString(CultureInfo.InvariantCulture),
                ["inkstrokes"] = data.InkStrokes.Count().ToString(CultureInfo.InvariantCulture),
                ["background"] = data.BackgroundColor.ToHex(),
                ["durationMin"] = data.Duration.TotalMinutes.ToString(CultureInfo.InvariantCulture),
                ["introSketch"] = introSketch,
                ["startDate"] = data.Start.ToString("u", CultureInfo.InvariantCulture),
                ["colors"] = colors.ToString("g", CultureInfo.InvariantCulture)
            };

            Analytics.TrackEvent("Image Exported", properties);
        }
    }
}
