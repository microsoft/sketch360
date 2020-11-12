// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.Data;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Sketch360.XPlat.Interfaces
{
    public interface IRemote
    {
        ObservableCollection<RemoteSystemInfo> RemoteSystems { get; }

        Task<bool> ConnectAsync();
    }
}
