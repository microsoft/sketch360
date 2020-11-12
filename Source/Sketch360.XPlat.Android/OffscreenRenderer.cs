using Android.Graphics;
using Sketch360.XPlat.Data;
using SkiaSharp;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

[assembly: Dependency(typeof(Sketch360.XPlat.Droid.OffscreenRenderer))]

namespace Sketch360.XPlat.Droid
{
    public class OffscreenRenderer : IOffscreenRenderer
    {
        public void Render(float width, float height, IEnumerable<InkStroke> strokes)
        {
            using (var bitmap = Bitmap.CreateBitmap((int) width, (int) height, Bitmap.Config.Argb8888))
            {
                try
                {
                    using (var surface = SKSurface.Create((int) width, (int) height, SKColorType.Rgba8888, SKAlphaType.Premul, bitmap.LockPixels(), (int) width * 4))
                    {
                        var skcanvas = surface.Canvas;

                        skcanvas.Draw(strokes);
                    }
                }
                catch(Exception e)
                {

                }
                finally
                {
                    bitmap.UnlockPixels();
                }

                //bitmap.
            }
        }
    }
}