using System.Windows.Controls;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for successPayment.xaml
    /// </summary>
    public partial class successPayment : Page
    {
        int lockerId = 0;
        int userId = 0;
        public successPayment(int lockerId, int userId)
        {
            InitializeComponent();
            this.lockerId = lockerId;
            this.userId = userId;
        }

        public successPayment()
        {
            InitializeComponent();
        }

        private void NavigateToNextPage(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new loginPage());
        }
    }
}
