using Client.Front.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.Front.Controls
{
    public partial class ChatTab : UserControl
    {
        public event EventHandler Clicked;

        public string Title = "Example";
        public string LastPreview = "Example Preview";
        public string Time = "12:00";
        public int Unread = 0;

        public string UnreadStr => Unread.ToString();
        public bool showUnreadMark => Unread > 0;

        public ChatTab()
        {
            InitializeComponent();
        }

        private void OpenChat_Click(object sender, RoutedEventArgs e)
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }
       
    }
}
