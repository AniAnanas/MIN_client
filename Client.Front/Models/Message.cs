using System;

namespace Client.Front.Models
{
    public class Message
    {
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
