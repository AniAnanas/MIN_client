using Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public class ChatsListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TabViewModel> Tabs { get; } = new ObservableCollection<TabViewModel>();

        // При поступлении новых ChatModel:
        public void AddChat(ChatModel chat)
        {
            var tab = new TabViewModel(chat);
            Tabs.Add(tab);
        }

        // При удалении:
        public void RemoveChat(TabViewModel tab)
        {
            tab.Dispose();
            Tabs.Remove(tab);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
