using System;
using System.Collections.Generic;

namespace miniMessanger.Models
{
    public partial class Chatroom
    {
        public int ChatId { get; set; }
        public string ChatToken { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<dynamic> users;
    }
}
