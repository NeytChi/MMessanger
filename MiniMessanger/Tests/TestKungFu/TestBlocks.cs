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
            blocks.CreateBlockedUser(first.UserId, second.UserId, "Test block");
        }
        
        [Test]
        public void CheckComplaintMessage()
        {
            string blockedReason = "Test block";
            bool success = blocks.CheckComplaintMessage(blockedReason, ref message);
            bool empty = blocks.CheckComplaintMessage("", ref message);
            bool nullable = blocks.CheckComplaintMessage(null, ref message);
            while (blockedReason.Length < 100)
                blockedReason += blockedReason;
            bool moreCharacters = blocks.CheckComplaintMessage(blockedReason, ref message);
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
