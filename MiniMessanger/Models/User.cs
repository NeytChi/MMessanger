using System.Collections.Generic;

namespace miniMessanger.Models
{
    public partial class User
    {
        public User()
        {
            Blocks = new HashSet<BlockedUser>();
            Complaints = new HashSet<Complaint>();
            Profile = new Profile();
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
        public string ProfileToken { get; set; }
        public bool Deleted { get; set; }
        public virtual Profile Profile { get; set; }
        public virtual ICollection<BlockedUser> Blocks { get; set; }
        public virtual ICollection<Complaint> Complaints { get; set; }
    }
}
