// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
//using Microsoft.AppCenter.Distribute;
using Plugin.Multilingual;
using Sketch360.XPlat.Data;
using Sketch360.XPlat.Interfaces;
using Sketch360.XPlat.Modes;
using Sketch360.XPlat.Resources;
using Sketch360.XPlat.Serialization;
using Sketch360.XPlat.Support;
using Sketch360.XPlat.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Sketch360.XPlat
{
    /// <summary>
    /// Xamarin Forms App
    /// </summary>
    public sealed partial class App : Application, IDisposable
    {
        private SketchData _sketchData;
        private readonly ISplitPage _splitPage;
        private readonly UndoManager _undoManager = new UndoManager();
        /// <summary>
        /// Initializes a new instance of the App class
        /// </summary>
        public App()
        {

            VersionTracking.Track();

            InitializeContainer();

            InitializeComponent();

            var culture = CrossMultilingual.Current.DeviceCultureInfo;

            AppResources.Culture = culture;

            _splitPage = App.Container.Resolve<ISplitPage>();

            if (VersionTracking.IsFirstLaunchForCurrentVersion)
            {
                // Navigating to Carousel on first time load
                var carouselPage = App.Container.Resolve<ICarouselPage>();
                MainPage = new NavigationPage(carouselPage as Page);
            }
            else
            {
                MainPage = new NavigationPage(_splitPage as Page);
            }
        }

        /// <summary>
        /// Gets the sketch data
        /// </summary>
        /// <returns>the sketch data</returns>
        public static ISketchData GetSketchData()
        {
            if (App.Current is App app)
            {
                return app._sketchData;
            }

            return null;
        }

        /// <summary>
        /// Gets or sets the AutoFAC DI Container
        /// </summary>
        public static IContainer Container { get; private set; }

        /// <summary>
        /// Gets the sketch data path
        /// </summary>
        public static string SketchDataPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "_sketchData.json");
            }
        }

        private bool _isSaving;

        /// <summary>
        /// Show the carousel pages
        /// </summary>
        public void ShowCarouselPage()
        {
            var carouselPage = App.Container.Resolve<ICarouselPage>();

            MainPage = new NavigationPage(carouselPage as Page);
        }

        /// <summary>
        /// Serialize the sketch data
        /// </summary>
        /// <returns></returns>
        public string SerializeSketchData()
        {
            lock (_sketchData)
            {
                var sketchData = new SketchData
                {
                    BackgroundColor = _sketchData.BackgroundColor,
                    Height = _sketchData.Height,
                    InkStrokes = _sketchData.InkStrokes.ToArray(),
                    Width = _sketchData.Width,
                    Name = _sketchData.Name,
                    Start = _sketchData.Start
                };

                var options = CreateSerializationOptions();

                var json = JsonSerializer.Serialize(sketchData, options);

                return json;
            }
        }
        /// <summary>
        /// Save the sketch data
        /// </summary>
        /// <returns>an async task</returns>
        public async Task SaveSketchDataAsync()
        {
            if (_isSaving) return;

            _isSaving = true;

            await Task.Run(delegate
            {
                _isSaving = true;

                var json = SerializeSketchData();

                File.WriteAllText(SketchDataPath, json);

                _isSaving = false;
            }).ConfigureAwait(false);
        }

        static JsonSerializerOptions CreateSerializationOptions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new ColorConverter());
            options.Converters.Add(new SKRectConverter());
            //options.Converters.Add(new PointConverter());
            //options.Converters.Add(new DateTimeOffsetConverter()); // the default converter is sufficient

            options.WriteIndented = true;

            return options;
        }


        /// <summary>
        /// Load a sketch from a JSON string
        /// </summary>
        /// <param name="json">the JSON string</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public bool LoadSketch(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            var options = CreateSerializationOptions();

            try
            {
                var sketchData = JsonSerializer.Deserialize<SketchData>(json, options);

                foreach (var stroke in sketchData.InkStrokes)
                {
                    if (stroke.DrawingAttributes.Color.A == 0)
                    {
                        stroke.DrawingAttributes.Color = Xamarin.Forms.Color.FromRgba(stroke.DrawingAttributes.Color.R, stroke.DrawingAttributes.Color.G, stroke.DrawingAttributes.Color.B, 255);
                    }
                    stroke.UpdateBounds();
                }

                _sketchData = sketchData;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Error deserializing SketchData: {e.Message}");

                _sketchData = new SketchData();

                Crashes.TrackError(e);
            }

            _splitPage.SketchDataUpdated(_sketchData);

            return true;
        }

        public async Task LoadInitialSketchAsync()
        {
            var assembly = GetType().GetTypeInfo().Assembly;

            using var stream = assembly.GetManifestResourceStream("Sketch360.XPlat.Assets.sketch360.json");
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var json = await reader.ReadToEndAsync().ConfigureAwait(false);

                LoadSketch(json);
            }
        }

        /// <summary>
        /// App startup
        /// </summary>
        protected override async void OnStart()
        {
            AppCenter.Start(
                  "android=585676e3-a68b-48f0-8090-119788c6b1e5;" +
                  //"uwp={Your UWP App secret here};" +
                  "ios={87bd45a1-e43a-4c0f-9401-0ac71509de57}",
                  typeof(Analytics), typeof(Crashes));//, typeof(Distribute));

            await Task.Run(async delegate
            {
                var path = SketchDataPath;

                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);

                    LoadSketch(json);
                }
                else
                {
                    if (VersionTracking.IsFirstLaunchEver)
                    {
                        await LoadInitialSketchAsync().ConfigureAwait(true);
                    }
                    else
                    {
                        _sketchData = new SketchData();

                        _splitPage.SketchDataUpdated(_sketchData);
                    }
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// App sleep
        /// </summary>
        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        /// <summary>
        /// App resume
        /// </summary>
        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        private void InitializeContainer()
        {
            var builder = new Autofac.ContainerBuilder();

            builder.RegisterType<AddStrokesUndoItem>().As<IAddStrokesUndoItem>();
            builder.RegisterType<DrawingMode>().As<IDrawingMode>();
            builder.RegisterType<DrawingPageViewModel>().As<IDrawingPageViewModel>();
            builder.RegisterType<ErasingMode>().As<IErasingMode>();
            builder.RegisterType<EraseStrokesUndoItem>().As<IEraseStrokesUndoItem>();
            builder.RegisterType<PanningMode>().As<IPanningMode>();
            builder.RegisterType<SplitPage>().As<ISplitPage>();
            builder.RegisterType<CarouselPage>().As<ICarouselPage>();
            builder.RegisterType<SplitPageViewModel>().As<ISplitPageViewModel>();
            builder.RegisterInstance(_undoManager).As<IUndoManager>();

            Container = builder.Build();
        }

        /// <summary>
        /// Dispose of the resources
        /// </summary>
        public void Dispose()
        {
            _undoManager?.Dispose();
        }
    }
}
