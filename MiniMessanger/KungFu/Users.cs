using Common;
using System;
using System.Net;
using System.Linq;
using miniMessanger.Models;

namespace miniMessanger.Manage
{
    public class Users
    {
        public Context context;
        public string awsPath;
        public Validator validator;
        public Users(Context context, Validator validator)
        {
            this.context = context;
            this.awsPath = Config.AwsPath;
            this.validator = validator;
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
        public bool BlockUser(UserCache cache, ref string message)
        {
            string blockedReason = WebUtility.UrlDecode(cache.blocked_reason);
            User user = GetUserByToken(cache.user_token, ref message);
            if (user != null)
            {
                User interlocutor = GetUserByPublicToken(cache.opposide_public_token, ref message);
                if (interlocutor != null)
                {
                    if (CheckComplaintMessage(blockedReason, ref message))
                    {
                        if (!CheckExistBlocked(user.UserId, interlocutor.UserId, ref message))
                        {
                            CreateBlockedUser(user.UserId, interlocutor.UserId, blockedReason);
                            return true;
                        }
                    }
                }
            }
            return false;
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
    }
}