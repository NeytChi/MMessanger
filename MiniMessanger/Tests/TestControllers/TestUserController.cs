using Controllers;
using System.Linq;
using NUnit.Framework;
using miniMessanger.Models;

namespace miniMessanger.Test
{
    [TestFixture]
    public class TestUserController
    {
        public UsersController controller;
        public TestFileSaver saver;
        public UserCache cache = new UserCache();
        public Context context;
        public string message;
        public TestUserController()
        {
            this.context = new Context(true, true);
            this.controller = new UsersController(context);
            saver = new TestFileSaver();
        }
        [Test]
        public void Registration()
        {
            SetUpUserCache();
            var success = controller.Registration(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void RegistrationResponse()
        {
            var success = controller.RegistrationResponse("Test message", "123456");
            Assert.AreEqual(success.success, true);
        }
        [Test]
        public void RegistrationEmail()
        {
            SetUpUserCache();
            controller.Registration(cache);
            var success = controller.RegistrationEmail(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void Login()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            var success = controller.Login(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void UserResponse()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            var success = controller.UserResponse(user);
            Assert.AreEqual(success.user_id, user.UserId);
        }
        [Test]
        public void LogOut()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            cache.user_token = user.UserToken;
            controller.Activate(user.UserHash);
            var success = controller.LogOut(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void RecoveryPassword()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            cache.user_token = user.UserToken;
            controller.Activate(user.UserHash);
            var success = controller.RecoveryPassword(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void CheckRecoveryCode()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            cache.user_token = user.UserToken;
            controller.Activate(user.UserHash);
            controller.RecoveryPassword(cache);
            user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            cache.recovery_code = (int)user.RecoveryCode;
            var success = controller.CheckRecoveryCode(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void ChangePassword()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            cache.user_token = user.UserToken;
            controller.Activate(user.UserHash);
            controller.RecoveryPassword(cache);
            user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            cache.recovery_code = (int)user.RecoveryCode;
            controller.CheckRecoveryCode(cache);
            user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            cache.recovery_token = user.RecoveryToken;
            cache.user_confirm_password = cache.user_password;
            var success = controller.ChangePassword(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void Activate()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            var success = controller.Activate(user.UserHash);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void Delete()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            var success = controller.Delete(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void UpdateProfile()
        {
            // SetUpUserCache();
            // controller.Registration(cache);
            // User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            // controller.Activate(user.UserHash);
            // cache.user_token = user.UserToken;
            // var file = saver.CreateFormFile();
            // var success = controller.UpdateProfile(file);
            // Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void RegistrateProfile()
        {

        }
        [Test]
        public void ProfileToResponse()
        {
            Profile profile = new Profile();
            profile.ProfileAge = 12;
            dynamic success = controller.ProfileToResponse(profile);
            Assert.AreEqual(success.profile_age, 12);
        }
        [Test]
        public void Profile()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            var success = controller.Profile(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void GetUsersList()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            CreateAnotherUser();
            var success = controller.GetUsersList(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void UsersResponse()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            var success = controller.UsersResponse(user);
            Assert.AreEqual(success.user_id, user.UserId);
        }
        [Test]
        public void CreateChat()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            User anotherUser = CreateAnotherUser();
            cache.opposide_public_token = anotherUser.UserPublicToken;
            var success = controller.CreateChat(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void SelectChats()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            User anotherUser = CreateAnotherUser();
            cache.opposide_public_token = anotherUser.UserPublicToken;
            controller.CreateChat(cache);
            cache.page = 0;
            cache.count = 1;
            var success = controller.SelectChats(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void SelectMessages()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            User anotherUser = CreateAnotherUser();
            cache.opposide_public_token = anotherUser.UserPublicToken;
            var chat = controller.CreateChat(cache);
            ChatCache chatCache = new ChatCache()
            {
                user_token = user.UserToken,
                chat_token = chat.Value.data.chat_token,
                page = 0,
                count = 1
            };
            controller.SendMessage(chatCache);
            var success = controller.SelectMessages(chatCache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void SendMessage()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            User anotherUser = CreateAnotherUser();
            cache.opposide_public_token = anotherUser.UserPublicToken;
            var chat = controller.CreateChat(cache);
            ChatCache chatCache = new ChatCache()
            {
                user_token = user.UserToken,
                chat_token = chat.Value.data.chat_token,
                message_text = "Test message."
            };
            var success = controller.SendMessage(chatCache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void MessagePhoto()
        {

        }
        [Test]
        public void BlockUser()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            User anotherUser = CreateAnotherUser();
            cache.opposide_public_token = anotherUser.UserPublicToken;
            cache.blocked_reason = "Test block.";
            var success = controller.BlockUser(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void GetBlockedUsers()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            CreateAnotherUser();
            var success = controller.GetBlockedUsers(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void UnblockUser()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            User anotherUser = CreateAnotherUser();
            cache.opposide_public_token = anotherUser.UserPublicToken;
            cache.blocked_reason = "Test block.";
            controller.BlockUser(cache);
            var success = controller.UnblockUser(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void ComplaintContent()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            User anotherUser = CreateAnotherUser();
            cache.opposide_public_token = anotherUser.UserPublicToken;
            var chat = controller.CreateChat(cache);
            ChatCache chatCache = new ChatCache()
            {
                user_token = user.UserToken,
                chat_token = chat.Value.data.chat_token,
                message_text = "Test message."
            };
            cache.complaint = "Test block.";
            cache.user_token = anotherUser.UserToken;
            cache.opposide_public_token = user.UserPublicToken;
            var message = controller.SendMessage(chatCache);
            cache.message_id = message.Value.data.message_id;
            var success = controller.ComplaintContent(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void SelectChatsByGender()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            User anotherUser = CreateAnotherUser();
            cache.opposide_public_token = anotherUser.UserPublicToken;
            controller.CreateChat(cache);
            cache.page = 0;
            cache.count = 1;
            var success = controller.SelectChatsByGender(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void GetUsersByGender()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            CreateAnotherUser();
            var success = controller.GetUsersByGender(cache);
            Assert.AreEqual(success.Value.success, true);
        }        
        [Test]
        public void ReciprocalUsers()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            CreateAnotherUser();
            var success = controller.ReciprocalUsers(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void LikeUnlikeUsers()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            User anotherUser = CreateAnotherUser();
            cache.opposide_public_token = anotherUser.UserPublicToken;
            var success = controller.LikeUnlikeUsers(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        [Test]
        public void DislikeUsers()
        {
            SetUpUserCache();
            controller.Registration(cache);
            User user = context.User.Where(u => u.UserEmail == cache.user_email).First();
            controller.Activate(user.UserHash);
            cache.user_token = user.UserToken;
            controller.Profile(cache);
            User anotherUser = CreateAnotherUser();
            cache.opposide_public_token = anotherUser.UserPublicToken;
            var success = controller.DislikeUsers(cache);
            Assert.AreEqual(success.Value.success, true);
        }
        public void SetUpUserCache()
        {
            context.User.RemoveRange(context.User.ToList());
            context.SaveChanges();
            controller.users.context = context;
            controller.authentication.context = context;
            controller.chats.context = context;
            controller.blocks.context = context;
            controller.profiles.context = context;
            cache.user_login = "testUser";
            cache.user_email = "test@gmail.com";
            cache.user_password = "Pass1234";
        }
        public User CreateAnotherUser()
        {
            UserCache anotherCache = new UserCache();
            anotherCache.user_login = "testUserSecond";
            anotherCache.user_email = "testSecond@gmail.com";
            anotherCache.user_password = "Pass1234";
            controller.Registration(anotherCache);
            User user = context.User.Where(u => u.UserEmail == anotherCache.user_email).First();
            controller.Activate(user.UserHash);
            anotherCache.user_token = user.UserToken;
            var success = controller.Profile(cache);
            return context.User.Where(u => u.UserEmail == anotherCache.user_email).First();
        }
    }
}