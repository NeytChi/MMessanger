using System;
using Common;
using System.IO;
using System.Linq;
using miniMessanger.Manage;
using miniMessanger.Models;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Collections.Generic;

namespace miniMessanger
{
    public class Chats
    {
        public Context context;
        public Users users;
        public Validator validator;
        public FileSaver system;
        public string savePath;
        public string awsPath;
        public Chats(Context context, Users users, Validator validator)
        {
            Config config = new Config();
            this.context = context;
            this.savePath = config.savePath;
            this.users = users;
            this.awsPath = config.AwsPath;
            this.validator = validator;
            this.system = new FileSaver();
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
        public Message CreateMessage(string messageText, string userToken, string chatToken , ref string answer)
        {
            if (CheckMessageText(ref messageText, ref answer))
            {
                User user = users.GetUserByToken(userToken, ref answer);
                if (user != null)
                {
                    Chatroom room = GetChatroom(chatToken, ref answer);
                    if (room != null)
                    {
                        Message message = SaveTextMessage(room.ChatId, user.UserId, messageText);
                        Log.Info("Create new message with id ->" + message.MessageId + ".", user.UserId);
                        return message;
                    }
                } 
            }
            return null;
        }
        public Message SaveTextMessage(int ChatId, int UserId, string messageText)
        {
            Message message = new Message();
            message.ChatId = ChatId;
            message.UserId = UserId;
            message.MessageType = "text";
            message.MessageText = messageText;
            message.MessageViewed = false;
            message.UrlFile = "";
            message.CreatedAt = DateTime.Now;
            context.Messages.Add(message);
            context.SaveChanges();
            Log.Info("Save new text message.", UserId);
            return message;
        }
        public Chatroom GetChatroom(string ChatToken, ref string message)
        {
            Chatroom room = context.Chatroom.Where(ch 
            => ch.ChatToken == ChatToken).FirstOrDefault();
            if (room == null)
            {
                message = "Server can't define chat by chat_token."; 
            } 
            return room;
        }
        public bool CheckMessageText(ref string messageText, ref string answer)
        {
            if (!string.IsNullOrEmpty(messageText))
            {
                if (messageText.Length < 500)
                {
                    messageText = WebUtility.UrlDecode(messageText);
                    return true;
                }
                messageText = "Message can't be more that 500 characters";
            }
            else
            {
                answer = "Message is empty. Server woundn't upload this message."; 
            }
            return false;
        }
        public Message UploadMessagePhoto(IFormFile photo, ChatCache cache, ref string message)
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
                    message = "Server can't define chat by 'chat_token' key."; 
                }
            }
            return null;
        }
        public Message MessagePhoto(IFormFile photo, int userId, int chatId, ref string answer)
        {
            if (photo != null)
            {
                if (photo.ContentType.Contains("image"))
                {
                    Message message = new Message()
                    {
                        ChatId = chatId,
                        UserId = userId,
                        MessageType = "photo",
                        MessageText = "",
                        MessageViewed = false,
                        UrlFile = system.CreateFile(photo, "/MessagePhoto/"),
                        CreatedAt = DateTime.Now
                    };
                    context.Messages.Add(message);
                    context.SaveChanges();
                    Log.Info("Create photo message, messageId ->" + message.MessageId + ".", userId);
                    return message;
                }
                answer = "Incorrect file type of message's foto.";
            }
            else
            {
                answer = "Input file is null.";
            } 
            return null;
        }
        public dynamic GetMessages(int UserId, string ChatToken, int Page, int Count, ref string answer)
        {
            Chatroom room = GetChatroom(ChatToken, ref answer);
            if (room != null)
            {
                var messages = context.Messages.Where(m 
                => m.ChatId == room.ChatId).OrderByDescending(m => m.MessageId)
                .Skip(Page * Count).Take(Count).Select(m 
                => new 
                { 
                    message_id = m.MessageId,
                    chat_id = m.ChatId,
                    user_id = m.UserId,
                    message_type = m.MessageType,
                    message_text = m.MessageText,
                    url_file = string.IsNullOrEmpty(m.UrlFile) ? "" : awsPath + m.UrlFile,
                    message_viewed = m.MessageViewed,
                    created_at = m.CreatedAt
                }).ToList(); 
                UpdateMessagesToViewed(room.ChatId, UserId);
                Log.Info("Get list of messages, chatId -> " + room.ChatId + ".", UserId); 
                return messages;
            }
            return null;
        }
        public void UpdateMessagesToViewed(int ChatId, int UserId)
        {
            List<Message> data = context.Messages.Where(m
            => m.ChatId == ChatId
            && m.UserId != UserId).ToList();
            data.ForEach(m => m.MessageViewed = true);
            context.SaveChanges();
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