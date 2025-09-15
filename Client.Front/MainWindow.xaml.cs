using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.Front
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Config? config;
        public string timeBoot;
        public string currentDir;
        public static MainWindow? Instance;
        public MainWindow()
        {
            InitializeComponent();
            
            Instance = this;
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