using MySql.Data.MySqlClient;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for SignUpPart1.xaml
    /// </summary>

    public partial class SignUpPart1 : Page
    {
        private string _conString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
        private SharedLibrary _sharedLibrary = new SharedLibrary();
        public SignUpPart1()
        {
            InitializeComponent();
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
                                        MessageBox.Show("Data Inserted Successfully!!!");
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
            if (lstComboItems.SelectedItem != null)
            {
                txtSelectedItem.Text = ((ListBoxItem)lstComboItems.SelectedItem).Content.ToString();
                popupListBox.IsOpen = false;
            }
        }

    }
}
