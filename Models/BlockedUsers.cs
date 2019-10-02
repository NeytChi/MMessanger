using System;
using System.Collections.Generic;

namespace miniMessanger.Models
{
    public partial class BlockedUsers
    {
        public BlockedUsers()
        {
            
        }

        public int BlockedId { get; set; }
        public int UserId { get; set; }
        public int BlockedUserId { get; set; }
        public string BlockedReason { get; set; }
        public bool BlockedDeleted { get; set; }

        public virtual Users BlockedUser { get; set; }
        public virtual Users User { get; set; }
        public virtual Complaints Complaints { get; set; }
    }
}
