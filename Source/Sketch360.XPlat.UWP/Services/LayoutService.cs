using System;
using System.Collections.Generic;
using Xamarin.Duo.Forms.Samples;
using Xamarin.Forms;

[assembly: Dependency(typeof(Sketch360.XPlat.UWP.Services.LayoutService))]

namespace Sketch360.XPlat.UWP.Services
{
    public sealed class LayoutService : ILayoutService
    {
        private readonly Dictionary<string, LayoutGuide> _layoutGuides = new Dictionary<string, LayoutGuide>();

        public IReadOnlyDictionary<string, LayoutGuide> LayoutGuides => _layoutGuides;

        public event EventHandler<LayoutGuideChangedArgs> LayoutGuideChanged;

        public void AddLayoutGuide(string name, Rectangle location)
        {
        }

        public Point? GetLocationOnScreen(VisualElement visualElement)
        {
            return null;
        }
    }
}
