using Client.Data;
using Client.Net;
using Client.Shared.Interfaces;
using Client.ViewModels;
using System;
using System.IO;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private INetworkService _net;
        private DatabaseService _db;

        private long _userId;
        private string _token = string.Empty;

        public Config? config;

        public MainWindow()
        {
            InitializeComponent();  
        }

    
        public MainWindow(long userId) : this()
        {
            _net = App.Current.Resources["Net"] as INetworkService 
                    ?? throw new NullReferenceException("App.Current.Resources[\"Net\"]");
            _userId = userId;
            config = App.Current.Resources["Config"] as Config
                    ?? throw new NullReferenceException("App.Current.Resources[\"Config\"]");
            _db = App.Current.Resources["Database"] as DatabaseService
                    ?? throw new NullReferenceException("App.Current.Resources[\"Database\"]");
            
            _db.SetCurrentUser(_userId);
            

            DataContext = new MainViewModel(this);

            Loaded += async (s, e) => await LoadInitialDataAsync();

            Log.Info("MainWindow Initialized!");
        }
        private async Task LoadInitialDataAsync()
        {
            try
            {
                Log.Info("Loading initial data...");

                // Получаем список пользователей с сервера
                var users = await _net.GetUserListAsync();
                Log.Info($"Received {users.Length} users from server");

                // Сохраняем пользователей в БД
                foreach (var user in users)
                {
                    _db.SaveOrUpdateUser(user.Id, user.Username, user.Fullname, user.IsOnline);
                }

                // Уведомляем ViewModel о загрузке данных
                if (DataContext is MainViewModel vm)
                {
                    await vm.InitializeDataAsync(users);
                }

                Log.Success("Initial data loaded successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load initial data: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Task.WaitAll(_net.Disconnect(false));
            _db?.Dispose();
        }
    }
}
