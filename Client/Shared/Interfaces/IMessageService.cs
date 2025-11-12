using Client.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Client.Shared.Interfaces
{
    public interface IMessageService
    {
        Task<ObservableCollection<MessageModel>> GetMessagesAsync(string chatId, int offset = 0, int limit = 50);
        Task SendMessageAsync(string chatId, MessageModel message);
        Task DeleteMessageAsync(string chatId, int messageId);
        Task EditMessageAsync(string chatId, int messageId, string newText);
        Task MarkAsReadAsync(string chatId, int messageId);
    }
}
