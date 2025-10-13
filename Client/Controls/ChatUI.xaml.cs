using Client.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls
{
    public partial class ChatUI : UserControl
    {
        public ChatUI()
        {
            InitializeComponent();
            // DataContext will be set by parent or binding
        }

        // Dependency property for ChatViewModel
        public ChatViewModel ViewModel
        {
            get { return (ChatViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ChatViewModel),
                typeof(ChatUI), new PropertyMetadata(null, OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ChatUI;
            if (control != null)
            {
                control.DataContext = e.NewValue;
            }
        }

        // Message input text property
        public string MessageText
        {
            get { return (string)GetValue(MessageTextProperty); }
            set { SetValue(MessageTextProperty, value); }
        }

        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.Register("MessageText", typeof(string),
                typeof(ChatUI), new PropertyMetadata(string.Empty, OnMessageTextChanged));

        private static void OnMessageTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ChatUI;
            if (control?.ViewModel != null)
            {
                // Update ViewModel draft if needed
                // control.ViewModel.Draft = e.NewValue as string ?? string.Empty;
            }
        }

        // Message input handlers
        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.Handled) return;

            var textBox = sender as TextBox;
            if (textBox != null)
            {
                MessageText = textBox.Text;
            }
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled) return;

            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    // Insert new line
                    var textBox = sender as TextBox;
                    if (textBox != null)
                    {
                        var caretIndex = textBox.CaretIndex;
                        textBox.Text = textBox.Text.Insert(caretIndex, Environment.NewLine);
                        textBox.CaretIndex = caretIndex + Environment.NewLine.Length;
                    }
                    e.Handled = true;
                }
                else if (!string.IsNullOrWhiteSpace(MessageText))
                {
                    // Send message
                    SendMessage();
                    e.Handled = true;
                }
            }
        }

        private void SendMessage()
        {
            if (ViewModel != null && !string.IsNullOrWhiteSpace(MessageText))
            {
                // TODO: Implement message sending through ViewModel
                Log.Info($"Sending message: {MessageText}");
                MessageText = string.Empty;
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

        // Send button click
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        // Attach file button
        private void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open file picker
            Log.Info("Attach file clicked");
        }

        // Emoji button
        private void EmojiButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Show emoji picker
            Log.Info("Emoji picker clicked");
        }
    }
}
