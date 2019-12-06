namespace miniMessanger.Models
{
    public enum LogLevel 
    { 
        DEBUG, 
        INFO, 
        WARN, 
        ERROR, 
        FATAL,
        OFF,
        TRACE
    }

    public partial class LogMessage
    {
        public long log_id { get; set; }
        public string message { get; set; }
        public string user_computer { get; set; }
        public System.DateTime time { get; set; }
        public string level { get; set; }
        public long user_id { get; set; }
        public long thread_id { get; set; }
        public string user_ip { get; set; }
    }
}
