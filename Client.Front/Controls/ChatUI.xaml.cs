using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class ChatUI : UserControl, INotifyPropertyChanged
    {
        public ChatUI()
        {
            InitializeComponent();
        }
        private string _messageText = string.Empty;
        public string MessageText
        {
            get => _messageText;
            set
            {
                _messageText = value;
                OnPropertyChanged(nameof(MessageText));
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // я не знаю чёто спокойно въебать можем тут на будущее короче
            if (e.Handled) return;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled) return;
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    Input_Area.AppendText(Environment.NewLine);
                    return;
                } 
                else if (!string.IsNullOrWhiteSpace(MessageText))
                {
                    //типо отправкак тут будэ
                }
            }
        }

        private void Input_Area_Drop(object sender, DragEventArgs e)
        {
            //тут типо сюда файлы кидать через курсор братан(ну ты тип понял драг н дроп)0
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
