using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace miniMessanger.Models
{
    public partial class Messages
    {
        public Messages()
        {
            Complaints = new HashSet<Complaints>();
        }

        public long MessageId { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string MessageText { get; set; }
        public bool MessageViewed { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Complaints> Complaints { get; set; }
    }
}
