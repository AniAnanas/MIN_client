using System.Collections.ObjectModel;

namespace Client.Front.Models
{
    public class ChatTab
    {
        public string Title { get; set; } = string.Empty;
        public string LastPreview { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LastOnLineTime { get; set; } 
        public int UnreadMessagesAmount { get => Messages.Count;  }
        public ObservableCollection<Message> Messages { get; set; } = new();
    }
}
