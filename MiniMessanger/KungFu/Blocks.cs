using Common;
using System.Net;
using System.Linq;
using miniMessanger.Models;

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
                    if (CheckComplaintMessage(BlockedReason, ref message))
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
        public void CreateBlockedUser(int userId,int opposideUserId, string blockedReason)
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
    }
}