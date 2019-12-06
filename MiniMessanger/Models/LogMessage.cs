using System;

namespace miniMessanger.Models
{
    public partial class LogMessage
    {
        public long logId { get; set; }
        public string message { get; set; }
        public string userComputer { get; set; }
        public DateTime time { get; set; }
        public string level { get; set; }
        public long userId { get; set; }
        public long threadId { get; set; }
        public string userIp { get; set; }
    }
}
