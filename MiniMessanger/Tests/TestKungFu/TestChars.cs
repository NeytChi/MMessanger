using Common;
using System.IO;
using System.Linq;
using NUnit.Framework;
using miniMessanger.Models;
using miniMessanger.Manage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace miniMessanger.Test
{
    [TestFixture]
    public class TestChats
    {
        public TestChats()
        {
            Validator validator = new Validator();
            this.context = new Context(true, true);
            this.chats = new Chats(context, new Users(context, validator), validator);
            this.authentication = new Authentication(context, new Validator());
        }
        public Context context;
        public Authentication authentication;
        public Chats chats;
        public string message;
        
        [Test]
        public void UploadMessagePhoto()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();

            //chats.UploadMessagePhoto()
        }
        [Test]
        public void MessagePhoto()
        {

        }
        [Test]
        public void ReciprocalUsers()
        {

        }
        [Test]
        public void DeleteFile()
        {
            FormFile file = CreateFormFile();
            string relativePath = chats.CreateFile(file, "/MessagePhoto/");
            chats.DeleteFile(relativePath);
        }
        [Test]
        public void CreateFile()
        {
            FormFile file = CreateFormFile();
            string result = chats.CreateFile(file, "/MessagePhoto/");
            chats.DeleteFile(result);
            Assert.AreEqual(result[0], '/');
        }
        public User CreateMockingUser()
        {
            string UserEmail = "test@gmail.com";
            string UserPassword = "Test1234";
            string UserLogin = "Test";
            string UserToken = "Test";
            User user = authentication.CreateUser(UserEmail, UserLogin, UserPassword);
            UserToken = user.UserToken;
            user.Activate = 1;
            context.User.Update(user);
            context.SaveChanges();
            return user;
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
