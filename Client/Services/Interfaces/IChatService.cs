using Client.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Client.Services.Interfaces
{
    public interface IChatService
    {
        Task<ObservableCollection<ChatModel>> GetChatsAsync();
        Task<ChatModel?> GetChatByIdAsync(string id);
        Task CreateChatAsync(ChatModel chat);
        Task UpdateChatAsync(ChatModel chat);
        Task DeleteChatAsync(string id);
        Task SendMessageAsync(string chatId, MessageModel message);
        Task<ObservableCollection<MessageModel>> GetMessagesAsync(string chatId, int offset = 0, int limit = 50);
    }
}
