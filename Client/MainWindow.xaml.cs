using Client.Net;
using Client.ViewModels;
using System;
using System.IO;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    //public partial class MainWindow : Window
    //{
    //    public Config? config;
    //    public string timeBoot;
    //    public string currentDir;
    //    private readonly NetworkService _net;
    //    private readonly long _userId;

    //    public MainWindow(NetworkService net, long userId)
    //    {
    //        InitializeComponent();
    //        _net = net;
    //        _userId = userId;
    //        DataContext = new MainViewModel(this);
    //        // DataContext is set in XAML to MainViewModel
    //        currentDir = Directory.GetCurrentDirectory();
    //        timeBoot = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
    //        config = Config.Read();
    //        Initialized += OnPostInitialize;
    //        Log.Info("MainWindow Initialized!");
    //    }

    //    public void OnPostInitialize(object? sender, EventArgs e)
    //    {
    //    }

    //}
    public partial class MainWindow : Window
    {
        private NetworkService _net;
        private long _userId;
        public Config? config;
        public string timeBoot;
        public string currentDir;

        public MainWindow()
        {
            InitializeComponent();  
        }

    
        public MainWindow(NetworkService net, long userId) : this()
        {
            _net = net ?? throw new ArgumentNullException(nameof(net));
            _userId = userId;
            DataContext = new MainViewModel(this);
            // DataContext is set in XAML to MainViewModel
            currentDir = Directory.GetCurrentDirectory();
            timeBoot = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
            config = Config.Read();
            Log.Info("MainWindow Initialized!");

        }

        private async Task LoadInitialDataAsync()
        {
            var users = await _net.GetUserListAsync();
            // … заполнить UI …
        }
    }
}
