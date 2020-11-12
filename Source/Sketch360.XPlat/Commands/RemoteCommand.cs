// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.Core.Commands;
using Sketch360.XPlat.Interfaces;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sketch360.XPlat.Commands
{
    public class RemoteCommand : BusyCommand
    {
        private readonly IRemote _remote;

        public RemoteCommand()
        {
            _remote = DependencyService.Get<IRemote>();
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            if (await _remote.ConnectAsync().ConfigureAwait(false))
            {

            }
        }
    }
}
