using Client.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls
{
    public partial class TabUI : UserControl
    {
        public TabUI()
        {
            InitializeComponent();
            // DataContext will be set by parent or binding
        }

        // Dependency property for TabViewModel
        public TabViewModel ViewModel
        {
            get { return (TabViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(TabViewModel),
                typeof(TabUI), new PropertyMetadata(null, OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TabUI;
            if (control != null)
            {
                control.DataContext = e.NewValue;
            }
        }

        // Tab click handler
        private void Tab_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && ViewModel != null)
            {
                // Notify parent to select this tab
                var parent = System.Windows.Media.VisualTreeHelper.GetParent(this) as FrameworkElement;
                if (parent != null)
                {
                    // Find the parent ListBox
                    var listBox = FindParentListBox(parent);
                    if (listBox != null)
                    {
                        listBox.SelectedItem = this.DataContext;
                    }
                }
            }
        }

        private ListBox? FindParentListBox(FrameworkElement element)
        {
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(element) as FrameworkElement;
            while (parent != null)
            {
                if (parent is ListBox listBox)
                {
                    return listBox;
                }
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent) as FrameworkElement;
            }
            return null;
        }

        // Context menu handlers
        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement tab closing logic
            Log.Info($"Close tab requested for: {ViewModel?.Title}");
        }

        private void MuteTab_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement tab muting logic
            Log.Info($"Mute tab requested for: {ViewModel?.Title}");
        }

        private void PinTab_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement tab pinning logic
            Log.Info($"Pin tab requested for: {ViewModel?.Title}");
        }
    }
}
