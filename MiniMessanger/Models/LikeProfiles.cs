namespace miniMessanger.Models
{
    public partial class LikeProfiles
    {
        public LikeProfiles()
        {
            
        }
        public long LikeId { get; set; }
        public int UserId { get; set; }
        public int ToUserId { get; set; }
        public bool Like { get; set; }
        public bool Dislike { get; set; }
        public virtual User User { get; set; }
        public virtual User ToUser { get; set; }
    }
}
