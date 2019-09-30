using miniMessanger.Models;

namespace Common
{
    public static class Log
    {
        public static LogLevel Logging = LogLevel.DEBUG;
        private static string PathLogs = System.IO.Directory.GetCurrentDirectory() + "/logs/";
        private static string FileName = System.DateTime.Now.Day + "-" + System.DateTime.Now.Month + "-" + System.DateTime.Now.Year;
        private static string FullPathLog = PathLogs + FileName;
        private static System.DateTime CurrentFileDate = System.DateTime.Now;
        private static string UserComputer = System.Environment.UserName + "-" + System.Environment.MachineName;
        private static System.IO.StreamWriter Writer;
        public static Common.LogContext context = new LogContext();
        
        public static void WriteLogMessage(LogMessage log)
        {
            if (Logging != LogLevel.OFF)
            {
                if (!string.IsNullOrEmpty(log.message))
                {
                    if (log.message.Length > 2000)
                    {
                        log.message = log.message.Substring(0, 2000);
                    }
                }
                else
                {
                    Error("Log->message - is null or emply, call WriteLogMessage()");
                    return;
                }
                log.user_computer = UserComputer;
                log.time = System.DateTime.Now;
                log.thread_id = System.Threading.Thread.CurrentThread.ManagedThreadId;
                ChangeLogFile(log.time);
                System.Diagnostics.Debug.WriteLine(log.message); 
                Writer.WriteAsync
                (
                    "Time: " + log.time + " | " +
                    log.level + " | " +
                    "Message: " + log.message  + " | " + 
                    "UID: " + log.user_id + " | " +
                    "UIP: " + log.user_ip + " | " +
                    "TID: " + log.thread_id + " | " +
                    "UPC: " + log.user_computer + " | " +
                    "\r\n"
                );
                Writer.Flush();
                context.Logs.AddAsync(log);
                context.SaveChangesAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(log.message);
            }   
        }
        private static void ChangeLogFile(System.DateTime Local)
        {
            if (!System.IO.Directory.Exists(PathLogs))
            {
                System.IO.Directory.CreateDirectory(PathLogs);
            }
            if (!System.IO.File.Exists(FullPathLog) || Local.Day != CurrentFileDate.Day || Writer == null)
            {
                if (Writer != null)
                {
                    Writer.Dispose();
                }
                CurrentFileDate = Local;
                FileName = CurrentFileDate.Day + "-" + CurrentFileDate.Month + "-" + CurrentFileDate.Year;
                FullPathLog = PathLogs + FileName;
                if (System.IO.File.Exists(FullPathLog))
                {
                    Writer = new System.IO.StreamWriter(FullPathLog, true);
                }
                else
                {
                    Writer = System.IO.File.CreateText(FullPathLog);
                }
            }
        }
        public static void Trace(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "trace",
            };
            WriteLogMessage(log);
        }
        public static void Debug(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "debug",
            };
            WriteLogMessage(log);
        }
        public static void Info(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "info"
            };
            WriteLogMessage(log);
        }
        public static void Info(string message, string ip)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "info",
                user_ip = ip
            };
            WriteLogMessage(log);
        }
        public static void Info(string message, long user_id)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "info",
                user_id = user_id
            };
            WriteLogMessage(log);
        }
        public static void Info(string message, string ip, long user_id)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "info",
                user_ip = ip,
                user_id = user_id
            };
            WriteLogMessage(log);
        }
        public static void Warn(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "Warn",
            };
            WriteLogMessage(log);
        }
        public static void Warn(string message, string ip)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "Warn",
                user_ip = ip
            };
            WriteLogMessage(log);
        }
        public static void Warn(string message, long user_id)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "Warn",
                user_id = user_id
            };
            WriteLogMessage(log);
        }
        public static void Warn(string message, string ip, long user_id)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "Warn",
                user_ip = ip,
                user_id = user_id
            };
            WriteLogMessage(log);
        }
        public static void Error(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "ERROR",
            };
            WriteLogMessage(log);
        }
        public static void Error(string message, string ip)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "ERROR",
                user_ip = ip
            };
            WriteLogMessage(log);
        }
        public static void Error(string message, long user_id)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "ERROR",
                user_id = user_id
            };
            WriteLogMessage(log);
        }
        public static void Error(string message, string ip, long user_id)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "ERROR",
                user_ip = ip,
                user_id = user_id
            };
            WriteLogMessage(log);
        }
        public static void Fatal(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "FATAL",
            };
            WriteLogMessage(log);
        }
        public static void Fatal(string message, string ip)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "FATAL",
                user_ip = ip
            };
            WriteLogMessage(log);
        }
        public static void Fatal(string message, long user_id)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "FATAL",
                user_id = user_id
            };
            WriteLogMessage(log);
        }
        public static void Fatal(string message, string ip, long user_id)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "FATAL",
                user_ip = ip,
                user_id = user_id
            };
            WriteLogMessage(log);
        }
        public static void Off()
        {
            Logging = LogLevel.OFF;
        }
        private static string SetLevelLog(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.DEBUG: return "debug";
                case LogLevel.ERROR: return "error";
                case LogLevel.FATAL: return "fatal";
                case LogLevel.INFO: return "info";
                case LogLevel.OFF: return "off";
                case LogLevel.TRACE: return "trace";
                case LogLevel.WARN: return "warn";
                default: return "fatal";
            }
        }
    }
}
