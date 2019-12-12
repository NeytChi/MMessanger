using Common;
using miniMessanger.Manage;
using miniMessanger.Models;
using NUnit.Framework;

namespace miniMessanger.Test
{
    [TestFixture]
    public class TestUsers
    {
        public Users users;
        public Context context;
        public Authentication authentication;
        public string message;
        public TestUsers()
        {
            this.context = new Context(true, true);
            this.users = new Users(context, new Validator());
            this.authentication = new Authentication(context, new Validator());
        }
        [Test]
        public void LikeUser()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            UserCache cache = new UserCache()
            {
                user_token = first.UserToken,
                opposide_public_token = second.UserPublicToken
            };
            LikeProfiles like = users.LikeUser(cache, ref message);
            LikeProfiles unlike = users.LikeUser(cache, ref message);
            Assert.AreEqual(like.UserId, first.UserId);
            Assert.AreEqual(like.ToUserId, second.UserId);
        }
        [Test]
        public void CreateLike()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            LikeProfiles like = users.CreateLike(first.UserId, second.UserId);
            LikeProfiles unlike = users.CreateLike(first.UserId, second.UserId);
            Assert.AreEqual(like.UserId, first.UserId);
            Assert.AreEqual(like.ToUserId, second.UserId);
        }
        [Test]
        public void DislikeUser()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            UserCache cache = new UserCache()
            {
                user_token = first.UserToken,
                opposide_public_token = second.UserPublicToken
            };
            LikeProfiles dislike = users.DislikeUser(cache, ref message);
            LikeProfiles unlike = users.DislikeUser(cache, ref message);
            Assert.AreEqual(dislike.UserId, first.UserId);
            Assert.AreEqual(dislike.ToUserId, second.UserId);
        }
        [Test]
        public void CreateDislike()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            LikeProfiles like = users.CreateDislike(first.UserId, second.UserId);
            LikeProfiles unlike = users.CreateDislike(first.UserId, second.UserId);
            Assert.AreEqual(like.UserId, first.UserId);
            Assert.AreEqual(like.ToUserId, second.UserId);
        }
        [Test]
        public void GetLikeProfiles()
        {
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            LikeProfiles newLike = users.GetLikeProfiles(first.UserId, second.UserId);
            LikeProfiles success = users.GetLikeProfiles(first.UserId, second.UserId);
            Assert.AreEqual(newLike.UserId, first.UserId);
            Assert.AreEqual(success.LikeId, newLike.LikeId);
        }
        [Test]
        public void GetUserByToken()
        {
            User first = CreateMockingUser();
            User success = users.GetUserByToken(first.UserToken, ref message);
            User nullable = users.GetUserByToken("1234", ref message);
            Assert.AreEqual(success.UserId, first.UserId);
            Assert.AreEqual(nullable, null);
        }
        [Test]
        public void GetUserByPublicToken()
        {
            User first = CreateMockingUser();
            User success = users.GetUserByPublicToken(first.UserPublicToken, ref message);
            User nullable = users.GetUserByPublicToken("1234", ref message);
            Assert.AreEqual(success.UserId, first.UserId);
            Assert.AreEqual(nullable, null);
        }
        [Test]
        public void GetUserWithProfile()
        {
            User first = CreateMockingUser();
            User success = users.GetUserWithProfile(first.UserToken, ref message);
            User nullable = users.GetUserWithProfile("1234", ref message);
            Assert.AreEqual(success.UserId, first.UserId);
            Assert.AreEqual(success.Profile.UserId, first.UserId);
            Assert.AreEqual(nullable, null);
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
