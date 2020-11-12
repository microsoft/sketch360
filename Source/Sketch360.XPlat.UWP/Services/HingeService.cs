using System;
using System.ComponentModel;
using Xamarin.Duo.Forms.Samples;
using Xamarin.Forms;

[assembly: Dependency(typeof(Sketch360.XPlat.UWP.Services.HingeService))]

namespace Sketch360.XPlat.UWP.Services
{
    public sealed class HingeService : IHingeService
    {
        public bool IsSpanned => false;

        public bool IsLandscape => true;

        public event EventHandler<HingeEventArgs> OnHingeUpdated;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
        }

        public Rectangle GetHinge()
        {
            return Rectangle.Zero;
        }
    }
}
