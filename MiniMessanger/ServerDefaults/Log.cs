using System;
using System.IO;
using System.Threading;
using miniMessanger.Models;

namespace Common
{
    public static class Log
    {
        private static string PathLogs = Directory.GetCurrentDirectory() + "/logs/";
        private static string FileName = DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year;
        private static string FullPathLog = PathLogs + FileName;
        private static DateTime CurrentFileDate = DateTime.Now;
        private static string UserComputer = Environment.UserName + "-" + Environment.MachineName;
        private static StreamWriter Writer;
        
        public static void WriteLogMessage(LogMessage log)
        {
            if (!string.IsNullOrEmpty(log.message))
            {
                log.userComputer = UserComputer;
                log.time = DateTime.Now;
                log.threadId = Thread.CurrentThread.ManagedThreadId;
                System.Diagnostics.Debug.WriteLine(log.message); 
                CheckLogFile(log.time);
                WriteLogToFile(log);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(log.message);
            }   
        }
        public static void WriteLogToFile(LogMessage log)
        {
            Writer.WriteAsync
            (
                "Time: " + log.time + " | " +
                log.level + " | " +
                "Message: " + log.message  + " | " + 
                "UID: " + log.userId + " | " +
                "UIP: " + log.userIp + " | " +
                "TID: " + log.threadId + " | " +
                "UPC: " + log.userComputer + " | " +
                "\r\n"
            );
            Writer.Flush();
        }
        private static void CheckLogFile(DateTime Local)
        {
            if (!Directory.Exists(PathLogs))
            {
                Directory.CreateDirectory(PathLogs);
            }
            if (!File.Exists(FullPathLog) || Local.Day != CurrentFileDate.Day || Writer == null)
            {
                if (Writer != null)
                {
                    Writer.Dispose();
                }
                CurrentFileDate = Local;
                FileName = CurrentFileDate.Day + "-" + CurrentFileDate.Month + "-" + CurrentFileDate.Year;
                FullPathLog = PathLogs + FileName;
                if (File.Exists(FullPathLog))
                {
                    Writer = new StreamWriter(FullPathLog, true);
                }
                else
                {
                    Writer = File.CreateText(FullPathLog);
                }
            }
        }
        public static void Trace(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "TRACE",
            };
            WriteLogMessage(log);
        }
        public static void Debug(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "DEBUG",
            };
            WriteLogMessage(log);
        }
        public static void Info(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "INFO"
            };
            WriteLogMessage(log);
        }
        public static void Info(string message, string ip)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "INFO",
                userIp = ip
            };
            WriteLogMessage(log);
        }
        public static void Info(string message, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "INFO",
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Info(string message, string ip, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "INFO",
                userIp = ip,
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Warn(string message)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "WARN",
            };
            WriteLogMessage(log);
        }
        public static void Warn(string message, string ip)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "WARN",
                userIp = ip
            };
            WriteLogMessage(log);
        }
        public static void Warn(string message, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "WARN",
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Warn(string message, string ip, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "WARN",
                userIp = ip,
                userId = userId
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
                userIp = ip
            };
            WriteLogMessage(log);
        }
        public static void Error(string message, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "ERROR",
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Error(string message, string ip, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "ERROR",
                userIp = ip,
                userId = userId
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
                userIp = ip
            };
            WriteLogMessage(log);
        }
        public static void Fatal(string message, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "FATAL",
                userId = userId
            };
            WriteLogMessage(log);
        }
        public static void Fatal(string message, string ip, long userId)
        {
            LogMessage log = new LogMessage
            {
                message = message,
                level = "FATAL",
                userIp = ip,
                userId = userId
            };
            WriteLogMessage(log);
        }
    }
}
