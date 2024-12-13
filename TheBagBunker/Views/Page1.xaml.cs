using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        private string _selectedBtnName = string.Empty;
        public Page1()
        {
            InitializeComponent();
        }

        private void NavigateToNextPage(object sender, System.Windows.RoutedEventArgs e)
        {
            // Navigate to the next page
            if (_selectedBtnName == DropOff_Btn.Name)
                this.NavigationService.Navigate(new SignUpPart1());
            else if (_selectedBtnName == Collect_Btn.Name)
                this.NavigationService.Navigate(new loginPage());

        }
        private void NavigateToBackPage(object sender, System.Windows.RoutedEventArgs e)
        {
            // Navigate to the next page
            this.NavigationService.Navigate(new languagePage());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                _selectedBtnName = btn.Name;
            }

            // Reset the BorderBrush of all buttons to LightGray
            foreach (var child in FindVisualChildren<Button>(this))
            {
                child.BorderBrush = Brushes.LightGray;
            }

            // Set the BorderBrush of the clicked button to Red
            if (sender is Button clickedButton)
            {
                clickedButton.BorderBrush = Brushes.Red;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild)
                {
                    yield return tChild;
                }

                foreach (var descendant in FindVisualChildren<T>(child))
                {
                    yield return descendant;
                }
            }
        }

    }

}
