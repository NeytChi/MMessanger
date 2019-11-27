namespace miniMessanger.Models
{
    public struct UserCache
    {
        public string user_token { get; set; }
        public int page { get; set; }
        public long message_id { get; set; }
        public string complaint { get; set; }
        public string opposide_public_token { get; set; }
        public string blocked_reason { get; set; }
        public string chat_token { get; set; }
        public string message_text { get; set; }
    }
}