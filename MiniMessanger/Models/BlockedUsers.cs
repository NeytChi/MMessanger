namespace miniMessanger.Models
{
    public partial class BlockedUser
    {
        public BlockedUser()
        {
            
        }
        public int BlockedId { get; set; }
        public int UserId { get; set; }
        public int BlockedUserId { get; set; }
        public string BlockedReason { get; set; }
        public bool BlockedDeleted { get; set; }
        public virtual User Blocked { get; set; }
        public virtual User User { get; set; }
        public virtual Complaints Complaints { get; set; }
    }
}
