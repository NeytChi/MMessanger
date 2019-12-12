namespace miniMessanger.Models
{
    public struct UserCache
    {
        public string user_login;
        public string user_email;
        public string user_password;
        public string user_confirm_password;
        public string user_token;
        public int recovery_code;
        public string recovery_token;
        public int page;
        public int count;
        public long message_id;
        public string complaint;
        public string opposide_public_token;
        public string blocked_reason;

    }
}