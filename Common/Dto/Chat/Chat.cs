using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Dto;

namespace Common.Dto.Chat
{
    public class UserRequest
    {
        public string Message { get; set; }
        public List<ChatMessage> History { get; set; } = new();
        public ZimmerDto? ZimmerDetails { get; set; }
    }

    public class ChatMessage
    {
        public string Role { get; set; }   
        public string Text { get; set; }
    }
}
