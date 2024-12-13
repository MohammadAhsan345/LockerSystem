using System.Windows.Controls;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for TermsPage.xaml
    /// </summary>
    public partial class TermsPage : Page
    {
        private int lockerId = 0;
        private int userId = 0;
        private DateTime _processedTime;
        public TermsPage(int lockerId, int userId,
            DateTime processedTime)
        {
            InitializeComponent();
            this.lockerId = lockerId;
            this.userId = userId;
            _processedTime = processedTime;
        }

        private void NavigateToNextPage(object sender, System.Windows.RoutedEventArgs e)
        {
            // Navigate to the next page
            this.NavigationService.Navigate(new AmountPage(lockerId, userId,
                Convert.ToInt32((DateTime.UtcNow - _processedTime).TotalSeconds)));
        }
    }
}
