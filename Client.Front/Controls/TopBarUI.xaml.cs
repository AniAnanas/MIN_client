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
    /// Логика взаимодействия для TopBarUI.xaml
    /// </summary>
    public partial class TopBarUI : UserControl
    {
        public TopBarUI()
        {
            InitializeComponent();
            DataContext = new ViewModels.TobBar();
        }

        // Свойство для заголовка
        public string WindowTitle
        {
            get { return (string)GetValue(WindowTitleProperty); }
            set { SetValue(WindowTitleProperty, value); }
        }

        public static readonly DependencyProperty WindowTitleProperty =
            DependencyProperty.Register("WindowTitle", typeof(string),
                typeof(TopBarUI), new PropertyMetadata("My App"));

        // События для кнопок
        public event EventHandler MinimizeClicked;
        public event EventHandler MaximizeClicked;
        public event EventHandler CloseClicked;

        // Перетаскивание окна
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    // Двойной клик - максимизация
                    MaximizeClicked?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    // Перетаскивание окна
                    var window = Window.GetWindow(this);
                    window?.DragMove();
                }
            }
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            MinimizeClicked?.Invoke(this, EventArgs.Empty);
        }

        private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
        {
            MaximizeClicked?.Invoke(this, EventArgs.Empty);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseClicked?.Invoke(this, EventArgs.Empty);
            
        }
    }
}
