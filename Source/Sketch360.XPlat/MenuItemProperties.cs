// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using Xamarin.Forms;

namespace Sketch360.XPlat
{
    /// <summary>
    /// <see cref="MenuItem"/> attached properties
    /// </summary>
    public static class MenuItemProperties
    {
        /// <summary>
        /// BackgroundColor attached property
        /// </summary>
        /// <remarks>To use this, change the Build Action for the resource from AndroidImage to Embedded Resource</remarks>
        public static readonly BindableProperty BackgroundColorProperty = BindableProperty.CreateAttached(
            "BackgroundColor",
            typeof(Color),
            typeof(MenuItemProperties),
            Color.Transparent,
            default,
            default,
            OnBackgroundColorChanged);

        /// <summary>
        /// Gets the name of the embedded resource
        /// </summary>
        public static readonly BindableProperty ResourceNameProperty = BindableProperty.CreateAttached(
            "ResourceName",
            typeof(string),
            typeof(MenuItemProperties),
            string.Empty,
            default,
            default,
            OnResourceNameChanged);

        /// <summary>
        /// Gets or sets the bitmap scale
        /// </summary>
        public static double BitmapScale { get; set; } = 1.5;

        /// <summary>
        /// Gets the resource name
        /// </summary>
        /// <param name="target">the <see cref="MenuItem"/></param>
        /// <returns>the embedded resource name</returns>
        public static string GetResourceName(BindableObject target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            return (string)target.GetValue(ResourceNameProperty);
        }

        /// <summary>
        /// Sets the resource name
        /// </summary>
        /// <param name="target">the <see cref="MenuItem"/></param>
        /// <param name="name">the embedded resource name</param>
        public static void SetResourceName(BindableObject target, string name)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            target.SetValue(ResourceNameProperty, name);
        }

        /// <summary>
        /// Gets the background color
        /// </summary>
        /// <param name="target">the <see cref="MenuItem"/></param>
        /// <returns>the background color</returns>
        public static Color GetBackgroundColor(BindableObject target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            return (Color)target.GetValue(BackgroundColorProperty);
        }

        /// <summary>
        /// sets the background color
        /// </summary>
        /// <param name="target">the <see cref="MenuItem"/></param>
        /// <param name="color">the background color</param>
        public static void SetBackgroundColor(BindableObject target, Color color)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            target.SetValue(BackgroundColorProperty, color);
        }

        private static void OnResourceNameChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateImageSource(bindable);
        }

        private static void OnBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateImageSource(bindable);
        }

        private static void UpdateImageSource(BindableObject bindable)
        {
            if (bindable is MenuItem item)
            {
                var resourceName = GetResourceName(bindable);

                if (string.IsNullOrWhiteSpace(resourceName)) return;

                var baseUrl = DependencyService.Get<IBaseUrl>();

                var stream = baseUrl.GetDrawableImageStream(resourceName);

                if (stream != null)
                {
                    using (stream)
                    {
                        var bitmap = SKBitmap.Decode(stream);

                        SKBitmap newBitmap = null;
                        try
                        {
                            newBitmap = new SKBitmap(
                               Convert.ToInt32(bitmap.Width * BitmapScale),
                               Convert.ToInt32(bitmap.Height * BitmapScale));

                            using (var canvas = new SKCanvas(newBitmap))
                            {
                                var backgroundColor = GetBackgroundColor(bindable);

                                canvas.Clear(backgroundColor.ToSKColor());

                                canvas.DrawBitmap(bitmap, new SKRect(0, 0, newBitmap.Width, newBitmap.Height));
                            }

                            var newImageSource = (SKBitmapImageSource)newBitmap;

                            item.IconImageSource = newImageSource;

                            newBitmap = null;
                        }
                        finally
                        {
                            newBitmap?.Dispose();
                        }
                    }
                }
            }
        }
    }
}
