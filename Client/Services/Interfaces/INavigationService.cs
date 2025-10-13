using Client.ViewModels;

namespace Client.Services.Interfaces
{
    public interface INavigationService
    {
        void NavigateToChatList();
        void NavigateToChat(string chatId);
        void NavigateToSettings();
        void NavigateToUserProfile();
        void NavigateToAuth();
        void NavigateToLoading();
    }
}
