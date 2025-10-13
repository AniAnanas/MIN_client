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

            // DataContext is set in XAML to MainViewModel
            currentDir = Directory.GetCurrentDirectory();
            timeBoot = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
            config = Config.Read();
            Initialized += OnPostInitialize;
            Log.Info("MainWindow Initialized!");
        }

        public void OnPostInitialize(object? sender, EventArgs e)
        {
            // Post-initialization logic can be added here
        }

        // Window events are now handled by ViewModel commands
        // No need for manual event handlers - everything is bound in XAML
    }
}
