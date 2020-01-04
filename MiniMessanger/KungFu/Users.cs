using Common;
using Serilog;
using System.Linq;
using Serilog.Core;
using miniMessanger.Models;
using System.Collections.Generic;

namespace miniMessanger.Manage
{
    public class Users
    {
        public Context context;
        public string awsPath;
        public Validator validator;
        public Logger log;
        public Users(Context context, Validator validator)
        {
            Config config = new Config();
            this.context = context;
            this.awsPath = config.AwsPath;
            this.validator = validator;
            log = new LoggerConfiguration()
            .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        }
        public LikeProfiles LikeUser(UserCache cache, ref string message)
        {
            User user = GetUserByToken(cache.user_token, ref message);
            User opposideUser = GetUserByPublicToken(cache.opposide_public_token, ref message);
            if (user != null && opposideUser != null)
            {
                if (user.UserId != opposideUser.UserId)
                {
                    return CreateLike(user.UserId, opposideUser.UserId);                    
                }
                else
                {
                    message = "User can't like himself.";
                }
            }
            return null;
        }
        public LikeProfiles CreateLike(int UserId, int OpposideUserId)
        {
            LikeProfiles like = GetLikeProfiles(UserId, OpposideUserId);
            like.Like = like.Like == true ? false : true; 
            if (like.Like && like.Dislike)
            {
                like.Dislike = false;
            }
            context.LikeProfile.Update(like);
            context.SaveChanges();
            return like;
        }
        public LikeProfiles DislikeUser(UserCache cache, ref string message)
        {
            User user = GetUserByToken(cache.user_token, ref message);
            User opposideUser = GetUserByPublicToken(cache.opposide_public_token, ref message);
            if (user != null && opposideUser != null)
            {
                if (user.UserId != opposideUser.UserId)
                {
                    return CreateDislike(user.UserId, opposideUser.UserId);
                }
                else
                {
                    message = "User can't like himself.";
                }
            }
            return null;
        }
        public LikeProfiles CreateDislike(int UserId, int OpposideUserId)
        {
            LikeProfiles like = GetLikeProfiles(UserId, OpposideUserId);
            like.Dislike = like.Dislike == true ? false : true;
            if (like.Dislike && like.Like)
            {
                like.Like = false;
            }
            context.LikeProfile.Update(like);
            context.SaveChanges();
            return like;
        }
        public LikeProfiles GetLikeProfiles(int userId, int toUserId)
        {
            LikeProfiles like = context.LikeProfile.Where(l 
            => l.UserId == userId 
            && l.ToUserId == toUserId).FirstOrDefault();
            if (like == null)
            {
                like = new LikeProfiles()
                {
                    UserId = userId,
                    ToUserId = toUserId,
                    Like = false,
                    Dislike = false
                };
                context.LikeProfile.Add(like);
                context.SaveChanges();
            }
            return like;
        }
        public User GetUserByToken(string userToken, ref string message)
        {
            if (!string.IsNullOrEmpty(userToken))
            {
                User user = context.User.Where(u 
                => u.UserToken == userToken).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by token.";
                }
                return user;
            }
            return null;
        }
        public User GetUserByPublicToken(string token, ref string message)
        {
            if (!string.IsNullOrEmpty(token))
            {
                User user = context.User.Where(u 
                => u.UserPublicToken == token
                && u.Activate == 1
                && u.Deleted == false).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by token.";
                }
                return user;
            }
            return null;
        }
        public User GetUserWithProfile(string userToken, ref string message)
        {
            if (!string.IsNullOrEmpty(userToken))
            {
                User user = context.User.Where(u => 
                u.UserToken == userToken).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by token.";
                }
                else
                {
                    user.Profile = context.Profile.Where(p 
                    => p.UserId == user.UserId).FirstOrDefault();
                }
                return user;
            }
            return null;
        }
        public dynamic GetUsers(int userid, int page = 0, int count = 30)
        {
            log.Information("Get users by user, id -> " + userid);
            return (from users in context.User
            join block in context.BlockedUsers on users.UserId equals block.BlockedUserId into blocks
            where users.UserId != userid
            && users.Activate == 1
            && !users.Deleted
            && ( blocks.All(b => b.UserId == userid && b.BlockedDeleted) || blocks.Count() == 0)
            orderby users.UserId descending
            select new
            {
                user_id = users.UserId,
                user_email = users.UserEmail,
                user_login = users.UserLogin,
                created_at = users.CreatedAt,
                last_login_at = users.LastLoginAt,
                user_public_token = users.UserPublicToken
            })
            .Skip(page * count).Take(count).ToList();
        }
        /// <summary>
        /// Select list of users with profile data, like and dislike keys.
        /// Doesn't select users who a block by current user and users that current user d
        /// </summary>
        public dynamic GetUsersByGender(int userid, bool ProfileGender, int page = 0, int count = 30)
        {
            log.Information("Get users by user and gender, id -> " + userid);
            return (from users in context.User
            join profile in context.Profile on users.UserId equals profile.UserId
            join likesProfile in context.LikeProfile on users.UserId equals likesProfile.ToUserId into likes    
            join block in context.BlockedUsers on users.UserId equals block.BlockedUserId into blocks
            where users.UserId != userid
            && profile.ProfileGender != ProfileGender
            && (!likes.Any(l => l.UserId == userid && l.Like))
            && users.Activate == 1
            && !users.Deleted
            && ( blocks.All(b => b.UserId == userid && b.BlockedDeleted) || blocks.Count() == 0)
            orderby users.UserId descending
            select new 
            { 
                user_id = users.UserId,
                user_email = users.UserEmail,
                user_login = users.UserLogin,
                created_at = users.CreatedAt,
                last_login_at = users.LastLoginAt,
                user_public_token = users.UserPublicToken,
                profile = new 
                {
                    url_photo = profile.UrlPhoto == null ? "" : awsPath + profile.UrlPhoto,
                    profile_age = profile.ProfileAge == null ? -1 : profile.ProfileAge,
                    profile_gender = profile.ProfileGender,
                    profile_city = profile.ProfileCity == null  ? "" : profile.ProfileCity
                },
                liked_user = false,
                disliked_user = likes.Any(l => l.Dislike && l.UserId == userid) ? true : false
            })
            .Skip(page * count).Take(count).ToList();
        }
        public dynamic ReciprocalUsers(int userId, bool profileGender, int page, int count)
        {
            bool exist = false;
            var likedUsers = GetLikedUsers(userId, profileGender, page, count);
            var reciprocalUsers = GetReciprocalUsers(userId, profileGender, page, count);
            foreach (dynamic reciprocal in reciprocalUsers)
            {
                if (likedUsers.Count < count)
                {
                    foreach (dynamic user in likedUsers)
                    {
                        if (user.user_id == reciprocal.user_id)
                        {
                            exist = true;
                        }
                    }
                    if (!exist)
                    {
                        likedUsers.Add(reciprocal);
                    }
                    exist = false;
                }
            }
            log.Information("Get reciprocal users, id -> " + userId);
            return likedUsers;
        }
        public dynamic GetLikedUsers(int userId, bool profileGender, int page, int count)
        {
            log.Information("Get liked users by user, id -> " + userId);
            return (from users in context.User
            join like in context.LikeProfile on users.UserId equals like.ToUserId
            join profile in context.Profile on users.UserId equals profile.UserId
            join blocked in context.BlockedUsers on users.UserId equals blocked.BlockedUserId into blocks
            where like.UserId == userId
            && like.Like 
            && !users.Deleted
            && profile.ProfileGender != profileGender 
            && ( blocks.All(b => b.UserId == userId && b.BlockedDeleted) || blocks.Count() == 0)
            orderby users.UserId
            select new
            { 
                user_id = users.UserId,
                user_email = users.UserEmail,
                user_public_token = users.UserPublicToken,
                user_login = users.UserLogin,
                last_login_at = users.LastLoginAt,
                profile = new 
                {
                    url_photo = profile.UrlPhoto == null ? "" : awsPath + profile.UrlPhoto,
                    profile_age = profile.ProfileAge == null ? -1 : (sbyte)(long)profile.ProfileAge,
                    profile_gender = profile.ProfileGender,
                    profile_city = profile.ProfileCity == null  ? "" : profile.ProfileCity
                },
                liked_user = like.Like,
                disliked_user = like.Dislike
            }).Skip(page * count).Take(count).ToList();
        }
        public dynamic GetReciprocalUsers(int userId, bool profileGender, int page, int count)
        {
            log.Information("Get liked users by user(reciprocal), id -> " + userId);
            return (from users in context.User
            join like in context.LikeProfile on users.UserId equals like.UserId
            join profile in context.Profile on users.UserId equals profile.UserId
            join blocked in context.BlockedUsers on users.UserId equals blocked.BlockedUserId into blocks
            where like.ToUserId == userId
            && like.Like 
            && !users.Deleted
            && profile.ProfileGender != profileGender 
            && ( blocks.All(b => b.BlockedUserId == userId && b.BlockedDeleted) || blocks.Count() == 0)
            orderby users.UserId
            select new
            { 
                user_id = users.UserId,
                user_email = users.UserEmail,
                user_public_token = users.UserPublicToken,
                user_login = users.UserLogin,
                last_login_at = users.LastLoginAt,
                profile = new 
                {
                    url_photo = profile.UrlPhoto == null ? "" : awsPath + profile.UrlPhoto,
                    profile_age = profile.ProfileAge == null ? -1 : (sbyte)(long)profile.ProfileAge,
                    profile_gender = profile.ProfileGender,
                    profile_city = profile.ProfileCity == null  ? "" : profile.ProfileCity
                },
                liked_user = like.Like,
                disliked_user = like.Dislike
            }).Skip(page * count).Take(count).ToList();
        }
    }
}