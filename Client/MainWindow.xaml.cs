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
        public Config? config;
        public string timeBoot;
        public string currentDir;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(this);
            // DataContext is set in XAML to MainViewModel
            currentDir = Directory.GetCurrentDirectory();
            timeBoot = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
            config = Config.Read();
            Initialized += OnPostInitialize;
            Log.Info("MainWindow Initialized!");
        }

        public void OnPostInitialize(object? sender, EventArgs e)
        {
        }

    }
}
