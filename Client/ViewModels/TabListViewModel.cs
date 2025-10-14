using Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Client.ViewModels
{
    public class TabListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TabViewModel> Tabs { get; } = [];
        private TabViewModel? _selectedTab;
        public TabViewModel? SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    RaisePropertyChanged(nameof(SelectedTab));
                }
            }
        }

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
