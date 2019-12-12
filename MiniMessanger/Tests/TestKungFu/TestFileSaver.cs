using System.IO;
using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace miniMessanger.Test
{
    [TestFixture]
    public class TestFileSaver
    {
        public FileSaver system = new FileSaver();
        [Test]
        public void DeleteFile()
        {
            FormFile file = CreateFormFile();
            string relativePath = system.CreateFile(file, "/ProfilePhoto/");
            system.DeleteFile(relativePath);
        }
        [Test]
        public void CreateFile()
        {
            FormFile file = CreateFormFile();
            string result = system.CreateFile(file, "/ProfilePhoto/");
            system.DeleteFile(result);
            Assert.AreEqual(result[0], '/');
        }
        public FormFile CreateFormFile()
        {
            byte[] fileBytes = File.ReadAllBytes("/home/neytchi/Configuration/messanger-configuration/parrot.jpg");
            FormFile file = new FormFile(new MemoryStream(fileBytes), 0, 0, "file", "parrot.jpg");
            file.Headers = new HeaderDictionary();
            file.ContentType = "image/jpeg";
            return file;
        }
    }
}