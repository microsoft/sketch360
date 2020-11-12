using Android.Content;
using Android.Graphics;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Sketch360.XPlat.InkCanvas), typeof(Sketch360.XPlat.Droid.InkCanvasRenderer))]

namespace Sketch360.XPlat.Droid
{
    /// <summary>
    /// InkCanvas renderer for Android
    /// </summary>
    public class InkCanvasRenderer : ViewRenderer
    {
        Paint _inkPaint = new Paint();

        /// <summary>
        /// Initializes a new instance of the InkCanvasRenderer class
        /// </summary>
        /// <param name="context">the context</param>
        public InkCanvasRenderer(Context context) : base(context)
        {
        }

        protected override void OnDraw(Canvas canvas)
        {
            if (canvas == null) throw new ArgumentNullException(nameof(canvas));

            canvas.DrawText("InkCanvas", 100, 100, _inkPaint);
        }
    }
}