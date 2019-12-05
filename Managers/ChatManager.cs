using System;
using Common;
using System.IO;
using miniMessanger.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace miniMessanger
{
    public class ChatManager
    {
        public MMContext context;
        public UserManager userManager;
        public string savePath;
        public string awsPath;
        public ChatManager(MMContext context, UserManager userManager)
        {
            this.context = context;
            this.savePath = Common.Config.savePath;
            this.userManager = userManager;
            this.awsPath = Common.Config.AwsPath;
        }
        public Messages UploadMessagePhoto(IFormFile photo, UserCache cache, ref string message)
        {
            Users user = userManager.GetUserByToken(cache.user_token, ref message);
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
        public Messages MessagePhoto(IFormFile photo, int userId, int chatId, ref string answer)
        {
            if (photo != null)
            {
                Messages message = new Messages();
                if (photo.ContentType.Contains("image"))
                {
                    Directory.CreateDirectory(savePath + "/MessagePhoto/" 
                    + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day);
                    string url = "/MessagePhoto/" + DateTime.Now.Year + "-" + DateTime.Now.Month 
                    + "-" + DateTime.Now.Day + "/" + Validator.GenerateHash(10);
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
        public dynamic ResponseMessage(Messages message)
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