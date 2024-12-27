using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using TheBagBunker.Helper;
using TheBagBunker.Model;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for SignUpPart1.xaml
    /// </summary>

    public partial class SignUpPart1 : Page, INotifyPropertyChanged
    {
        private string _conString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
        private SharedLibrary _sharedLibrary = new SharedLibrary();

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly LocalizationHelper _localizationHelper;

        public string PageTitle => _localizationHelper.GetTranslation("signUpPageTitle", CLanguage.currentLanguage);
        public string Name1 => _localizationHelper.GetTranslation("signUpPageFullName", CLanguage.currentLanguage);
        public string Nationality => _localizationHelper.GetTranslation("signUpPageNationality", CLanguage.currentLanguage);
        public string Passport => _localizationHelper.GetTranslation("signUpPagePassportNo", CLanguage.currentLanguage);
        public string Phone => _localizationHelper.GetTranslation("signUpPagePhoneNumber", CLanguage.currentLanguage);
        public string Email => _localizationHelper.GetTranslation("signUpPageEmail", CLanguage.currentLanguage);
        public string ButtonS => _localizationHelper.GetTranslation("signUpPageButton", CLanguage.currentLanguage);
        public string PlaceHolderName => _localizationHelper.GetTranslation("signUpPageNameP", CLanguage.currentLanguage);
        public string PlaceHolderCountry => _localizationHelper.GetTranslation("signUpPageNationalityP", CLanguage.currentLanguage);
        public string PlaceHolderPassport => _localizationHelper.GetTranslation("signUpPagePassportP", CLanguage.currentLanguage);
        public string PlaceHolderNumber => _localizationHelper.GetTranslation("signUpPageNumberP", CLanguage.currentLanguage);
        public string PlaceHolderEmail => _localizationHelper.GetTranslation("signUpPageEmailP", CLanguage.currentLanguage);

        public List<Country> Countries { get; set; }

        public SignUpPart1()
        {
            InitializeComponent();
            _localizationHelper = new LocalizationHelper("translations.json");
            LoadCountries();
            DataContext = this;
        }
        private void LoadCountries()
        {
            Countries = new List<Country>
            {
                new Country { Name = "Afghanistan" },
                new Country { Name = "Albania" },
                new Country { Name = "Algeria" },
                new Country { Name = "Andorra" },
                new Country { Name = "Angola" },
                new Country { Name = "Argentina" },
                new Country { Name = "Australia" },
                new Country { Name = "Austria" },
                new Country { Name = "Bahamas" },
                new Country { Name = "Bahrain" },
                new Country { Name = "Bangladesh" },
                new Country { Name = "Belgium" },
                new Country { Name = "Bhutan" },
                new Country { Name = "Brazil" },
                new Country { Name = "Canada" },
                new Country { Name = "China" },
                new Country { Name = "France" },
                new Country { Name = "Germany" },
                new Country { Name = "India" },
                new Country { Name = "Pakistan" },
                new Country { Name = "Spain" },
                new Country { Name = "United States" }
                // Add all the countries here
            };
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool IsNotValidated()
        {
            if (lstComboItems.SelectedItem is ListBoxItem selectedItem)
            {
                // Update the TextBlock with the selected value
                txtSelectedItem.Text = selectedItem.Content.ToString();

                // Close the popup after selection
                popupListBox.IsOpen = false;
            }

            string msg = "";
            if (string.IsNullOrEmpty(EmailTextBox.Text.Trim()) || string.IsNullOrEmpty(PhoneTextBox.Text.Trim())
                || string.IsNullOrEmpty(PassportTextBox.Text.Trim()) || string.IsNullOrEmpty(FullNameTextBox.Text.Trim())
                || string.IsNullOrEmpty(txtSelectedItem.Text))
            {
                MessageBox.Show("Please provide all the fields!!!");
                return true;
            }

            if (!Regex.IsMatch(EmailTextBox.Text.Trim(), @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                msg += "Email is not Valid!!!\n";
            }

            /*if (!Regex.IsMatch(PhoneTextBox.Text.Trim(), @"^[+]{1}(?:[0-9\-\(\)\/\.]\s?){6, 15}[0-9]{1}$"))
            {
                msg += "Phone Number is not Valid!!!\n";
            }*/

            if (string.IsNullOrEmpty(msg))
                return false;

            MessageBox.Show(msg);

            return true;
        }

        private void NavigateToNextPage(object sender, RoutedEventArgs e)
        {
            try
            {
                (int userId, int lockerId, bool isAllowedToProceed) result = ProceedWithUserSignup();

                if (!result.isAllowedToProceed)
                    return;

                // If lockerId is 0 and isAllowedToProceed is true then still it will proceed to signup.
                // Just to Update Initial Password!!!
                this.NavigationService.Navigate(new SignUpPart2(result.userId));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }


        private (int userId, int lockerId, bool isAllowedToProceed) ProceedWithUserSignup()
        {
            if (IsNotValidated())
                return (0, 0, false);

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_conString))
                {
                    connection.Open();

                    string query = "SELECT * FROM Users WHERE UPPER(Email) = UPPER(@Email) OR Phone = @Phone OR " +
                        "PassportNo = @PassportNo;";

                    using (MySqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        command.Parameters.AddWithValue("@Email", EmailTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@Phone", PhoneTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@PassportNo", PassportTextBox.Text.Trim());

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                reader.Close();

                                int lockerId = _sharedLibrary.IsLockerAvaiable(connection);
                                if (lockerId == 0)
                                {
                                    MessageBox.Show("Currently either no Locker is Available or others are in " +
                                        "Process to get acquired. Please try again later!!!");
                                    return (0, 0, false);
                                }

                                query = "INSERT INTO Users (FullName, Nationality, PassportNo, Phone, Email, Password) " +
                                    "VALUES (@FullName, @Nationality, @PassportNo, @Phone, @Email, @Password);" +
                                    "SELECT LAST_INSERT_ID();";

                                using (MySqlCommand insertCommand = new MySqlCommand(query, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@FullName", FullNameTextBox.Text.Trim());
                                    insertCommand.Parameters.AddWithValue("@Nationality", txtSelectedItem.Text.ToString());
                                    insertCommand.Parameters.AddWithValue("@PassportNo", PassportTextBox.Text.Trim());
                                    insertCommand.Parameters.AddWithValue("@Phone", PhoneTextBox.Text.Trim());
                                    insertCommand.Parameters.AddWithValue("@Email", EmailTextBox.Text.Trim().ToLower());
                                    insertCommand.Parameters.AddWithValue("@Password", "");

                                    object result = insertCommand.ExecuteScalar();

                                    if (result != null && result != DBNull.Value)
                                    {
                                        int userId = Convert.ToInt32(result);
                                        connection.Close();
                                        return (userId, lockerId, true);
                                    }
                                }
                            }
                            else
                            {
                                while (reader.Read())
                                {
                                    string? email = reader["Email"].ToString();
                                    string? phone = reader["Phone"].ToString();
                                    string? passportNo = reader["PassportNo"].ToString();
                                    string? password = reader["Password"].ToString();

                                    string message = "";

                                    if (string.Equals(email, EmailTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase)
                                        && password != string.Empty)
                                    {
                                        message += "Email Already Exists: " + email + "\n";
                                    }

                                    if (string.Equals(phone, PhoneTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase)
                                        && password != string.Empty)
                                    {
                                        message += "Phone Already Exists: " + phone + "\n";
                                    }

                                    if (!string.IsNullOrEmpty(message))
                                    {
                                        MessageBox.Show(message);
                                        return (0, 0, false);
                                    }

                                    int lockerId = _sharedLibrary.IsLockerAvaiable(connection);

                                    int userId = Convert.ToInt32(reader["Id"]);

                                    return (userId, lockerId, true);
                                }
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return (0, 0, false);
        }


        private void FullNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText.Visibility = Visibility.Collapsed;
        }

        private void FullNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
        }
        private void PassportTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText1.Visibility = Visibility.Collapsed;
        }

        private void PassportTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PassportTextBox.Text))
            {
                PlaceholderText1.Visibility = Visibility.Visible;
            }
        }
        private void PhoneTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText2.Visibility = Visibility.Collapsed;
        }

        private void PhoneTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                PlaceholderText2.Visibility = Visibility.Visible;
            }
        }
        private void EmailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText3.Visibility = Visibility.Collapsed;
        }

        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                PlaceholderText3.Visibility = Visibility.Visible;
            }
        }

        private void nationalityComboBox_Click(object sender, RoutedEventArgs e)
        {
            popupListBox.IsOpen = !popupListBox.IsOpen;
        }

        private void lstComboItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstComboItems.SelectedItem is Country selectedCountry)
            {
                txtSelectedItem.Text = selectedCountry.Name; // Update the ComboBox Button Text
                popupListBox.IsOpen = false; // Close the popup
            }
        }
    }
}
