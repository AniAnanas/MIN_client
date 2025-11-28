using Client.Net;
using Client.ViewModels;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Client.Controls
{
    public partial class ChatUI : UserControl
    {
        public ChatUI()
        {
            InitializeComponent();
            DataContextChanged += ChatUI_DataContextChanged;
        }

        private void ChatUI_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Отписываемся от старого ViewModel
            if (e.OldValue is ChatViewModel oldViewModel)
            {
                oldViewModel.Messages.CollectionChanged -= Messages_CollectionChanged;
            }

            // Подписываемся на новый ViewModel
            if (e.NewValue is ChatViewModel newViewModel)
            {
                newViewModel.Messages.CollectionChanged += Messages_CollectionChanged;

                // Сразу скроллим к последнему сообщению
                ScrollToBottom();
            }
        }

        private void Messages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Автоматически скроллим к новому сообщению
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                ScrollToBottom();
            }
        }

        private void ScrollToBottom()
        {
            // Откладываем скроллинг, чтобы дать UI время на обновление
            Dispatcher.InvokeAsync(() =>
            {
                MessagesScrollViewer?.ScrollToEnd();
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        // Message input handlers
        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e) { }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                if (DataContext is ChatViewModel viewModel && viewModel.SendMessageCommand.CanExecute(null))
                {
                    viewModel.SendMessageCommand.Execute(null);
                }
            }
        }

        // File drop handler
        private void MessageInput_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    // TODO: Handle file drop
                    Log.Info($"File dropped: {files[0]}");
                }
            }
        }
        // Attach file button
        private void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open file picker
            Log.Info("Attach file clicked");
        }
    }
}
