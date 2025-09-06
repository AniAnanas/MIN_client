using System.Collections.ObjectModel;

namespace Client.Front.Models
{
    public class Chat
    {
        public string Title { get; set; }
        public string LastPreview { get; set; }
        public string Status { get; set; }
        public ObservableCollection<Message> Messages { get; set; } = new();
    }
}
