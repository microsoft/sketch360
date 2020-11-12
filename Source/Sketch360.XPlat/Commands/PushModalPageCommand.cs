// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.Core.Commands;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.DualScreen;

namespace Sketch360.XPlat.Commands
{

    /// <summary>
    /// Push modal page command
    /// </summary>
    /// <typeparam name="TPageType">the type of Xamarin Page</typeparam>
    public class PushModalPageCommand<TPageType> : BusyCommand where TPageType : Page, new()
    {
        /// <summary>
        /// Push the page onto the navigation stack
        /// </summary>
        /// <param name="parameter">the parameter is not used</param>
        /// <returns>an async task</returns>
        protected override async Task ExecuteAsync(object parameter)
        {
            var page = App.Current.MainPage as Page;
            await page.Navigation.PushAsync(new TPageType(), true).ConfigureAwait(false);
        }
    }
}
