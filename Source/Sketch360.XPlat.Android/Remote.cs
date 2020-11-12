// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Android.App;
using Android.Content;
using Android.Webkit;
using Android.Widget;
using Microsoft.ConnectedDevices;
using Sketch360.XPlat.Data;
using Sketch360.XPlat.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(Sketch360.XPlat.Droid.Remote))]

namespace Sketch360.XPlat.Droid
{
    /// <summary>
    /// Remote system using Project Rome to send data to other systems
    /// </summary>
    public sealed class Remote : IRemote, IDisposable
    {
        const string AppId = "f2d1007c-dbd7-4d32-a488-93a6bdbc12b9";

        private WebView _webView;
        internal Dialog _authDialog;
        private RemoteSystemWatcher _remoteSystemWatcher;

        public Remote()
        {
            RemoteSystems = new ObservableCollection<RemoteSystemInfo>();
        }

        public static MainActivity MainActivity { get; set; }

        public ObservableCollection<RemoteSystemInfo> RemoteSystems { get; private set; }

        public async Task<bool> ConnectAsync()
        {
            Platform.FetchAuthCode += Platform_FetchAuthCode;

            if (await Platform.InitializeAsync(MainActivity, AppId).ConfigureAwait(false))
            {
                DiscoverDevices();

                return true;
            }

            return false;
        }

        public void DiscoverDevices()
        {
            _remoteSystemWatcher = RemoteSystem.CreateWatcher();

            _remoteSystemWatcher.RemoteSystemAdded += RemoteSystemAdded;
            _remoteSystemWatcher.RemoteSystemRemoved += RemoteSystemRemoved;
            _remoteSystemWatcher.RemoteSystemUpdated += RemoteSystemUpdated;

            _remoteSystemWatcher.Start();
        }

        private void RemoteSystemUpdated(RemoteSystemWatcher watcher, RemoteSystemUpdatedEventArgs args)
        {
        }

        private void RemoteSystemRemoved(RemoteSystemWatcher watcher, RemoteSystemRemovedEventArgs args)
        {
            MainActivity.RunOnUiThread(delegate
            {
                var itemToRemove = (from item in RemoteSystems
                                    where item.Id == args.P0
                                    select item).FirstOrDefault();

                if (itemToRemove != null)
                {
                    RemoteSystems.Remove(itemToRemove);
                }
            });
        }

        private void RemoteSystemAdded(RemoteSystemWatcher watcher, RemoteSystemAddedEventArgs args)
        {
            MainActivity.RunOnUiThread(delegate
            {
                RemoteSystems.Add(new RemoteSystemInfo
                {
                    DisplayName = args.P0.DisplayName,
                    Id = args.P0.Id,
                    Kind = args.P0.Kind.ToString()
                });
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private void Platform_FetchAuthCode(string oauthUrl)
        {
            _authDialog = new Dialog(MainActivity);

            var linearLayout = new LinearLayout(_authDialog.Context);
            _webView = new WebView(_authDialog.Context);
            linearLayout.AddView(_webView);
            _authDialog.SetContentView(linearLayout);

            _webView.SetWebChromeClient(new WebChromeClient());
            _webView.Settings.JavaScriptEnabled = true;
            _webView.Settings.DomStorageEnabled = true;
            _webView.LoadUrl(oauthUrl);

            _webView.SetWebViewClient(new MsaWebViewClient(MainActivity, _authDialog));
            _authDialog.Show();
            _authDialog.SetCancelable(true);
        }

        /// <summary>
        /// Dispose of the auth dialog and webview
        /// </summary>
        public void Dispose()
        {
            if (_authDialog != null)
            {
                _authDialog.Dispose();
                _authDialog = null;
            }

            if (_webView != null)
            {
                _webView.Dispose();
                _webView = null;
            }
        }

        internal class MsaWebViewClient : WebViewClient
        {
            bool authComplete;

            private readonly MainActivity _parentActivity;
            private readonly Dialog _authDialog;

            public MsaWebViewClient(MainActivity activity, Dialog authDialog)
            {
                _parentActivity = activity;
                _authDialog = authDialog;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);
                if (url.Contains("?code=", StringComparison.InvariantCultureIgnoreCase) && !authComplete)
                {
                    authComplete = true;

                    var uri = Android.Net.Uri.Parse(url);
                    string token = uri.GetQueryParameter("code");
                    _authDialog.Dismiss();
                    Platform.SetAuthCode(token);
                }
                else if (url.Contains("error=access_denied", StringComparison.InvariantCultureIgnoreCase))
                {
                    authComplete = true;
                    //Console.WriteLine("Page finished failed with ACCESS_DENIED_HERE");
                    Intent resultIntent = new Intent();
                    _parentActivity.SetResult(0, resultIntent);
                    _authDialog.Dismiss();
                }

            }
        }
    }
}