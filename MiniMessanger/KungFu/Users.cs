using System.Linq;
using Common;
using miniMessanger.Models;

namespace miniMessanger.Manage
{
    public class Users
    {
        public Context context;
        public string awsPath;
        public Users(Context context)
        {
            this.context = context;
            this.awsPath = Common.Config.AwsPath;
        }
        public LikeProfiles LikeUser(UserCache cache, ref string message)
        {
            User user = GetUserByToken(cache.user_token, ref message);
            User opposideUser = GetUserByOpposideToken(cache.opposide_public_token, ref message);
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
            User user = GetUserByToken(cache.user_token, ref message);
            User opposideUser = GetUserByOpposideToken(cache.opposide_public_token, ref message);
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
        public User GetUserByToken(string userToken, ref string message)
        {
            if (!string.IsNullOrEmpty(userToken))
            {
                User user = context.User.Where(u => 
                u.UserToken == userToken).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by token.";
                }
                return user;
            }
            return null;
        }
        public User GetUserByPublicToken(string userPublicToken, ref string message)
        {
            User user = context.User.Where(u => u.UserPublicToken == userPublicToken).FirstOrDefault();
            if (user == null)
            {
                message = "Server can't define user by public token";
            }
            return user;
        }
        public bool CheckExistBlocked(int userId, int opposideUserId, ref string message)
        {
            BlockedUser blocked = context.BlockedUsers.Where(b 
            => b.UserId == userId
            && b.BlockedUserId == opposideUserId
            && b.BlockedDeleted == false).FirstOrDefault();
            if (blocked == null)
            {
                message = "User blocked current user."; 
                return false;
            }
            return true;
        }
        public void CreateBlockedUser(int userId,int opposideUserId,string blockedReason )
        {
            BlockedUser blockedUser = new BlockedUser();
            blockedUser.UserId = userId;
            blockedUser.BlockedUserId = opposideUserId;
            blockedUser.BlockedReason = blockedReason;
            blockedUser.BlockedDeleted = false;
            context.BlockedUsers.Add(blockedUser);
            context.SaveChanges();
        }
        public bool CheckComplaintMessage(string complaint, ref string message)
        {
            if (!string.IsNullOrEmpty(complaint))
            {
                if (complaint.Length < 100)
                {
                    return true;
                }
                message = "Complaint can't be more than 100 characters.";
            }
            else
            {
                message = "Complaint message can't be null or empty.";
            }
            return false;
        }
        public User GetUserByOpposideToken(string opposideToken, ref string message)
        {
            if (!string.IsNullOrEmpty(opposideToken))
            {
                User user = context.User.Where(u => 
                u.UserPublicToken == opposideToken).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by token.";
                }
                return user;
            }
            return null;
        }
        public dynamic UserResponse(User user)
        {
            if (user != null)
            {
                return new 
                {
                    user_id = user.UserId,
                    user_email = user.UserEmail,
                    user_login = user.UserLogin,
                    created_at = user.CreatedAt,
                    last_login_at = user.LastLoginAt,
                    user_public_token = user.UserPublicToken
                };        
            }
            return null;
        }
        public void SendConfirmEmail(string UserEmail, string UserHash)
        {
            MailF.SendEmail(UserEmail, "Confirm account", 
            "Confirm account: <a href=http://" + Config.IP + ":" + Config.Port
            + "/v1.0/users/Activate/?hash=" + UserHash + ">Confirm url!</a>");    
        }
        public Profile CreateIfNotExistProfile(int UserId)
        {
            Profile profile = context.Profile.Where(p => p.UserId == UserId).FirstOrDefault();
            if (profile == null)
            {
                profile = new Profile();
                profile.UserId = UserId;
                profile.ProfileGender = true;
                context.Add(profile);
                context.SaveChanges();
            }
            return profile;
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
                    => p.UserId == user.UserId).First();
                }
                return user;
            }
            return null;
        }
        public dynamic ReciprocalUsers(int userId, bool profileGender, int page, int count)
        {
            return (from users in context.User
            join like in context.LikeProfile on users.UserId equals like.ToUserId
            join profile in context.Profile on users.UserId equals profile.UserId
            join blocked in context.BlockedUsers on users.UserId equals blocked.BlockedUserId into blockedUsers
            where like.UserId == userId
            && like.Like 
            && profile.ProfileGender != profileGender 
            && (blockedUsers.All(b => b.UserId == userId && b.BlockedDeleted == true)
            || blockedUsers.Count() == 0)
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