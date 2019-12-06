namespace miniMessanger.Models
{
    public partial class Profile
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public string UrlPhoto { get; set; }
        public sbyte? ProfileAge { get; set; }
        public bool ProfileGender { get; set; }
        public string ProfileCity { get; set; }

        public virtual User User { get; set; }
    }
}
