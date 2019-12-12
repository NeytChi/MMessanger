using Common;
using NUnit.Framework;
using miniMessanger.Models;
using miniMessanger.Manage;

namespace miniMessanger.Test
{
    [TestFixture]
    public class TestBlocks
    {
        public TestBlocks()
        {
            Validator validator = new Validator();
            this.context = new Context(true, true);
            this.blocks = new Blocks(new Users(context, validator), context);
            this.authentication = new Authentication(context, validator);
        }
        public Context context;
        public Authentication authentication;
        public TestFileSaver saver = new TestFileSaver();
        public Blocks blocks;
        public string message;
        
        [Test]
        public void BlockUser()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            bool blocked = blocks.BlockUser(first.UserToken, second.UserPublicToken, "Test block", ref message);
            bool nonDoubleBlocks = blocks.BlockUser(first.UserToken, second.UserPublicToken, "Test block", ref message);
            Assert.AreEqual(blocked, true);
            Assert.AreEqual(nonDoubleBlocks, false);
        }
        [Test]
        public void GetExistBlocked()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            BlockedUser nonBlocked = blocks.GetExistBlocked(first.UserId, second.UserId, ref message);
            bool createBlock = blocks.BlockUser(first.UserToken, second.UserPublicToken, "Test block", ref message);
            BlockedUser blocked = blocks.GetExistBlocked(first.UserId, second.UserId, ref message);
            Assert.AreEqual(createBlock, true);
            Assert.AreEqual(blocked.UserId, first.UserId);
            Assert.AreEqual(nonBlocked, null);
        }
        
        [Test]
        public void CreateBlockedUser()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            BlockedUser block = blocks.CreateBlockedUser(first.UserId, second.UserId, "Test block");
            Assert.AreEqual(block.UserId, first.UserId);
            Assert.AreEqual(block.BlockedUserId, second.UserId);
        }
        [Test]
        public void CheckComplaintMessage()
        {
            string emptyString = "";
            string blockedReason = "Test block";
            bool success = blocks.CheckComplaintMessage(ref blockedReason, ref message);
            bool empty = blocks.CheckComplaintMessage(ref emptyString, ref message);
            emptyString = null;
            bool nullable = blocks.CheckComplaintMessage(ref emptyString, ref message);
            while (blockedReason.Length < 100)
                blockedReason += blockedReason;
            bool moreCharacters = blocks.CheckComplaintMessage(ref blockedReason, ref message);
            Assert.AreEqual(success, true);
            Assert.AreEqual(empty, false);
            Assert.AreEqual(nullable, false);
            Assert.AreEqual(moreCharacters, false);
        }
        
        [Test]
        public void UnblockUser()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            bool createBlock = blocks.BlockUser(first.UserToken, second.UserPublicToken, "Test block", ref message);
            bool unblocked = blocks.UnblockUser(first.UserToken, second.UserPublicToken, ref message);
            bool blockDeleted = blocks.UnblockUser(first.UserToken, second.UserPublicToken, ref message);
            Assert.AreEqual(createBlock, true);
            Assert.AreEqual(unblocked, true);
            Assert.AreEqual(blockDeleted, false);
        }
        [Test]
        public void GetBlockedUser()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            User third = CreateMockingUser();
            blocks.BlockUser(first.UserToken, second.UserPublicToken, "Test blocking.", ref message);
            blocks.BlockUser(first.UserToken, third.UserPublicToken, "Test blocking.", ref message);
            var blocked = blocks.GetBlockedUsers(first.UserId, 0, 2);
            blocks.UnblockUser(first.UserToken, second.UserPublicToken, ref message);
            var blockedWithoutThirdUser = blocks.GetBlockedUsers(first.UserId, 0, 2);
            Assert.AreEqual(blocked[0].user_id, second.UserId);
            Assert.AreEqual(blocked[1].user_id, third.UserId);
            Assert.AreEqual(blockedWithoutThirdUser[0].user_id, third.UserId);
        }
        [Test]
        public void Complaint()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            Chats chats = new Chats(context, new Users(context, new Validator()), new Validator());
            Chatroom room = chats.CreateChat(first.UserToken, second.UserPublicToken, ref message);
            Message firstMessage = chats.CreateMessage("Test message.", first.UserToken, room.ChatToken, ref message);
            Message secondMessage = chats.CreateMessage("Test message.", second.UserToken, room.ChatToken, ref message);
            bool success = blocks.Complaint(first.UserToken, secondMessage.MessageId, "Test complaint.", ref message);
            bool tryCreateAgain = blocks.Complaint(first.UserToken, secondMessage.MessageId, "Test complaint.", ref message);
            bool tryUnknowMessage = blocks.Complaint(first.UserToken, 0, "Test complaint.", ref message);
            bool tryComplainOnHimSelf = blocks.Complaint(first.UserToken, firstMessage.MessageId, "Test complaint.", ref message);
            Assert.AreEqual(success, true);
            Assert.AreEqual(tryCreateAgain, false);
            Assert.AreEqual(tryUnknowMessage, false);
            Assert.AreEqual(tryComplainOnHimSelf, false);
        }
        [Test]
        public void CreateComplaint()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            Chats chats = new Chats(context, new Users(context, new Validator()), new Validator());
            Chatroom room = chats.CreateChat(first.UserToken, second.UserPublicToken, ref message);
            Message roomMessage = chats.CreateMessage("Test message.", first.UserToken, room.ChatToken, ref message);
            BlockedUser block = blocks.CreateBlockedUser(first.UserId, second.UserId, "Test blocking.");
            Complaint complaint = blocks.CreateComplaint(first.UserId, block.BlockedId, roomMessage.MessageId, "Test complaint.");
            Assert.AreEqual(complaint.UserId, first.UserId);
            Assert.AreEqual(complaint.MessageId, roomMessage.MessageId);
            Assert.AreEqual(complaint.BlockId, block.BlockedId);
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
    }
}
