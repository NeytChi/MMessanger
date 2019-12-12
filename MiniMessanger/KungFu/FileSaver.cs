using System;
using Common;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace miniMessanger
{
    public class FileSaver
    {
        public string SavePath;
        public FileSaver()
        {
            Config config = new Config();
            this.SavePath = config.savePath;
        }
        public void DeleteFile(string relativePath)
        {
            if (File.Exists(SavePath + relativePath))
            {
                File.Delete(SavePath + relativePath);
                Log.Info("Delete file ->" + relativePath + ".");
            }
        }
        public string CreateFile(IFormFile file, string relativePath)
        {
            DateTime now = DateTime.Now;
            Directory.CreateDirectory(SavePath + relativePath + now.Year + "-" + now.Month + "-" + now.Day);
            
            string UrlPhoto = relativePath + now.Year + "-" + now.Month 
            + "-" + now.Day + "/" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            file.CopyTo(new FileStream(SavePath + UrlPhoto, FileMode.Create));
            Log.Info("Create new file, relative path ->" + UrlPhoto + ".");
            return UrlPhoto;
        }
    }   
}