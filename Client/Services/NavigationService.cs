using Client.Services.Interfaces;
using Client.ViewModels;
using System.ComponentModel;

namespace Client.Services
{
    public class NavigationService : INavigationService
    {
        public void NavigateToChatList()
        {
            // Implementation for navigating to chat list
            Log.Info("Navigating to chat list");
        }

        public void NavigateToChat(ChatViewModel chatViewModel)
        {
            // Implementation for navigating to specific chat
            Log.Info($"Navigating to chat: {chatViewModel?.Title ?? "Unknown"}");
        }

        public void NavigateToSettings()
        {
            // Implementation for navigating to settings
            Log.Info("Navigating to settings");
        }

        public void NavigateToUserProfile()
        {
            // Implementation for navigating to user profile
            Log.Info("Navigating to user profile");
        }

        public void NavigateToChat(string chatId)
        {
            throw new NotImplementedException();
        }

        public void NavigateToAuth()
        {
            throw new NotImplementedException();
        }

        public void NavigateToLoading()
        {
            throw new NotImplementedException();
        }
    }
}
