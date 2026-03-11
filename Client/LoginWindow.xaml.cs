using Client.Data;
using Client.Net;
using Client.Shared.Interfaces;
using System.IO;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly INetworkService _net;
        private readonly DatabaseService _db;


        public LoginWindow()
        {
            InitializeComponent();

            _net = App.Current.Resources["Net"] as INetworkService
                    ?? throw new NullReferenceException("App.Current.Resources[\"Net\"]");
            _db = App.Current.Resources["Database"] as DatabaseService
                    ?? throw new NullReferenceException("App.Current.Resources[\"Database\"]");
            string username = _db.GetSetting("CurrentUsername") ?? string.Empty;
            TbLogin.Text = username;
        }
        private void SaveToken(string token)
        {
            var dir = Path.Combine(Environment.CurrentDirectory, "Data");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "token.txt"), token);
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            await PerformAuthAsync(isRegister: false);
        }

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            await PerformAuthAsync(isRegister: true);
        }

        private async Task PerformAuthAsync(bool isRegister)
        {
            TbError.Visibility = Visibility.Collapsed;

            string login = TbLogin.Text.Trim();
            string password = PbPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль.");
                return;
            }

            try
            {
                BtnLogin.IsEnabled = BtnRegister.IsEnabled = false;

                (long userId, string token) result;

                if (isRegister)
                {
                    Log.Info($"Registering user: {login}");
                    result = await _net.RegisterAsync(login, password);
                    Log.Success($"Registration successful, userId: {result.userId}");
                }
                else
                {
                    Log.Info($"Logging in user: {login}");
                    result = await _net.LoginAsync(login, password);
                    Log.Success($"Login successful, userId: {result.userId}");
                }

                _db.SaveSetting("CurrentUsername", _net.CurrentUser.Username = _net.CurrentUser.Name = login);
                SaveToken(result.token);

                var main = new MainWindow(result.userId);
                main.Show();

                Close();
            }
            catch (Exception ex)
            {
                Log.Error($"Authentication failed: {ex.Message}");
                ShowError(ex.Message);
                BtnLogin.IsEnabled = true;
                BtnRegister.IsEnabled = true;
            }
        }

        private void ShowError(string msg)
        {
            TbError.Text = msg;
            TbError.Visibility = Visibility.Visible;
        }
    }
}
