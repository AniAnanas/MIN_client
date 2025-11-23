using Client.Controls;
using Client.Net;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var net = new NetworkService();

            var loginWnd = new LoginWindow(net);
            loginWnd.Show();
        }
    }

}
