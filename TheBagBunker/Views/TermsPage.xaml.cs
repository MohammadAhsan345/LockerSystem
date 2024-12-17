using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Documents;
using TheBagBunker.Helper;
using TheBagBunker.Model;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for TermsPage.xaml
    /// </summary>
    public partial class TermsPage : Page, INotifyPropertyChanged
    {
        private int lockerId = 0;
        private int userId = 0;
        private DateTime _processedTime;

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly LocalizationHelper _localizationHelper;

        public string PageTitle => _localizationHelper.GetTranslation("termsPageTitle", CLanguage.currentLanguage);
        public string TextBa => _localizationHelper.GetTranslation("termPageB1", CLanguage.currentLanguage);
        public string TextBb => _localizationHelper.GetTranslation("termPageB2", CLanguage.currentLanguage);
        public string TextLa => _localizationHelper.GetTranslation("termPageL1", CLanguage.currentLanguage);
        public string TextRa => _localizationHelper.GetTranslation("termPageR1", CLanguage.currentLanguage);
        public string TextLb => _localizationHelper.GetTranslation("termPageL2", CLanguage.currentLanguage);
        public string TextRb => _localizationHelper.GetTranslation("termPageR2", CLanguage.currentLanguage);
        public string TextRc => _localizationHelper.GetTranslation("termPageR3", CLanguage.currentLanguage);
        public string ButtonS => _localizationHelper.GetTranslation("termPageButton", CLanguage.currentLanguage);
        public TermsPage(int lockerId, int userId,
            DateTime processedTime)
        {
            InitializeComponent();
            _localizationHelper = new LocalizationHelper("translations.json");
            DataContext = this;
            this.lockerId = lockerId;
            this.userId = userId;
            _processedTime = processedTime;
            // Populate the TextBlock manually
            TextBlockWithLinks.Inlines.Clear();
            TextBlockWithLinks.Inlines.Add(new Run { Text = _localizationHelper.GetTranslation("termPageR1", CLanguage.currentLanguage) });
            TextBlockWithLinks.Inlines.Add(new Hyperlink
            {
                NavigateUri = new Uri("http://www.example.com"),
                Inlines = { new Run { Text = _localizationHelper.GetTranslation("termPageL1", CLanguage.currentLanguage) } }
            });
            TextBlockWithLinks.Inlines.Add(new Run { Text = _localizationHelper.GetTranslation("termPageR2", CLanguage.currentLanguage) });
            TextBlockWithLinks.Inlines.Add(new Hyperlink
            {
                NavigateUri = new Uri("http://www.example.com"),
                Inlines = { new Run { Text = _localizationHelper.GetTranslation("termPageL2", CLanguage.currentLanguage) } }
            });
            TextBlockWithLinks.Inlines.Add(new Run { Text = _localizationHelper.GetTranslation("termPageR3", CLanguage.currentLanguage) });
        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NavigateToNextPage(object sender, System.Windows.RoutedEventArgs e)
        {
            // Navigate to the next page
            this.NavigationService.Navigate(new AmountPage(lockerId, userId,
                Convert.ToInt32((DateTime.UtcNow - _processedTime).TotalSeconds)));
        }
    }
}
