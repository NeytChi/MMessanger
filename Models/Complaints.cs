﻿using System;
using System.Collections.Generic;

namespace miniMessanger.Models
{
    public partial class Complaints
    {
        public int ComplaintId { get; set; }
        public int UserId { get; set; }
        public int BlockedId { get; set; }
        public long MessageId { get; set; }
        public string Complaint { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual BlockedUsers Blocked { get; set; }
        public virtual Messages Message { get; set; }
        public virtual Users User { get; set; }
    }
}
