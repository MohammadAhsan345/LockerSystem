using System.Windows.Controls;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private SharedLibrary _sharedLibrary = new SharedLibrary();
        public HomePage()
        {
            InitializeComponent();
            _sharedLibrary.ReleaseIdleLocks();
        }
        private void NavigateToLanguagePage(object sender, System.Windows.RoutedEventArgs e)
        {
            // Navigate to the next page
            this.NavigationService.Navigate(new languagePage());
        }
    }
}
