using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace miniMessanger.Models
{
    public partial class Users
    {
        public Users()
        {
            BlockedUsersBlockedUser = new HashSet<BlockedUsers>();
            BlockedUsersUser = new HashSet<BlockedUsers>();
            Complaints = new HashSet<Complaints>();
            Profiles = new HashSet<Profiles>();
        }

        public int UserId { get; set; }
        [Required]
        public string UserEmail { get; set; }
        [Required]
        public string UserLogin { get; set; }
        [Required]
        public string UserPassword { get; set; }
        public int? CreatedAt { get; set; }
        public string UserHash { get; set; }
        public sbyte? Activate { get; set; }
        public string UserType { get; set; }
        public string UserToken { get; set; }
        public int? LastLoginAt { get; set; }
        public int? RecoveryCode { get; set; }
        public string RecoveryToken { get; set; }
        public string UserPublicToken { get; set; }
        public bool Deleted { get; set; }

        public virtual ICollection<BlockedUsers> BlockedUsersBlockedUser { get; set; }
        public virtual ICollection<BlockedUsers> BlockedUsersUser { get; set; }
        public virtual ICollection<Complaints> Complaints { get; set; }
        public virtual ICollection<Profiles> Profiles { get; set; }
    }
}
