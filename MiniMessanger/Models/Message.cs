using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace miniMessanger.Models
{
    /// <summary>
    /// This class need to contains chat's messages.
    /// Message type can be: "text" and "photo". 
    /// </summary>
    public partial class Message
    {
        public long MessageId { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string MessageType { get; set; }
        public string MessageText { get; set; }
        public string UrlFile { get; set; }
        public bool MessageViewed { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

    }
}
