// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
//using Microsoft.AppCenter.Distribute;
using System.Threading.Tasks;
//using Xamarin.Duo.Forms.Samples;

namespace Sketch360.XPlat.Droid
{
    /// <summary>
    /// Android main activity
    /// </summary>
    [Activity(Label = "Sketch 360", Icon = "@drawable/logo", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    [IntentFilter(new[] { Intent.ActionView, Intent.ActionMain, Intent.ActionEdit },
        //Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault, Intent.CategoryOpenable, Intent.CategoryLauncher},
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },

        DataMimeTypes = new[] { "application/x-sketch360", "application/octet-stream" },
        DataScheme = "file",
        Label = "Sketch 360",
        DataPathPatterns = new[]
        {
            "*.sketch360",
            ".*\\.sketch360",
            ".*\\..*\\.sketch360",
            ".*\\..*\\..*\\.sketch360",
            ".*\\..*\\..*\\..*\\.sketch360"
        },
        Icon = "@drawable/logo",
        DataHost = "*")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        internal static MainActivity Instance { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        protected override void OnCreate(Bundle savedInstanceState)
        {
            global::Xamarin.Forms.DualScreen.DualScreenService.Init(this);
            Remote.MainActivity = this;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            Instance = this;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            //Distribute.SetEnabledForDebuggableBuild(true);
            
            LoadApplication(new App());

            //enable Android webview debugging
            global::Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 101)
            {
                switch (resultCode)
                {
                    case Result.Ok:
                        if (data != null && data.Data != null)
                        {
                            _fileName?.SetResult(data.Data);
                        }
                        else
                            _fileName?.SetResult(null);

                        break;
                    default:
                        _fileName?.SetResult(null);
                        break;
                }
            }
        }

        TaskCompletionSource<Android.Net.Uri> _fileName;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        public Task<Android.Net.Uri> OpenSaveFileDialog(string currentFileName)
        {
            Intent intent = new Intent(Intent.ActionCreateDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("application/octet-stream");
            intent.PutExtra(Intent.ExtraTitle, currentFileName);
            _fileName = new TaskCompletionSource<Android.Net.Uri>();
            StartActivityForResult(intent, 101);
            return _fileName.Task;
        }
    }
}