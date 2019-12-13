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
            User first  = CreateMockingUser();
            User second = CreateMockingUser();
            User third  = CreateMockingUser();
            var success = users.GetUsers(first.UserId, 0, 2);
            Blocks blocks = new Blocks(users, context);
                blocks.BlockUser(first.UserToken, third.UserPublicToken, "Test block.", ref message);
            var successWithBlocked = users.GetUsers(first.UserId, 0, 2);
            var anotherUserWithBlocked = users.GetUsers(third.UserId, 0, 2);
            Assert.AreEqual(success[0].user_id, third.UserId);
            Assert.AreEqual(success[1].user_id, second.UserId);
            Assert.AreEqual(successWithBlocked[0].user_id, second.UserId);
            Assert.AreEqual(anotherUserWithBlocked[0].user_id, second.UserId);
            Assert.AreEqual(anotherUserWithBlocked[1].user_id, first.UserId);
        }
        [Test]
        public void GetUserByGender()
        {
            DeleteUsers();
            User first  = CreateMockingUser();
            User second = CreateMockingUser();
            User third  = CreateMockingUser();
            Profiles profiles = new Profiles(context);
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
            User first  = CreateMockingUser();
            User second = CreateMockingUser();
            User third  = CreateMockingUser();
            Profiles profiles = new Profiles(context);
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
            User first  = CreateMockingUser();
            User second = CreateMockingUser();
            User third  = CreateMockingUser();
            Profiles profiles = new Profiles(context);
                first.Profile = profiles.CreateIfNotExistProfile(first.UserId);
                profiles.UpdateGender(first.Profile, "1", ref message);   
            users.CreateLike(first.UserId, third.UserId);
            var successWithLiked = users.GetUsersByGender(first.UserId, true, 0, 1);
            Assert.AreEqual(successWithLiked[0].user_id, second.UserId);
        }
        [Test]
        public void GetLikedUsers()
        {
            DeleteUsers();
            User firstUser  = CreateMockingUser();
            User secondUser = CreateMockingUser();
            User thirdUser  = CreateMockingUser();
            Profiles profiles = new Profiles(context);
                firstUser.Profile = profiles.CreateIfNotExistProfile(firstUser.UserId);
                profiles.UpdateGender(firstUser.Profile, "1", ref message);   
            users.CreateLike(firstUser.UserId, thirdUser.UserId);
            users.CreateLike(firstUser.UserId, secondUser.UserId);
            var success = users.GetLikedUsers(firstUser.UserId, true, 0, 2);
            Assert.AreEqual(success[0].user_id, secondUser.UserId);
            Assert.AreEqual(success[1].user_id, thirdUser.UserId);
        }
        [Test]
        public void GetLikedUsersWithBlocked()
        {
            DeleteUsers();
            User firstUser  = CreateMockingUser();
            User secondUser = CreateMockingUser();
            User thirdUser  = CreateMockingUser();
            Profiles profiles = new Profiles(context);
                firstUser.Profile = profiles.CreateIfNotExistProfile(firstUser.UserId);
                profiles.UpdateGender(firstUser.Profile, "1", ref message);   
            users.CreateLike(firstUser.UserId, thirdUser.UserId);
            users.CreateLike(firstUser.UserId, secondUser.UserId);
            Blocks blocks = new Blocks(users, context);
                blocks.BlockUser(firstUser.UserToken, secondUser.UserPublicToken, "Test block.", ref message);
            var success = users.GetLikedUsers(firstUser.UserId, true, 0, 1);
            Assert.AreEqual(success[0].user_id, thirdUser.UserId);
        }
        [Test]
        public void GetReciprocalUsers()
        {
            DeleteUsers();
            User firstUser  = CreateMockingUser();
            User secondUser = CreateMockingUser();
            User thirdUser  = CreateMockingUser();
            Profiles profiles = new Profiles(context);
                firstUser.Profile = profiles.CreateIfNotExistProfile(firstUser.UserId);
                profiles.UpdateGender(firstUser.Profile, "1", ref message);   
            users.CreateLike(secondUser.UserId, firstUser.UserId);
            users.CreateLike(thirdUser.UserId, firstUser.UserId);
            var success = users.GetReciprocalUsers(firstUser.UserId, true, 0, 2);
            Assert.AreEqual(success[0].user_id, secondUser.UserId);
            Assert.AreEqual(success[1].user_id, thirdUser.UserId);
        }
        [Test]
        public void GetReciprocalUsersWithBlocked()
        {
            DeleteUsers();
            User firstUser  = CreateMockingUser();
            User secondUser = CreateMockingUser();
            User thirdUser  = CreateMockingUser();
            Profiles profiles = new Profiles(context);
                firstUser.Profile = profiles.CreateIfNotExistProfile(firstUser.UserId);
                profiles.UpdateGender(firstUser.Profile, "1", ref message);   
            users.CreateLike(thirdUser.UserId, firstUser.UserId);
            users.CreateLike(secondUser.UserId, firstUser.UserId);
            Blocks blocks = new Blocks(users, context);
                blocks.BlockUser(firstUser.UserToken, secondUser.UserPublicToken, "Test block.", ref message);
            var success = users.GetReciprocalUsers(firstUser.UserId, true, 0, 1);
            Assert.AreEqual(success[0].user_id, thirdUser.UserId);
        }
         [Test]
        public void ReciprocalUsers()
        {
            DeleteUsers();
            User firstUser  = CreateMockingUser();
            User secondUser = CreateMockingUser();
            User thirdUser  = CreateMockingUser();
            User fourthUser = CreateMockingUser();
            Profiles profiles = new Profiles(context);
                firstUser.Profile = profiles.CreateIfNotExistProfile(firstUser.UserId);
                profiles.UpdateGender(firstUser.Profile, "1", ref message);   
            users.CreateLike(firstUser.UserId, secondUser.UserId);
            users.CreateLike(thirdUser.UserId, firstUser.UserId);
            users.CreateLike(fourthUser.UserId, firstUser.UserId);
            var success = users.ReciprocalUsers(firstUser.UserId, true, 0, 2);
            Assert.AreEqual(success[0].user_id, secondUser.UserId);
            Assert.AreEqual(success[1].user_id, thirdUser.UserId);
        }
        [Test]
        public void ReciprocalUsersWithBlocked()
        {
            DeleteUsers();
            User firstUser  = CreateMockingUser();
            User secondUser = CreateMockingUser();
            User thirdUser  = CreateMockingUser();
            User fourthUser = CreateMockingUser();
            Profiles profiles = new Profiles(context);
                firstUser.Profile = profiles.CreateIfNotExistProfile(firstUser.UserId);
                profiles.UpdateGender(firstUser.Profile, "1", ref message);   
            users.CreateLike(firstUser.UserId, secondUser.UserId);
            users.CreateLike(thirdUser.UserId, firstUser.UserId);
            users.CreateLike(fourthUser.UserId, firstUser.UserId);
            Blocks blocks = new Blocks(users, context);
                blocks.BlockUser(firstUser.UserToken, secondUser.UserPublicToken, "Test block.", ref message);   
            var success = users.ReciprocalUsers(firstUser.UserId, true, 0, 2);
            Assert.AreEqual(success[0].user_id, thirdUser.UserId);
            Assert.AreEqual(success[1].user_id, fourthUser.UserId);
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
