using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using TheBagBunker.Helper;
using TheBagBunker.Model;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for successPayment.xaml
    /// </summary>
    public partial class successPayment : Page, INotifyPropertyChanged
    {
        int lockerId = 0;
        int userId = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly LocalizationHelper _localizationHelper;

        public string PageTitle => _localizationHelper.GetTranslation("successPageTitle", CLanguage.currentLanguage);
        public string SuccessDes => _localizationHelper.GetTranslation("successPageDes", CLanguage.currentLanguage);
        public string ButtonS => _localizationHelper.GetTranslation("successPageButton", CLanguage.currentLanguage);

        public successPayment(int lockerId, int userId)
        {
            InitializeComponent();
            this.lockerId = lockerId;
            this.userId = userId;
            _localizationHelper = new LocalizationHelper("translations.json");
            DataContext = this;
        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public successPayment()
        {
            InitializeComponent();
        }

        private void NavigateToNextPage(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HomePage());
        }
    }
}
