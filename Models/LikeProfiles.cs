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
        public virtual Users User { get; set; }
        public virtual Users ToUser { get; set; }
    }
}
