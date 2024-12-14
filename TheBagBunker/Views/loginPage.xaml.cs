using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TheBagBunker.Helper;
using TheBagBunker.Model;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for loginPage.xaml
    /// </summary>
    public partial class loginPage : Page, INotifyPropertyChanged
    {
        private SharedLibrary _sharedLibrary = new SharedLibrary();

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly LocalizationHelper _localizationHelper;

        public string PageTitle => _localizationHelper.GetTranslation("loginPageTitle", CLanguage.currentLanguage);
        public string Op1 => _localizationHelper.GetTranslation("loginPagePhonr", CLanguage.currentLanguage);
        public string Op2 => _localizationHelper.GetTranslation("loginPagePassword", CLanguage.currentLanguage);
        public string ButtonB => _localizationHelper.GetTranslation("loginPageButton", CLanguage.currentLanguage);
        public string NP => _localizationHelper.GetTranslation("loginPageNP", CLanguage.currentLanguage);
        public string PP => _localizationHelper.GetTranslation("loginPagePP", CLanguage.currentLanguage);

        public loginPage()
        {
            InitializeComponent();
            _localizationHelper = new LocalizationHelper("translations.json");
            DataContext = this;
        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void NavigateToNextPage(object sender, System.Windows.RoutedEventArgs e)
        {
            AuhenticateUser();
        }

        private void AuhenticateUser()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(SharedLibrary._ConnectionString))
                {
                    conn.Open();

                    string query = "SELECT Id, Password FROM Users WHERE Phone = @Phone;";


                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@Phone", PhoneTextBox.Text.Trim());

                        using (MySqlDataAdapter SDA = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            SDA.Fill(dt);

                            if (dt.Rows.Count == 1)
                            {
                                string? password = dt.Rows[0]["Password"].ToString();
                                int userId = Convert.ToInt32(dt.Rows[0]["Id"]);

                                if (string.IsNullOrEmpty(password))
                                {
                                    MessageBox.Show("You didn't have password. So, you have not done Signup Cycle " +
                                        "and not assigned any locker. After this message box, you will be redirected to this. " +
                                        "Please complete Signup!!!");
                                    this.NavigationService.Navigate(new SignUpPart2(userId));
                                    return;
                                }

                                if (!string.IsNullOrEmpty(PasswordTextBox.Text)
                                    && password.Equals(PasswordTextBox.Text))
                                {
                                    cmd.CommandText = "SELECT * FROM LockerInformation WHERE UserId = @UserId;";
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("@UserId", userId);

                                    using (MySqlDataAdapter SDA2 = new MySqlDataAdapter(cmd))
                                    {
                                        dt = new DataTable();
                                        SDA2.Fill(dt);

                                        if (dt.Rows.Count == 0)
                                        {
                                            (int lockerId, DateTime processedTime) = _sharedLibrary
                                                .LockerAvailability(conn, userId);

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
                                        else
                                        {
                                            DateTime lockerUpTime = Convert.ToDateTime(dt.Rows[0]["LockerUpTime"]);
                                            int lockerId = Convert.ToInt32(dt.Rows[0]["LockerId"]);
                                            TimeSpan timeDifference = DateTime.UtcNow - lockerUpTime;

                                            if (timeDifference.TotalHours >= 3)
                                            {
                                                this.NavigationService.Navigate(new AmountPage(lockerId, userId));
                                            }
                                            else
                                            {
                                                this.NavigationService.Navigate(new lastPage());
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Wrong Credentials!!!");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Wrong Credentials!!!");
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void phonetextbox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText.Visibility = Visibility.Collapsed;
        }

        private void phonetextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
        }
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length == 0)
            {
                PlaceholderPassword.Visibility = Visibility.Collapsed;
            }
        }
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length == 0)
            {
                PlaceholderPassword.Visibility = Visibility.Visible;
            }
        }
        private void TogglePasswordVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                // Switch to plain text password
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                // Switch to hidden password
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = PasswordBox.Password; // Sync password to TextBox
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasswordBox.Password = PasswordTextBox.Text; // Sync plain text to PasswordBox
        }



    }
}
