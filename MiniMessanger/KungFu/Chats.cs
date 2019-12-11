using System;
using Common;
using System.IO;
using System.Linq;
using miniMessanger.Manage;
using miniMessanger.Models;
using Microsoft.AspNetCore.Http;

namespace miniMessanger
{
    public class Chats
    {
        public Context context;
        public Users users;
        public Validator validator;
        public string savePath;
        public string awsPath;
        public Chats(Context context, Users users, Validator validator)
        {
            this.context = context;
            this.savePath = Config.savePath;
            this.users = users;
            this.awsPath = Config.AwsPath;
            this.validator = validator;
        }
        public Chatroom CreateChat(string userToken, string publicToken, ref string message)
        {
            User user = users.GetUserByToken(userToken, ref message);
            if (user != null)
            {
                User interlocutor = context.User.Where(u => u.UserPublicToken == publicToken).FirstOrDefault();
                if (interlocutor != null)
                {
                    return CreateChatIfNotExist(user, interlocutor);
                } 
                else 
                { 
                    message = "Can't define interlocutor by interlocutor_public_token from request's body."; 
                }
            } 
            return null;
        }
        public Chatroom CreateChatIfNotExist(User user, User interlocutor)
        {
            Chatroom room;
            Participants participant = context.Participants.Where(p 
            => p.UserId == user.UserId 
            && p.OpposideId == interlocutor.UserId).FirstOrDefault();
            if (participant == null)
            {
                room = SaveChat();
                SaveParticipants(room.ChatId,  user.UserId, interlocutor.UserId);
                SaveParticipants(room.ChatId, interlocutor.UserId, user.UserId);
                Log.Info("Create chat for userId ->" + user.UserId + ".", user.UserId);
            }
            else
            {
                room = context.Chatroom.Where(ch => ch.ChatId == participant.ChatId).First();
                Log.Info("Select chat for userId ->" + user.UserId + ".", user.UserId);
            }
            return room;
        }
        public Chatroom SaveChat()
        {
            Chatroom room = new Chatroom();
            room.ChatToken = users.validator.GenerateHash(20);
            room.CreatedAt = DateTime.Now;
            context.Chatroom.Add(room);
            context.SaveChanges();
            Log.Info("Save new chat.");
            return room;
        }
        public void SaveParticipants(int chatId, int userId, int opposideUserId)
        {
            Participants participant = new Participants();
            participant.ChatId = chatId;
            participant.UserId = userId;
            participant.OpposideId = opposideUserId;
            context.Participants.Add(participant);
            context.SaveChanges();
            Log.Info("Create and save new participants.");
        }
        public Message UploadMessagePhoto(IFormFile photo, UserCache cache, ref string message)
        {
            User user = users.GetUserByToken(cache.user_token, ref message);
            if (user != null)
            {
                Chatroom room = context.Chatroom.Where(ch 
                => ch.ChatToken == cache.chat_token).FirstOrDefault();
                if (room != null)
                {
                    return MessagePhoto(photo, user.UserId, room.ChatId, ref message);
                } 
                else 
                { 
                    message = "Server can't define chat by chat_token."; 
                }
            }
            return null;
        }
        public Message MessagePhoto(IFormFile photo, int userId, int chatId, ref string answer)
        {
            if (photo != null)
            {
                Message message = new Message();
                if (photo.ContentType.Contains("image"))
                {
                    Directory.CreateDirectory(savePath + "/MessagePhoto/" 
                    + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day);
                    string url = "/MessagePhoto/" + DateTime.Now.Year + "-" + DateTime.Now.Month 
                    + "-" + DateTime.Now.Day + "/" + validator.GenerateHash(10);
                    photo.CopyTo(new FileStream(Config.savePath + url, FileMode.Create));
                    message.ChatId = chatId;
                    message.UserId = userId;
                    message.MessageType = "photo";
                    message.MessageText = "";
                    message.MessageViewed = false;
                    message.UrlFile = url;
                    message.CreatedAt = DateTime.Now;
                    context.Messages.Add(message);
                    context.SaveChanges();
                    Log.Info("Create photo message, messageId ->" + message.MessageId + ".", userId);
                    return message;
                }
                else
                {
                    answer = "Incorrect file type of message's foto.";
                }
            }
            else
            {
                answer = "Input file is null.";
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
        public void DeleteFile(string relativePath)
        {
            if (File.Exists(Config.savePath + relativePath))
            {
                File.Delete(Config.savePath + relativePath);
                Log.Info("Delete file ->" + relativePath + ".");
            }
        }
        public string CreateFile(IFormFile file, string relativePath)
        {
            Directory.CreateDirectory(Config.savePath + relativePath +
                DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day);
            string UrlPhoto = relativePath + DateTime.Now.Year + "-" + DateTime.Now.Month 
            + "-" + DateTime.Now.Day + "/" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            file.CopyTo(new FileStream(Config.savePath + UrlPhoto, FileMode.Create));
            return UrlPhoto;
        }
        
        public dynamic ResponseMessage(Message message)
        {
            if (message != null)
            {
                return new
                {
                    message_id = message.MessageId,
                    chat_id = message.ChatId,
                    user_id = message.UserId,
                    message_type = message.MessageType,
                    message_text = message.MessageText,
                    url_file = string.IsNullOrEmpty(message.UrlFile) ? "" : awsPath + message.UrlFile,
                    message_viewed = message.MessageViewed,
                    created_at = message.CreatedAt
                };
            }
            return null;
        }  
    }
}