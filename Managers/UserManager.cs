using System.Linq;
using miniMessanger.Models;

namespace miniMessanger
{
    public class UserManager
    {
        public MMContext context;
        public string awsPath;
        public UserManager(MMContext context)
        {
            this.context = context;

            this.awsPath = Common.Config.AwsPath;
        }
        public LikeProfiles LikeUser(UserCache cache, ref string message)
        {
            Users user = GetUserByToken(cache.user_token, ref message);
            Users opposideUser = GetUserByOpposideToken(cache.opposide_public_token, ref message);
            if (user != null && opposideUser != null)
            {
                if (user.UserId != opposideUser.UserId)
                {
                    LikeProfiles like = GetLikeProfiles(user.UserId, opposideUser.UserId);
                    if (like.Like)
                    {
                        like.Like = false;
                    }
                    else
                    {
                        like.Like = true;
                    }
                    if (like.Like && like.Dislike)
                    {
                        like.Dislike = false;
                    }
                    context.LikeProfile.Update(like);
                    context.SaveChanges();
                    return like;
                }
                else
                {
                    message = "User can't like himself.";
                }
            }
            return null;
        }
        public LikeProfiles DislikeUser(UserCache cache, ref string message)
        {
            Users user = GetUserByToken(cache.user_token, ref message);
            Users opposideUser = GetUserByOpposideToken(cache.opposide_public_token, ref message);
            if (user != null && opposideUser != null)
            {
                if (user.UserId != opposideUser.UserId)
                {
                    LikeProfiles like = GetLikeProfiles(user.UserId, opposideUser.UserId);
                    if (like.Dislike)
                    {
                        like.Dislike = false;
                    }
                    else
                    {
                        like.Dislike = true;
                    }
                    if (like.Dislike && like.Like)
                    {
                        like.Like = false;
                    }
                    context.LikeProfile.Update(like);
                    context.SaveChanges();
                    return like;
                }
                else
                {
                    message = "User can't like himself.";
                }
            }
            return null;
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
        public Users GetUserByToken(string userToken, ref string message)
        {
            if (!string.IsNullOrEmpty(userToken))
            {
                Users user = context.Users.Where(u => 
                u.UserToken == userToken).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by token.";
                }
                return user;
            }
            return null;
        }
        public Users GetUserByOpposideToken(string opposideToken, ref string message)
        {
            if (!string.IsNullOrEmpty(opposideToken))
            {
                Users user = context.Users.Where(u => 
                u.UserPublicToken == opposideToken).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by token.";
                }
                return user;
            }
            return null;
        }
        public Users GetUserWithProfile(string userToken, ref string message)
        {
            if (!string.IsNullOrEmpty(userToken))
            {
                Users user = context.Users.Where(u => 
                u.UserToken == userToken).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by token.";
                }
                else
                {
                    user.Profile = context.Profiles.Where(p 
                    => p.UserId == user.UserId).First();
                }
                return user;
            }
            return null;
        }
        public dynamic ReciprocalUsers(int userId, int page, int count)
        {
            return (from users in context.Users
            join profile in context.Profiles on users.UserId equals profile.UserId
            join like in context.LikeProfile on userId equals like.UserId
            join opposideLike in context.LikeProfile on users.UserId equals opposideLike.UserId into opposide
            join blocked in context.BlockedUsers on users.UserId equals blocked.BlockedUserId into blockedUsers
            where (opposide.Any(o => o.Like && o.ToUserId == userId) || like.Like)
            && users.UserId != userId
            && (blockedUsers.All(b => b.UserId == userId && b.BlockedDeleted == true)
            || blockedUsers.Count() == 0)
            orderby users.UserId descending
            select new
            {
                user = new 
                { 
                    user_id = users.UserId,
                    user_email = users.UserEmail,
                    user_public_token = users.UserPublicToken,
                    user_login = users.UserLogin,
                    last_login_at = users.LastLoginAt,
                    profile = new 
                    {
                        url_photo = profile.UrlPhoto == null ? "" : awsPath + profile.UrlPhoto,
                        profile_age = profile.ProfileAge == null ? -1 : profile.ProfileAge,
                        profile_gender = profile.ProfileGender,
                        profile_city = profile.ProfileCity == null  ? "" : profile.ProfileCity
                    },
                    liked_user = like.Like,
                    disliked_user = like.Dislike
                }
            }).Skip(page * count).Take(count).ToList();
        }
    }
}