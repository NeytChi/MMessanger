using Common;
using System.Net;
using System.Linq;
using miniMessanger.Models;
using System;

namespace miniMessanger.Manage
{
    public class Blocks
    {
        public Users users;
        public Context context;
        public Blocks(Users users, Context context)
        {
            this.users = users;
            this.context = context;
        }
        public Blocks(Users users)
        {
            this.users = users;
            this.context = new Context(true);
        }
        public bool BlockUser(string UserToken, string OpposidePublicToken, string BlockedReason, ref string message)
        {
            BlockedReason = WebUtility.UrlDecode(BlockedReason);
            User user = users.GetUserByToken(UserToken, ref message);
            if (user != null)
            {
                User interlocutor = users.GetUserByPublicToken(OpposidePublicToken, ref message);
                if (interlocutor != null)
                {
                    if (CheckComplaintMessage(ref BlockedReason, ref message))
                    {
                        if (GetExistBlocked(user.UserId, interlocutor.UserId, ref message) == null)
                        {
                            CreateBlockedUser(user.UserId, interlocutor.UserId, BlockedReason);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public BlockedUser GetExistBlocked(int userId, int opposideUserId, ref string message)
        {
            BlockedUser blocked = context.BlockedUsers.Where(b 
            => b.UserId == userId
            && b.BlockedUserId == opposideUserId
            && b.BlockedDeleted == false).FirstOrDefault();
            if (blocked == null)
            {
                message = "User did block current user."; 
            }
            return blocked;
        }
        public BlockedUser CreateBlockedUser(int userId,int opposideUserId, string blockedReason)
        {
            BlockedUser blockedUser = new BlockedUser();
            blockedUser.UserId = userId;
            blockedUser.BlockedUserId = opposideUserId;
            blockedUser.BlockedReason = blockedReason;
            blockedUser.BlockedDeleted = false;
            context.BlockedUsers.Add(blockedUser);
            context.SaveChanges();
            return blockedUser;
        }
        public bool CheckComplaintMessage(ref string complaint, ref string message)
        {
            if (!string.IsNullOrEmpty(complaint))
            {
                if (complaint.Length < 100)
                {
                    WebUtility.UrlDecode(complaint);
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
        public bool UnblockUser(string userToken, string opposidePublicToken, ref string message)
        {
            User user = users.GetUserByToken(userToken, ref message);
            if (user != null)
            {
                User interlocutor = users.GetUserByPublicToken(opposidePublicToken, ref message);
                if (interlocutor != null)
                {
                    BlockedUser blocked = GetExistBlocked(user.UserId, interlocutor.UserId, ref message);
                    if (blocked != null)
                    {
                        blocked.BlockedDeleted = true;
                        context.BlockedUsers.UpdateRange(blocked);
                        context.SaveChanges();
                        Log.Info("Unblock user; blockedId ->" + blocked.BlockedId + ".", user.UserId);
                        return true;
                    }
                }
            }
            return false;
        }
        public bool Complaint(string userToken, long messageId, string complaint, ref string message)
        {
            User user = users.GetUserByToken(userToken, ref message);
            if (user != null)
            {
                Message messageChat = context.Messages.Where(m => m.MessageId == messageId).FirstOrDefault();
                if (messageChat != null)
                {
                    if (CheckComplaintMessage(ref complaint, ref message))
                    {
                        if (messageChat.UserId != user.UserId)
                        {
                            User interlocutor = context.User.Where(u => u.UserId == messageChat.UserId).FirstOrDefault();
                            if (GetExistBlocked(user.UserId, interlocutor.UserId, ref message) == null)
                            {
                                BlockedUser block = CreateBlockedUser(user.UserId, interlocutor.UserId, complaint);
                                CreateComplaint(user.UserId, block.BlockedId, messageChat.MessageId, complaint);
                                Log.Info("Create complaint; user->user_id->" + user.UserId + ".", user.UserId);
                                return true;
                            }
                        }
                        else 
                        { 
                            message = "User can't complain on himself."; 
                        }
                    }
                }
                else 
                { 
                    message = "Server can't define message by message_id."; 
                }
            }
            return false;
        }
        public Complaint CreateComplaint(int UserId, int BlockId, long MessageId, string complaintText)
        {
            Complaint complaint = new Complaint()
            {
                UserId = UserId,
                BlockId = BlockId,
                MessageId = MessageId,
                ComplaintText = complaintText,
                CreatedAt = DateTime.Now
            };
            context.Complaint.Add(complaint);
            context.SaveChanges();
            return complaint;
        }
        public dynamic GetBlockedUsers(int UserId, int Page, int Count = 50)
        {
            var blockedUsers = (from user in context.User 
            join blocked in context.BlockedUsers on user.UserId equals blocked.BlockedUserId
            where blocked.UserId == UserId
            && blocked.BlockedDeleted == false
            orderby user.UserId
            select new
            { 
                block_id = blocked.BlockedId,
                user_id = blocked.BlockedUserId,
                user_email = user.UserEmail,
                user_login =  user.UserLogin,
                last_login_at = user.LastLoginAt,
                user_public_token = user.UserPublicToken,
                blocked_reason = blocked.BlockedReason
            }
            ).Skip(Page * Count).Take(Count).ToList();
            Log.Info("Get blocked users.");
            return blockedUsers;
        }
    }
}