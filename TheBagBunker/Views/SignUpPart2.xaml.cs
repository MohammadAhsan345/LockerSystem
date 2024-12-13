using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for SignUpPart2.xaml
    /// </summary>
    public partial class SignUpPart2 : Page
    {
        private int userId = 0;
        private int lockerId = 0;
        private SharedLibrary _sharedLibrary = new SharedLibrary();

        public SignUpPart2(int userId)
        {
            InitializeComponent();
            this.userId = userId;
        }

        private void NavigateToNextPage(object sender, RoutedEventArgs e)
        {
            if (userId == 0)
            {
                MessageBox.Show("User not Found!!!");
                return;
            }

            if (string.IsNullOrEmpty(PasswordTextBox2.Text))
            {
                MessageBox.Show("Please Provide Password!!!");
                return;
            }

            //this.NavigationService.Navigate(new TermsPage());
            if (!Regex.IsMatch(PasswordTextBox2.Text.Trim(), @"^\d{6,}$"))
            {
                MessageBox.Show("Password should be atleast 6 or more Digits only!!!");
                return;
            }

            if (PasswordTextBox2.Text.Trim() != PasswordTextBox3.Text.Trim())
            {
                MessageBox.Show("Password doesn't match!!!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(SharedLibrary._ConnectionString))
                {
                    conn.Open();

                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE Users SET Password = @Password WHERE Id = @Id;";
                        cmd.Parameters.AddWithValue("@Password", PasswordTextBox2.Text);
                        cmd.Parameters.AddWithValue("@Id", userId);
                        cmd.ExecuteNonQuery();
                        conn.Close();

                        (lockerId, DateTime processedTime) = _sharedLibrary.LockerAvailability(conn, userId);

                        if (lockerId == 0)
                        {
                            MessageBox.Show("Your password has been Updated. But We're really sorry. " +
                                "Currently, Either no Locker is Present or other Lockers are In Process " +
                                "to get acquired. " +
                                "Please try again later!!!");
                            return;
                        }

                        // Navigate to the next page
                        this.NavigationService.Navigate(new TermsPage(lockerId, userId, processedTime));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        //private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    if (PasswordBox.Password.Length == 0)
        //    {
        //        PlaceholderPassword.Visibility = Visibility.Collapsed;
        //    }
        //}

        //// Event handler for the LostFocus event of the PasswordBox
        //private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (PasswordBox.Password.Length == 0)
        //    {
        //        PlaceholderPassword.Visibility = Visibility.Visible;
        //    }
        //}
        //private void PasswordBox1_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    if (PasswordBox1.Password.Length == 0)
        //    {
        //        PlaceholderPassword1.Visibility = Visibility.Collapsed;
        //    }
        //}

        //// Event handler for the LostFocus event of the PasswordBox
        //private void PasswordBox1_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (PasswordBox1.Password.Length == 0)
        //    {
        //        PlaceholderPassword1.Visibility = Visibility.Visible;
        //    }
        //}

        private void PasswordBox2_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox2.Password.Length == 0)
            {
                PlaceholderPassword2.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordBox2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox2.Password.Length == 0)
            {
                PlaceholderPassword2.Visibility = Visibility.Visible;
            }
        }

        private void TogglePasswordVisibilityButton2_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox2.Visibility == Visibility.Visible)
            {
                PasswordTextBox2.Text = PasswordBox2.Password;
                PasswordBox2.Visibility = Visibility.Collapsed;
                PasswordTextBox2.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordBox2.Password = PasswordTextBox2.Text;
                PasswordBox2.Visibility = Visibility.Visible;
                PasswordTextBox2.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordBox2_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordTextBox2.Text = PasswordBox2.Password;
        }

        private void PasswordTextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordBox2.Password = PasswordTextBox2.Text;
        }

        private void PasswordBox3_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox3.Password.Length == 0)
            {
                PlaceholderPassword3.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordBox3_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox3.Password.Length == 0)
            {
                PlaceholderPassword3.Visibility = Visibility.Visible;
            }
        }

        private void TogglePasswordVisibilityButton3_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox3.Visibility == Visibility.Visible)
            {
                PasswordTextBox3.Text = PasswordBox3.Password;
                PasswordBox3.Visibility = Visibility.Collapsed;
                PasswordTextBox3.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordBox3.Password = PasswordTextBox3.Text;
                PasswordBox3.Visibility = Visibility.Visible;
                PasswordTextBox3.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordBox3_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordTextBox3.Text = PasswordBox3.Password;
        }

        private void PasswordTextBox3_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordBox3.Password = PasswordTextBox3.Text;
        }

    }
}
