using Client.Controls;
using Client.Data;
using Client.Net;
using Client.Services;
using Client.Shared.Interfaces;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    
    public partial class App : Application
    {
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        public bool DebugMode => Convert.ToBoolean(Current.Resources["IsDebug"] ?? false);
        public INetworkService? Net => Current.Resources["Net"] as INetworkService;
        public Config? GetConfig() => Current.Resources["Config"] as Config;

        public DateTime StartTime { get; } = DateTime.Now;
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SQLitePCL.Batteries_V2.Init();
            Current.Resources["IsDebug"] = true;
            try
            {
                Log.Initialize(new LoggingService());

                AllocConsole();

                var _net = new NetworkService();
                Current.Resources["Net"] = _net;
                var _config = Config.Read();
                Current.Resources["Config"] = _config;

                string dbPath = Path.Combine(Environment.CurrentDirectory, _config.DatabasePath);
                try { Directory.CreateDirectory(Directory.GetParent(dbPath)?.FullName); }
                catch { }
                DatabaseService _db = new DatabaseService(dbPath);
                App.Current.Resources["Database"] = _db;

                Log.Info("Connecting to server...");
                bool connected = await _net.ConnectAsync(_config.ServerHost, _config.ServerPort);
                if (!connected)
                {
                    Log.Error("Failed to connect to server.");
                    MessageBox.Show("Не удалось подключиться к серверу", "Ошибка",
                        MessageBoxButton.YesNo, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }
                Log.Success("Connected to server");

                string? token = LoadToken();
                if (!string.IsNullOrEmpty(token))
                {
                    Log.Info("Token found, attempting authorization...");

                    try
                    {
                        long userId = await _net.AuthorizeAsync(token);

                        if (userId > 0)
                        {
                            Log.Success($"Authorization successful, userId: {userId}");

                            var mainWindow = new MainWindow(userId);
                            mainWindow.Show();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"Token authorization failed: {ex.Message}");
                        DeleteToken();
                    }
                }

                // Токена нет или он невалидный - открываем окно входа
                Log.Info("Opening login window");
                var loginWindow = new LoginWindow();
                loginWindow.Show();

            }
            catch (Exception ex)
            {
                Log.Error($"Startup error: {ex.Message}");
                MessageBox.Show($"Ошибка запуска приложения: {ex.Message}",
                    "Ошибка", MessageBoxButton.YesNo, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private string? LoadToken()
        {
            try
            {
                var tokenPath = GetTokenPath();
                if (File.Exists(tokenPath))
                {
                    return File.ReadAllText(tokenPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading token: {ex.Message}");
            }
            return null;
        }

        private void DeleteToken()
        {
            try
            {
                var tokenPath = GetTokenPath();
                if (File.Exists(tokenPath))
                {
                    File.Delete(tokenPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error deleting token: {ex.Message}");
            }
        }

        private string GetTokenPath()
        {
            var dir = Path.Combine(Environment.CurrentDirectory, "Data");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "token.txt");
        }
    }
}
