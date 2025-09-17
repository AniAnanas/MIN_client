using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Логика взаимодействия для ChatTab.xaml
    /// </summary>
    public partial class ChatTab : UserControl
    {
        public string Title = "Example";
        public string LastPreview = "Example Preview";
        public string Time = "12:00";
        public int Unread = 0;
        public bool showUnreadMark => Unread > 0;

        public ChatTab()
        {
            InitializeComponent();
        }
    }
}
