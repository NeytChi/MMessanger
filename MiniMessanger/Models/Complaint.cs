using System;

namespace miniMessanger.Models
{
    public partial class Complaint
    {
        public int ComplaintId { get; set; }
        public int UserId { get; set; }
        public int BlockId { get; set; }
        public long MessageId { get; set; }
        public string ComplaintText { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual BlockedUser Blocked { get; set; }
        public virtual Message Message { get; set; }
        public virtual User User { get; set; }
    }
}
