using com.bitmick.marshall.models;
using com.bitmick.marshall.protocol;
using com.bitmick.marshall.utils;
using MySql.Data.MySqlClient;
using NayaxAPI;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheBagBunker.Helper;
using TheBagBunker.Model;
using static com.bitmick.marshall.protocol.vmc_socket_t;

namespace TheBagBunker.Views
{
    /// <summary>
    /// Interaction logic for AmountPage.xaml
    /// </summary>
    public partial class AmountPage : Page, INotifyPropertyChanged
    {
        private int lockerId = 0;
        private int userId = 0;
        private bool isPaymentSucceded = false;
        private Timer _timer;
        private DateTime _startTime = DateTime.MinValue;
        private SharedLibrary _sharedLibrary = new SharedLibrary();
        private int _timeToProceed;
        private int _timeSpentOnPreviousPage;
        private bool _isForLogin = false;
        private string _merchantId = "";
        private string _hashCode = "";
        private readonly NayaxAdapter _nayaxAdapter;
        private decimal _AEDExchangeRate = 0.27M;

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly LocalizationHelper _localizationHelper;

        public string PageTitle => _localizationHelper.GetTranslation("amountPageTitle", CLanguage.currentLanguage);
        public string Des => _localizationHelper.GetTranslation("amountPageDes", CLanguage.currentLanguage);
        public string ButtonS => _localizationHelper.GetTranslation("amountPageButton", CLanguage.currentLanguage);

        public AmountPage(int lockerId, int userId, int timeSpentOnPreviousPage)
        {
            InitializeComponent();
            this.lockerId = lockerId;
            this.userId = userId;
            this.Loaded += LoginPage_Loaded;
            _timer = new Timer(DoTask, null, Timeout.Infinite, Timeout.Infinite);
            _timeSpentOnPreviousPage = timeSpentOnPreviousPage;
            _timeToProceed = timeSpentOnPreviousPage >= SharedLibrary._lockIdleInterval ? 0 :
                SharedLibrary._lockIdleInterval - timeSpentOnPreviousPage;
            _isForLogin = false;
            _nayaxAdapter = new NayaxAdapter(_merchantId, _hashCode);
            _localizationHelper = new LocalizationHelper("translations.json");
            DataContext = this;

        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AmountPage(int lockerId, int userId)
        {
            InitializeComponent();
            this.lockerId = lockerId;
            this.userId = userId;
            _timer = new Timer(DoTask, null, Timeout.Infinite, Timeout.Infinite);
            _isForLogin = true;
            _nayaxAdapter = new NayaxAdapter(_merchantId, _hashCode);
            _localizationHelper = new LocalizationHelper("translations.json");
            DataContext = this;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isForLogin)
                return;

            if (_timeToProceed == 0)
            {
                DoInnerWork();
            }
            // Defer the MessageBox display until the rendering is complete
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _startTime = DateTime.UtcNow;
                _timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
                MessageBox.Show($"You have spent {_timeSpentOnPreviousPage} on Terms Page. " +
                                $"Now, after closing this message. " +
                                $"You just have {_timeToProceed} " +
                                $"seconds to proceed with Payment. " +
                                $"Otherwise your reserved locker will be released, and you will have to " +
                                $"login to proceed with Payment and get Locker assigned!!!");
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void DoTask(object? state)
        {
            if (((DateTime.UtcNow - _startTime).TotalSeconds > _timeToProceed) || _timeToProceed == 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DoInnerWork();
                });

                return;
            }
        }

        private void DoInnerWork()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _sharedLibrary.ReleaseLockOnLocker(lockerId);
            ForceLogin();
            MessageBox.Show("Time Up. You didn't proceed with Payment on Time. " +
                "Your locker has been Released. Please login to continue!!!");
        }

        private void ForceLogin()
        {
            try
            {
                if (this.NavigationService != null)
                {
                    this.NavigationService.Navigate(new loginPage());
                }
                else
                {
                    MessageBox.Show("NavigationService is not available. Login page navigation failed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to navigate to login page: " + ex.Message);
            }
        }

        private void NavigateToNextPage(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_isForLogin)
            {
                HandleLockerUpTimeOnLogin();
                return;
            }

            var transactionDetails = new Dictionary<string, string>
            {
                {"amount", (30 * _AEDExchangeRate).ToString()},
                {"currency", "USD"},
                {"orderId", "123444"},
                {"methodCode", "visa"},
                {"redirectUrl", "https://www.google.com/"},
                {"notificationUrl", "https://www.google.com/"}
            };
            string result = _nayaxAdapter.InitiatePayment(transactionDetails);

            isPaymentSucceded = true;
            if (isPaymentSucceded)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                AssignLocker();
                this.NavigationService.Navigate(new successPayment(lockerId, userId));
            }
        }

        private void HandleLockerUpTimeOnLogin()
        {
            var transactionDetails = new Dictionary<string, string>
            {
                {"amount", (30 * _AEDExchangeRate).ToString()},
                {"currency", "USD"},
                {"orderId", "123444"},
                {"methodCode", "visa"},
                {"redirectUrl", "https://www.google.com/"},
                {"notificationUrl", "https://www.google.com/"}
            };
            string result = _nayaxAdapter.InitiatePayment(transactionDetails);

            isPaymentSucceded = true;
            if (isPaymentSucceded)
            {
                bool signal = _sharedLibrary.FreeTheLockerAndUser(userId, lockerId, 30, "AED");
                if (signal)
                    this.NavigationService.Navigate(new lastPage());
                //this.NavigationService.Navigate(new successPayment());
            }
        }

        private void AssignLocker()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(SharedLibrary._ConnectionString))
                {
                    conn.Open();

                    string query = "UPDATE Lockers SET IsLockerInUse = 1, IsLockerInProcess = 0, " +
                        "LockerInProcessTime = NULL WHERE Id = @LockerId;" +
                        "INSERT INTO LockerInformation (UserId, LockerId, LockerUpTime, " +
                        "IsPaymentDone, Amount, Currency) " +
                        "VALUES (@UserId, @LockerId, @LockerUpTime, @IsPaymentDone, @Amount, @Currency);" +
                        "DELETE FROM TimeReservedLocks WHERE LockerId = @LockerId;";

                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@LockerId", lockerId);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@LockerUpTime", DateTime.UtcNow);
                        cmd.Parameters.AddWithValue("@IsPaymentDone", true);
                        cmd.Parameters.AddWithValue("@Amount", 30);
                        cmd.Parameters.AddWithValue("@Currency", "AED");
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("You have been assigned the Locker!!!");
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void PaymentButton_Click(object sender, RoutedEventArgs e)
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
