using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheBagBunker.Helper;
using TheBagBunker.Model;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for lastPage.xaml
    /// </summary>
    public partial class lastPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly LocalizationHelper _localizationHelper;

        public string PageTitle => _localizationHelper.GetTranslation("lastPageTitle", CLanguage.currentLanguage);
        public string Des => _localizationHelper.GetTranslation("lastPageDes", CLanguage.currentLanguage);
        public string ButtonS => _localizationHelper.GetTranslation("lastPageButton", CLanguage.currentLanguage);
        public lastPage()
        {
            InitializeComponent();
            _localizationHelper = new LocalizationHelper("translations.json");
            DataContext = this;
        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void lastButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new HomePage());
        }
    }
}
