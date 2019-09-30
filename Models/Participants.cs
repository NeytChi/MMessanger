using System;
using System.Collections.Generic;

namespace miniMessanger.Models
{
    public partial class Participants
    {
        public long ParticipantId { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public int OpposideId { get; set; }
    }
}
