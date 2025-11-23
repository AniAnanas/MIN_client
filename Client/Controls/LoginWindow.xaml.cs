using Client.Net;
using System.IO;
using System.Windows;

namespace Client.Controls
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly NetworkService _net;

        public LoginWindow(NetworkService net)
        {
            InitializeComponent();
            _net = net ?? throw new ArgumentNullException(nameof(net));
        }

        private void SaveToken(string token)
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MyApp");
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
                (long userId, string token) result;
                if (isRegister)
                    result = await _net.RegisterAsync(login, password);
                else
                    result = await _net.LoginAsync(login, password);

                SaveToken(result.token);

                var main = new MainWindow(_net, result.userId);
                main.Show();

                Close();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ShowError(string msg)
        {
            TbError.Text = msg;
            TbError.Visibility = Visibility.Visible;
        }
    }
}
