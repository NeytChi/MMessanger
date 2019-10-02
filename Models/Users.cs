using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace miniMessanger.Models
{
    public partial class Users
    {
        public Users()
        {
            BlockedUsers = new HashSet<BlockedUsers>();
            UsersBlocks = new HashSet<BlockedUsers>();
            Complaints = new HashSet<Complaints>();
            Profile = new Profiles();
        }

        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserLogin { get; set; }
        public string UserPassword { get; set; }
        public int CreatedAt { get; set; }
        public string UserHash { get; set; }
        public sbyte? Activate { get; set; }
        public string UserToken { get; set; }
        public int? LastLoginAt { get; set; }
        public int? RecoveryCode { get; set; }
        public string RecoveryToken { get; set; }
        public string UserPublicToken { get; set; }
        public bool Deleted { get; set; }

        public virtual ICollection<BlockedUsers> BlockedUsers { get; set; }
        public virtual ICollection<BlockedUsers> UsersBlocks { get; set; }
        public virtual ICollection<Complaints> Complaints { get; set; }
        public virtual Participants Opposite { get; set; }
        public virtual Participants ChatSide { get; set; }
        
        public virtual Profiles Profile { get; set; }
    }
}
