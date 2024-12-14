﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheBagBunker.Model;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for languagePage.xaml
    /// </summary>
    public partial class languagePage : Page
    {
        public languagePage()
        {
            InitializeComponent();
        }


        private void NavigateToNextPage(object sender, System.Windows.RoutedEventArgs e)
        {
            // Navigate to the next page
            if (CLanguage.currentLanguage == null)
            {
                MessageBox.Show("Choose Language..");
            }
            else
            {
                this.NavigationService.Navigate(new Page1());
            }
        }

        private void NavigateToBackPage(object sender, System.Windows.RoutedEventArgs e)
        {
            // Navigate to the next page
            this.NavigationService.Navigate(new HomePage());
        }

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset the BorderBrush of all buttons to LightGray
            foreach (var child in FindVisualChildren<Button>(this))
            {
                child.BorderBrush = Brushes.LightGray;
            }

            // Set the BorderBrush of the clicked button to Red
            if (sender is Button clickedButton)
            {
                clickedButton.BorderBrush = Brushes.Red;
                // Access the TextBlock within the Button's ControlTemplate
                var textBlock = FindVisualChild<TextBlock>(clickedButton);

                if (textBlock != null)
                {
                    // Set the current language based on the TextBlock's Text
                    CLanguage.currentLanguage = textBlock.Text.Substring(0, 2);
                }
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
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                {
                    return t;
                }

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }
    }
}
