using Common;
using System.Linq;
using NUnit.Framework;
using miniMessanger.Models;

namespace miniMessanger.Test
{
    [TestFixture]
    public class TestAuthentication
    {
        public TestAuthentication()
        {
            this.context = new Context(true, true);
            this.validator = new Validator();
            this.authentication = new Authentication(context, validator);
        }
        public Context context;
        public Validator validator;
        public Authentication authentication;
        public string UserEmail = "test@gmail.com";
        public string UserPassword = "Test1234";
        public string UserLogin = "Test";
        public string UserToken = "Test";
        public string message;
        
        [Test]
        public void GetActiveUserByEmail()
        {
            User user = CreateMockingUser();
            User Success = authentication.GetActiveUserByEmail(user.UserEmail, ref message);
            User UnknowEmail = authentication.GetActiveUserByEmail("1234", ref message);
            Assert.AreEqual(Success.UserId, user.UserId);
            Assert.AreEqual(UnknowEmail, null);
        }
        [Test]
        public void GetNonActivateUserByEmail()
        {
            User user = CreateMockingUser();
            user.UserEmail = UserEmail;
            user.Activate = 0;
            context.Update(user);
            context.SaveChanges();
            User NonActivate = authentication.GetActiveUserByEmail(user.UserEmail, ref message);
            Assert.AreEqual(NonActivate, null);
        }
        [Test]
        public void GetDeletedUserByEmail()
        {
            User user = CreateMockingUser();
            user.Deleted = true;
            user.Activate = 1;
            context.Update(user);
            context.SaveChanges();
            User DeletedUser = authentication.GetActiveUserByEmail(user.UserEmail, ref message);
            Assert.AreEqual(DeletedUser, null);
        }
        [Test]
        public void Login()
        {
            User user = CreateMockingUser();
            User success = authentication.Login(user.UserEmail, UserPassword, ref message);
            User unsuccess = authentication.Login("Error", UserPassword, ref message);
            Assert.AreEqual(success.UserEmail, user.UserEmail);
            Assert.AreEqual(unsuccess, null);
        }
        [Test]
        public void GetUserByToken()
        {
            User user = CreateMockingUser();
            User Success = authentication.GetUserByToken(user.UserToken, ref message);
            User UnknowToken = authentication.GetUserByToken("1234", ref message);
            Assert.AreEqual(Success.UserId, user.UserId);
            Assert.AreEqual(UnknowToken, null);
        }
        [Test]
        public void GetNonActivateUserByToken()
        {
            DeleteUser();
            User user = authentication.CreateUser(UserEmail, UserLogin, UserPassword);
            User NonActivate = authentication.GetUserByToken(user.UserEmail, ref message);
            Assert.AreEqual(NonActivate, null);
        }
        [Test]
        public void GetDeletedUserByToken()
        {
            User user = CreateMockingUser();
            user.Deleted = true;
            user.Activate = 1;
            context.Update(user);
            context.SaveChanges();
            User DeletedUser = authentication.GetUserByToken(user.UserEmail, ref message);
            Assert.AreEqual(DeletedUser, null);
        }
        [Test]
        public void LogOut()
        {
            User user = CreateMockingUser();
            bool success = authentication.LogOut(user.UserToken, ref message);
            bool unsuccess = authentication.LogOut("", ref message);
            Assert.AreEqual(success, true);
            Assert.AreEqual(unsuccess, false);
        }
        [Test]
        public void RecoveryPassword()
        {
            User user = CreateMockingUser();
            bool success = authentication.RecoveryPassword(user.UserEmail, ref message);
            bool unsuccess = authentication.RecoveryPassword("", ref message);
            Assert.AreEqual(success, true);
            Assert.AreEqual(unsuccess, false);
        }
        [Test]
        public void GetUserByEmail()
        {
            User user = CreateMockingUser();
            User success = authentication.GetUserByEmail(user.UserEmail, ref message);
            User unsuccess = authentication.GetUserByEmail("1234", ref message);
            Assert.AreEqual(success.UserEmail, user.UserEmail);
            Assert.AreEqual(unsuccess, null);
        }
        [Test]
        public void ConfirmEmail()
        {
            User user = CreateMockingUser();
            user.Activate = 0;
            context.User.Update(user);
            context.SaveChanges();
            bool success = authentication.ConfirmEmail(user.UserEmail, ref message);
            bool unsuccess = authentication.ConfirmEmail("1234", ref message);
            Assert.AreEqual(success, true);
            Assert.AreEqual(unsuccess, false);
        }
        [Test]
        public void CheckRecoveryCode()
        {
            User user = CreateMockingUser();
            user.RecoveryCode = 1234;
            context.User.Update(user);
            context.SaveChanges();
            string success = authentication.CheckRecoveryCode(user.UserEmail, (int)user.RecoveryCode,  ref message);
            string unsuccess = authentication.CheckRecoveryCode(user.UserEmail, 12345,  ref message);
            user = context.User.Where(u => u.UserId == user.UserId).First();
            Assert.AreEqual(success, user.RecoveryToken);
            Assert.AreEqual(unsuccess, null);
        }
        [Test]
        public void ChangePassword()
        {
            User user = CreateMockingUser();
            authentication.RecoveryPassword(user.UserEmail, ref message);
            string RecoveryToken = authentication.CheckRecoveryCode(user.UserEmail, (int)user.RecoveryCode,  ref message);
            bool success = authentication.ChangePassword(RecoveryToken, UserPassword, UserPassword, ref message);
            bool unsuccess = authentication.ChangePassword(RecoveryToken, UserPassword, UserPassword, ref message);
            Assert.AreEqual(success, true);
            Assert.AreEqual(unsuccess, false);
        }
        [Test]
        public void GetUserByRecoveryToken()
        {
            User user = CreateMockingUser();
            authentication.RecoveryPassword(user.UserEmail, ref message);
            string RecoveryToken = authentication.CheckRecoveryCode(user.UserEmail, (int)user.RecoveryCode,  ref message);
            User Success = authentication.GetUserByRecoveryToken(RecoveryToken, ref message);
            User UnknowToken = authentication.GetUserByRecoveryToken("1234", ref message);
            Assert.AreEqual(Success.UserId, user.UserId);
            Assert.AreEqual(UnknowToken, null);
        }
        [Test]
        public void GetNonActivateUserByRecoveryToken()
        {
            User user = CreateMockingUser();
            authentication.RecoveryPassword(user.UserEmail, ref message);
            string RecoveryToken = authentication.CheckRecoveryCode(user.UserEmail, (int)user.RecoveryCode,  ref message);
            user.Activate = 0;
            context.User.Update(user);
            context.SaveChanges();
            User NonActivate = authentication.GetUserByRecoveryToken(user.UserEmail, ref message);
            Assert.AreEqual(NonActivate, null);
        }
        [Test]
        public void GetDeletedUserByRecoveryToken()
        {
            User user = CreateMockingUser();
            authentication.RecoveryPassword(user.UserEmail, ref message);
            string RecoveryToken = authentication.CheckRecoveryCode(user.UserEmail, (int)user.RecoveryCode,  ref message);
            user.Deleted = true;
            user.Activate = 1;
            context.Update(user);
            context.SaveChanges();
            User DeletedUser = authentication.GetUserByRecoveryToken(user.UserEmail, ref message);
            Assert.AreEqual(DeletedUser, null);
        }
        [Test]
        public void Registrate()
        {
            DeleteUser();
            UserCache cache = new UserCache();
            cache.user_email = UserEmail;
            cache.user_login = UserLogin;
            cache.user_password = UserPassword;
            User success = authentication.Registrate(cache, ref message);
            User unsuccess = authentication.Registrate(cache, ref message);
            Assert.AreEqual(success.UserEmail, UserEmail);
            Assert.AreEqual(unsuccess, null);
        }
        [Test]
        public void CreateUser()
        {
            User success = authentication.CreateUser(UserEmail, UserLogin, UserPassword);
            User unsuccess = authentication.CreateUser("", "", "");
            Assert.AreEqual(success.UserEmail, UserEmail);
            Assert.AreEqual(unsuccess, null);
        }
        [Test]
        public void RestoreUser()
        {
            User user = CreateMockingUser();
            user.Deleted = true;
            context.User.Update(user);
            context.SaveChanges();
            bool success = authentication.RestoreUser(user, ref message);
            bool unsuccess = authentication.RestoreUser(user, ref message);
            Assert.AreEqual(success, true);
            Assert.AreEqual(unsuccess, false);
        }
        [Test]
        public void Activate()
        {
            User user = authentication.CreateUser(UserEmail, UserLogin, UserPassword);
            bool success = authentication.Activate(user.UserHash, ref message);
            bool unsuccess = authentication.Activate(user.UserHash, ref message);
            user.Deleted = true;
            user.Activate = 0;
            context.User.Update(user);
            context.SaveChanges();
            bool deleted = authentication.Activate(user.UserHash, ref message);
            Assert.AreEqual(success, true);
            Assert.AreEqual(unsuccess, false);
            Assert.AreEqual(deleted, false);
        }
        [Test]
        public void Delete()
        {
            User user = CreateMockingUser();
            bool success = authentication.Delete(user.UserToken, ref message);
            bool unsuccess = authentication.Delete(user.UserToken, ref message);
            Assert.AreEqual(success, true);
            Assert.AreEqual(unsuccess, false);
        }
        [Test]
        public void SendConfirmEmail()
        {
            authentication.SendConfirmEmail(UserEmail, UserEmail);
            authentication.SendConfirmEmail("", "");
        }
        public User CreateMockingUser()
        {
            DeleteUser();
            User user = authentication.CreateUser(UserEmail, UserLogin, UserPassword);
            UserToken = user.UserToken;
            user.Activate = 1;
            context.User.Update(user);
            context.SaveChanges();
            return user;
        }
        public void DeleteUser()
        {
            System.Collections.Generic.List<User> users = context.User.Where(u => u.UserEmail == UserEmail).ToList();
            context.User.RemoveRange(users);
            context.SaveChanges();
        }
    }
}
