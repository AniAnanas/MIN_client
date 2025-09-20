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
    /// Логика взаимодействия для AddButtonUI.xaml
    /// </summary>
    public partial class AddButtonUI : UserControl
    {
        public event EventHandler Clicked;
        public AddButtonUI()
        {
            InitializeComponent();
        }
        private void AddPerson_Click(object sender, RoutedEventArgs e)
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
