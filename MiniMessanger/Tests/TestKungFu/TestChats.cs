using Common;
using System.IO;
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
        public TestFileSaver saver = new TestFileSaver();
        public Chats chats;
        public string message;
        
        [Test]
        public void CreateChat()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            Chatroom room = chats.CreateChat(first.UserToken, second.UserPublicToken, ref message);
        }
        [Test]
        public void CreateChatIfNotExist()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            Chatroom room = chats.CreateChatIfNotExist(first, second);
        }
        [Test]
        public void SaveChat()
        {
            var room = chats.SaveChat();
        }
        [Test]
        public void SaveParticipants()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            Chatroom room = chats.SaveChat();
            chats.SaveParticipants(room.ChatId, first.UserId, second.UserId);
        }
        [Test]
        public void CreateMessage()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            Chatroom room = chats.CreateChat(first.UserToken, second.UserPublicToken, ref message);
            chats.CreateMessage("Testing text.", first.UserToken, room.ChatToken , ref message);
        }
        [Test]
        public void SaveTextMessage()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            Chatroom room = chats.CreateChat(first.UserToken, second.UserPublicToken, ref message);
            chats.SaveTextMessage(room.ChatId, first.UserId, "Testing text.");
        }
        [Test]
        public void CheckMessageText()
        {
            string messageText = "Test text.";
            bool success = chats.CheckMessageText(ref messageText, ref message);
            while (messageText.Length < 510)
                messageText += messageText;
            bool veryLongText = chats.CheckMessageText(ref messageText, ref message);
            messageText = "";
            bool emptyText = chats.CheckMessageText(ref messageText, ref message);
            Assert.AreEqual(success, true);
            Assert.AreEqual(veryLongText, false);
            Assert.AreEqual(emptyText, false);
        }
        [Test]
        public void GetChatroom()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            Chatroom room = chats.CreateChat(first.UserToken, second.UserPublicToken, ref message);
            Chatroom success = chats.GetChatroom(room.ChatToken, ref message);
            Chatroom unknowRoom = chats.GetChatroom("", ref message);
            Assert.AreEqual(success.ChatId, room.ChatId);
            Assert.AreEqual(unknowRoom, null);
        }
        [Test]
        public void GetMessages()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            Chatroom room = chats.CreateChat(first.UserToken, second.UserPublicToken, ref message);
            Message testMessage = chats.CreateMessage("Test message", first.UserToken, room.ChatToken, ref message);
            dynamic success = chats.GetMessages(first.UserId, room.ChatToken, 0, 30, ref message);
            Assert.AreEqual(success[0].message_id, testMessage.MessageId);
        }
        [Test]
        public void UpdateMessagesToViewed()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            Chatroom room = chats.CreateChat(first.UserToken, second.UserPublicToken, ref message);
            Message testMessage = chats.CreateMessage("Test message", first.UserToken, room.ChatToken, ref message);
            chats.UpdateMessagesToViewed(room.ChatId, first.UserId);
        }
        [Test]
        public void UploadMessagePhoto()
        {
            User first = CreateMockingUser();
            Chatroom room = chats.CreateChat(first.UserToken, CreateMockingUser().UserPublicToken, ref message);
            ChatCache cache = new ChatCache()
            {
                user_token = first.UserToken,
                chat_token = room.ChatToken
            };
            FormFile file = saver.CreateFormFile();
            Message success = chats.UploadMessagePhoto(file, cache, ref message);   
            Message nullable = chats.UploadMessagePhoto(null, cache, ref message);
            saver.system.DeleteFile(success.UrlFile);
            Assert.AreEqual(success.UserId, first.UserId);
            Assert.AreEqual(nullable, null);
        }
        [Test]
        public void MessagePhoto()
        {
            User first = CreateMockingUser();
            Chatroom room = chats.CreateChat(first.UserToken, CreateMockingUser().UserPublicToken, ref message);
            ChatCache cache = new ChatCache()
            {
                user_token = first.UserToken,
                chat_token = room.ChatToken
            };
            FormFile file = saver.CreateFormFile();
            Message success = chats.MessagePhoto(file, first.UserId, room.ChatId, ref message);   
            Message nullable = chats.MessagePhoto(null, first.UserId, room.ChatId, ref message);
            saver.system.DeleteFile(success.UrlFile);
            Assert.AreEqual(success.UserId, first.UserId);
            Assert.AreEqual(nullable, null);
        }
        [Test]
        public void GetChats()
        {
            User firstUser = CreateMockingUser();
            User secondUser = CreateMockingUser();
            User thirdUser = CreateMockingUser();
            var firstRoom = chats.CreateChat(firstUser.UserToken, secondUser.UserPublicToken, ref message);
            var secondRoom = chats.CreateChat(firstUser.UserToken, thirdUser.UserPublicToken, ref message);
            var firstMessage = chats.CreateMessage("Testing text.", firstUser.UserToken, firstRoom.ChatToken, ref message);
            var secondMessage = chats.CreateMessage("Testing text.", firstUser.UserToken, secondRoom.ChatToken, ref message);
            ChatCache cache = new ChatCache();
            cache.user_token = firstUser.UserToken;
            var success = chats.GetChats(firstUser.UserId, 0, 2);
            Assert.AreEqual(success[0].user.user_id, secondUser.UserId);
            Assert.AreEqual(success[1].user.user_id, thirdUser.UserId);
            Assert.AreEqual(success[0].chat.chat_id, firstRoom.ChatId);
            Assert.AreEqual(success[1].chat.chat_id, secondRoom.ChatId);
            Assert.AreEqual(success[0].last_message.message_id, firstMessage.MessageId);
            Assert.AreEqual(success[1].last_message.message_id, secondMessage.MessageId);
        }
        [Test]
        public void GetChatsWithBlocking()
        {
            User firstUser = CreateMockingUser();
            User secondUser = CreateMockingUser();
            User thirdUser = CreateMockingUser();
            var firstRoom = chats.CreateChat(firstUser.UserToken, secondUser.UserPublicToken, ref message);
            var secondRoom = chats.CreateChat(firstUser.UserToken, thirdUser.UserPublicToken, ref message);
            var firstMessage = chats.CreateMessage("Testing text.", firstUser.UserToken, firstRoom.ChatToken, ref message);
            var secondMessage = chats.CreateMessage("Testing text.", firstUser.UserToken, secondRoom.ChatToken, ref message);
            Blocks blocks = new Blocks(new Users(context, new Validator()), context);
            blocks.BlockUser(firstUser.UserToken, secondUser.UserPublicToken, "Test block.", ref message);
            ChatCache cache = new ChatCache();
            cache.user_token = firstUser.UserToken;
            var successWithBlocking = chats.GetChats(firstUser.UserId, 0, 2);
            Assert.AreEqual(successWithBlocking[0].user.user_id, thirdUser.UserId);
            Assert.AreEqual(successWithBlocking[0].chat.chat_id, secondRoom.ChatId);
            Assert.AreEqual(successWithBlocking[0].last_message.message_id, secondMessage.MessageId);   
        }
        [Test]
        public void GetChatsWithoutMessage()
        {
            User firstUser = CreateMockingUser();
            User secondUser = CreateMockingUser();
            var firstRoom = chats.CreateChat(firstUser.UserToken, secondUser.UserPublicToken, ref message);
            ChatCache cache = new ChatCache();
            cache.user_token = firstUser.UserToken;
            var successWithoutMessage = chats.GetChats(firstUser.UserId, 0, 2);
            Assert.AreEqual(successWithoutMessage[0].user.user_id, secondUser.UserId);
            Assert.AreEqual(successWithoutMessage[0].chat.chat_id, firstRoom.ChatId);
            Assert.AreEqual(successWithoutMessage[0].last_message, null);
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