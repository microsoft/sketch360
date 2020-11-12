using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(Sketch360.XPlat.UWP.BaseUrl))]

namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// UWP Base URL
    /// </summary>
    public class BaseUrl : IBaseUrl
    {
        /// <summary>
        /// Gets an appx package url scheme
        /// </summary>
        /// <returns>an appx url scheme</returns>
        public string GetBase()
        {
            return "ms-appx-web:///html/";
        }

        /// <summary>
        /// Gets the drawable image stream for UWP.
        /// </summary>
        /// <param name="filename">the filename</param>
        /// <returns>a image stream</returns>
        public Stream GetDrawableImageStream(string filename)
        {
            throw new System.NotImplementedException();
        }
    }
}
