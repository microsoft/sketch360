namespace Sketch360.XPlat.UWP
{
    /// <summary>
    /// Xamarin Forms MainPage class
    /// </summary>
    public sealed partial class MainPage
    {
        /// <summary>
        /// Initializes a new instance of the MainPage class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        public MainPage()
        {
            this.InitializeComponent();

            var app = new Sketch360.XPlat.App();

            LoadApplication(app);
        }
    }
}
