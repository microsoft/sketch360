// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Android;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Provider;
using Android.Support.V4.App;
using Sketch360.XPlat.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Linq;
using Microsoft.AppCenter.Crashes;

[assembly: Dependency(typeof(Sketch360.XPlat.Droid.PhotoLibrary))]

namespace Sketch360.XPlat.Droid
{
    /// <summary>
    /// Android Photo library
    /// </summary>
    public class PhotoLibrary : IPhotoLibrary
    {
        #region Constants
        /// <summary>
        /// XMP Metadata with equirectangular Projection property
        /// </summary>
        private const string Xmp = "<x:xmpmeta xmlns:x=\"adobe:ns:meta/\"><rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"><rdf:Description xmlns:prefix0=\"http://ns.google.com/photos/1.0/panorama/\"><prefix0:ProjectionType>equirectangular</prefix0:ProjectionType></rdf:Description></rdf:RDF></x:xmpmeta>";
        #endregion

        #region Methods
        /// <summary>
        /// Save a photo to the photo library with equirectangular metadata.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="folder"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public async Task<bool> SavePhotoAsync(byte[] data, string folder, string filename, Page page)
        {
            try
            {
                if (!(await RequestPermissionsAsync(page).ConfigureAwait(false)))
                {
                    return false;
                }

                string tempFilename = await SaveToTempFileAsync(data, filename).ConfigureAwait(false);

                AddExifData(tempFilename);

                var uri = await WriteContentValuesAsync(tempFilename, filename, "image/jpeg").ConfigureAwait(false);

                System.IO.File.Delete(tempFilename);

                var newFilename = GetPath(uri);

                await LaunchFileAsync(newFilename).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);

                return false;
            }

            return true;
        }

        private static async Task<bool> RequestPermissionsAsync(Page page)
        {
            Task<PermissionStatus>[] permissions = new[]
            {
                    new Permissions.StorageWrite().RequestAsync(),
                    new Permissions.Photos().RequestAsync(),
            };

            var results = await Task.WhenAll(permissions).ConfigureAwait(false);

            if(results.Any(x => x != PermissionStatus.Granted))
            {
                if (page != null)
                {
                    await page.DisplayAlert(
                        Resources.AndroidResources.PermissionsTitle,
                        Resources.AndroidResources.PermssionsMessage,
                        Resources.AndroidResources.OK).ConfigureAwait(false);
                }

                return false;
            }

            return true;
        }

        static string GetPath(Android.Net.Uri uri)
        {
            string path = null;

#pragma warning disable CS0618 // Type or member is obsolete
            String[] projection = { MediaStore.MediaColumns.Data };
#pragma warning restore CS0618 // Type or member is obsolete

            var metaCursor = MainActivity.Instance.ContentResolver.Query(uri, projection, null, null, null);
            if (metaCursor != null)
            {
                using (metaCursor)
                {
                    if (metaCursor.MoveToFirst())
                    {
                        path = metaCursor.GetString(0);
                    }
                }
            }

            return path;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public async Task<bool> SaveSketchAsync(byte[] data, string folder, string filename, Page page)
        {
            bool result;
            try
            {
                if (!(await RequestPermissionsAsync(page).ConfigureAwait(false)))
                {
                    return false;
                }

                var tempFilename = await SaveToTempFileAsync(data, filename).ConfigureAwait(false);

                var urilocation = await WriteDownloadValuesAsync(tempFilename, filename, "application/x-sketch360").ConfigureAwait(false);

                result = urilocation != null;

                //var documentsDirectory = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);

                //var fullPath = await WriteToFolderAsync(data, filename, documentsDirectory).ConfigureAwait(false);


                //MediaScannerConnection.ScanFile(MainActivity.Instance,
                //                                   new string[] { fullPath },
                //                                   new string[] { "application/x-sketch360" }, null);


                //var fullPath = await WriteToSpecialFolderAsync(tempFilename, filename, Environment.SpecialFolder.MyDocuments).ConfigureAwait(false);

                System.IO.File.Delete(tempFilename);

                //await LaunchFileAsync(fullPath).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);

                return false;
            }

            return result;
        }
        #endregion

        #region Implementation
        //private static async Task<string> WriteToSpecialFolderAsync(string tempFilename, string filename, Java.IO.File folder)
        //{
        //    folder.Mkdirs();

        //    var filePath = Path.Combine(folder.AbsolutePath, filename);

        //    var data = System.IO.File.ReadAllBytes(tempFilename);

        //    using (var stream = new Java.IO.FileOutputStream(filePath))
        //    {
        //        await stream.WriteAsync(data).ConfigureAwait(false);
        //    }

        //    return filePath;
        //    //var folder = System.Environment.GetFolderPath(specialFolder);

        //    //    System.IO.Directory.CreateDirectory(folder);

        //    //    var myPicturesFilename = Path.Combine(folder, filename);

        //    //    using (var stream = System.IO.File.Create(myPicturesFilename))
        //    //    using (var tempStream = System.IO.File.OpenRead(tempFilename))
        //    //    {
        //    //        await tempStream.CopyToAsync(stream).ConfigureAwait(false);
        //    //    }

        //    //    return myPicturesFilename;
        //}
        //private static async Task<string> WriteToSpecialFolderAsync(string tempFilename, string filename, Environment.SpecialFolder specialFolder)
        //{
        //    var folder = System.Environment.GetFolderPath(specialFolder);

        //    System.IO.Directory.CreateDirectory(folder);

        //    var myPicturesFilename = Path.Combine(folder, filename);

        //    using (var stream = System.IO.File.Create(myPicturesFilename))
        //    using (var tempStream = System.IO.File.OpenRead(tempFilename))
        //    {
        //        await tempStream.CopyToAsync(stream).ConfigureAwait(false);
        //    }

        //    return myPicturesFilename;
        //}

        //private static async Task<string> WriteToFolderAsync(byte[] data, string filename, Java.IO.File folder)
        //{
        //    folder.Mkdirs();

        //    var filePath = Path.Combine(folder.AbsolutePath, filename);

        //    using (var stream = new Java.IO.FileOutputStream(filePath))
        //    {
        //        await stream.WriteAsync(data).ConfigureAwait(false);
        //    }

        //    return filePath;
        //}


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private static async Task LaunchFileAsync(string filename)
        {
            //var fullPath = $"/storage/emulated/0/DCIM/Sketch360/{filename}";

            var request = new OpenFileRequest
            {

                File = new ReadOnlyFile(filename)
            };

            try
            {
                await Launcher.OpenAsync(request).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
            }
        }

        private static async Task<string> SaveToTempFileAsync(byte[] data, string filename)
        {
            var tempFilename = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), filename);

            using (var stream = System.IO.File.Create(tempFilename))
            {
                await stream.WriteAsync(data).ConfigureAwait(false);
            }

            return tempFilename;
        }

        public void AddExifData(string filename)
        {
            using var exif = new ExifInterface(filename);

            exif.SetAttribute(ExifInterface.TagXmp, Xmp);

            exif.SetAttribute(ExifInterface.TagSoftware, "Sketch 360");

            exif.SaveAttributes();
        }

        private static async Task<Android.Net.Uri> WriteDownloadValuesAsync(string tempfilename, string filename, string mimeType)
        {
            using var stream = System.IO.File.OpenRead(tempfilename);
            using var contentValues = new ContentValues();
            contentValues.Put(MediaStore.DownloadColumns.DisplayName, filename);
            contentValues.Put(MediaStore.DownloadColumns.MimeType, mimeType);


            var uri = await MainActivity.Instance.OpenSaveFileDialog(filename).ConfigureAwait(true);

            //var uri = MainActivity.Instance.ContentResolver.Insert(MediaStore.Downloads.ExternalContentUri, contentValues);

            if (uri == null) return null;

            using (var outputStream = MainActivity.Instance.ContentResolver.OpenOutputStream(uri))
            {
                await stream.CopyToAsync(outputStream).ConfigureAwait(false);
            }

            return uri;
        }

        private static async Task<Android.Net.Uri> WriteContentValuesAsync(string tempfilename, string filename, string mimeType)
        {
            using var stream = System.IO.File.OpenRead(tempfilename);
            using var contentValues = new ContentValues();
            contentValues.Put(MediaStore.MediaColumns.DisplayName, filename);
            contentValues.Put(MediaStore.MediaColumns.MimeType, mimeType);

            var uri = MainActivity.Instance.ContentResolver.Insert(MediaStore.Images.Media.ExternalContentUri, contentValues);

            if (uri == null) return null;

            using (var outputStream = MainActivity.Instance.ContentResolver.OpenOutputStream(uri))
            {
                await stream.CopyToAsync(outputStream).ConfigureAwait(false);
            }

            return uri;
        }
        #endregion
    }
}