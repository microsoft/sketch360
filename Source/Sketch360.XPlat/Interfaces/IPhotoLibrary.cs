// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace Sketch360.XPlat.Interfaces
{
    public interface IPhotoLibrary
    {
        //Task<Stream> PickPhotoAsync();

        Task<bool> SavePhotoAsync(byte[] data, string folder, string filename, Xamarin.Forms.Page page);

        Task<bool> SaveSketchAsync(byte[] data, string folder, string filename, Xamarin.Forms.Page page);

        void AddExifData(string filename);
    }
}
