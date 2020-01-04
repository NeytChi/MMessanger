using System;
using Common;
using Serilog;
using System.IO;
using Serilog.Core;
using Microsoft.AspNetCore.Http;

namespace miniMessanger
{
    public class FileSaver
    {
        public Logger log;
        public string savepath;
        public FileSaver()
        {
            Config config = new Config();
            this.savepath = config.savePath;
            log = new LoggerConfiguration()
            .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        }
        public void DeleteFile(string relativePath)
        {
            if (File.Exists(savepath + relativePath))
            {
                File.Delete(savepath + relativePath);
                log.Information("Delete file, path ->" + relativePath);
            }
        }
        public string CreateFile(IFormFile file, string relativePath)
        {
            DateTime now = DateTime.Now;
            string dir = relativePath + now.Year + "-" + now.Month + "-" + now.Day;

            Directory.CreateDirectory(savepath + dir);
            
            string url = dir + "/" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            file.CopyTo(new FileStream(savepath + url, FileMode.Create));
            log.Information("Create new file, relative path ->" + url);
            return url;
        }
    }   
}