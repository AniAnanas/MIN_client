using Client.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls
{
    /// <summary>
    /// Top bar control with window management and navigation
    /// </summary>
    public partial class TopBarUI : UserControl
    {
        public TopBarUI()
        {
            InitializeComponent();
            // DataContext will be set by parent or binding
        }
        private void MinimizeBtn_Click(object sender, RoutedEventArgs e) 
        { 
            var window = Window.GetWindow(this);
            if(window != null) window.WindowState = WindowState.Minimized;
        }
        private void MaximizeBtn_Click(object sender, RoutedEventArgs e) 
        {
            var window = Window.GetWindow(this);
            if (window == null) return;

            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        private void CloseBtn_Click(object sender, RoutedEventArgs e) 
        {
            var window = Window.GetWindow(this);
            if (window != null) window.Close();
            else Application.Current.Shutdown();
        }

        // Dependency property for TopBarViewModel
        public TopBarViewModel ViewModel
        {
            get { return (TopBarViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(TopBarViewModel),
                typeof(TopBarUI), new PropertyMetadata(null, OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TopBarUI;
            if (control != null)
            {
                control.DataContext = e.NewValue;
            }
        }

        // Window title dependency property
        public string WindowTitle
        {
            get { return (string)GetValue(WindowTitleProperty); }
            set { SetValue(WindowTitleProperty, value); }
        }

        public static readonly DependencyProperty WindowTitleProperty =
            DependencyProperty.Register("WindowTitle", typeof(string),
                typeof(TopBarUI), new PropertyMetadata("Messenger"));

        // Window state dependency property
        public WindowState CurrentWindowState
        {
            get { return (WindowState)GetValue(CurrentWindowStateProperty); }
            set { SetValue(CurrentWindowStateProperty, value); }
        }

        public static readonly DependencyProperty CurrentWindowStateProperty =
            DependencyProperty.Register("CurrentWindowState", typeof(WindowState),
                typeof(TopBarUI), new PropertyMetadata(WindowState.Normal));

        // Drag move functionality
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    // Double click - toggle maximize
                    if (ViewModel?.MaximizeCommand?.CanExecute(null) == true)
                    {
                        ViewModel.MaximizeCommand.Execute(null);
                    }
                }
                else
                {
                    // Drag window
                    var window = Window.GetWindow(this);
                    window?.DragMove();
                }
            }
        }
    }
}
