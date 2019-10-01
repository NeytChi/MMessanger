using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace miniMessanger.Models
{
    public partial class Chatroom
    {
        public int ChatId { get; set; }
        public string ChatToken { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public List<dynamic> users;
    }
}
