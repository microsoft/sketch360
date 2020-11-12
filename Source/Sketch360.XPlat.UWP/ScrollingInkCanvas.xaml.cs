using Windows.UI.Xaml.Controls;

namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// Scrolling ink canvas
    /// </summary>
    public sealed partial class ScrollingInkCanvas : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the ScrollingInkCanvas class.
        /// </summary>
        public ScrollingInkCanvas()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the ink canvas
        /// </summary>
        public InkCanvas InkCanvas => InkCanvasControl;
    }
}
