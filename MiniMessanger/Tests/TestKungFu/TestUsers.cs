using Common;
using NUnit.Framework;
using miniMessanger.Manage;
using miniMessanger.Models;

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
        [Test]
        public void GetUsers()
        {
            DeleteUsers();  
            Blocks blocks = new Blocks(users, context);
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            User third = CreateMockingUser();
            var success = users.GetUsers(first.UserId, 0, 2);
            Assert.AreEqual(success[0].user_id, third.UserId);
            Assert.AreEqual(success[1].user_id, second.UserId);
            blocks.BlockUser(first.UserToken, third.UserPublicToken, "Test block.", ref message);
            var successWithBlocked = users.GetUsers(first.UserId, 0, 2);
            Assert.AreEqual(successWithBlocked[0].user_id, second.UserId);
            var anotherUserWithBlocked = users.GetUsers(third.UserId, 0, 2);
            Assert.AreEqual(anotherUserWithBlocked[0].user_id, second.UserId);
            Assert.AreEqual(anotherUserWithBlocked[1].user_id, first.UserId);
        }
        [Test]
        public void GetUserByGender()
        {
            DeleteUsers();
            Profiles profiles = new Profiles(context);
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            User third = CreateMockingUser();
            first.Profile = profiles.CreateIfNotExistProfile(first.UserId);
            profiles.UpdateGender(first.Profile, "1", ref message);
            var success = users.GetUsersByGender(first.UserId, true, 0, 2);
            Assert.AreEqual(success[0].user_id, third.UserId);
            Assert.AreEqual(success[1].user_id, second.UserId);
        }
        [Test]
        public void GetUserByGenderWithBlocked()
        {
            DeleteUsers();
            Profiles profiles = new Profiles(context);
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            User third = CreateMockingUser();
            first.Profile = profiles.CreateIfNotExistProfile(first.UserId);
            profiles.UpdateGender(first.Profile, "1", ref message);
            Blocks blocks = new Blocks(users, context);
            blocks.BlockUser(first.UserToken, third.UserPublicToken, "Test block.", ref message);
            var successWithBlocked = users.GetUsersByGender(first.UserId, true, 0, 1);
            Assert.AreEqual(successWithBlocked[0].user_id, second.UserId);
        }
        [Test]
        public void GetUserByGenderWithLiked()
        {
            DeleteUsers();
            Profiles profiles = new Profiles(context);
            User first = CreateMockingUser();
            User second = CreateMockingUser();
            User third = CreateMockingUser();
            first.Profile = profiles.CreateIfNotExistProfile(first.UserId);
            profiles.UpdateGender(first.Profile, "1", ref message);   
            users.CreateLike(first.UserId, third.UserId);
            var successWithLiked = users.GetUsersByGender(first.UserId, true, 0, 1);
            Assert.AreEqual(successWithLiked[0].user_id, second.UserId);
        }
        [Test]
        public void ReciprocalUsers()
        {

        }
        public User UserEnviroment()
        {
            DeleteUsers();
            return CreateMockingUser();
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
        public void DeleteUsers()
        {
            context.RemoveRange(context.User);
        }
    }
}
