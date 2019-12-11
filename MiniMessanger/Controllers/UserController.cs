﻿using Common;
using System;
using System.IO;
using System.Net;
using System.Linq;
using miniMessanger;
using Newtonsoft.Json;
using miniMessanger.Models;
using miniMessanger.Manage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Controllers
{
    /// <summary>
    /// User functional for general movement. This class will be generate functional for user ability.
    /// </summary>
    [Route("v1.0/[controller]/[action]/")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private DateTime unixed = new DateTime(1970, 1, 1, 0, 0, 0);
        private Context context;
        private JsonVariableHandler jsonHandler;
        public Users users;
        public Chats chats;
        public Profiles profiles;
        public Authentication authentication;
        public Validator Validator;
        public UsersController(Context context)
        {
            this.context = context;
            this.Validator = new Validator();
            jsonHandler = new Controllers.JsonVariableHandler();
            this.users = new Users(context, Validator);
            this.chats = new Chats(context, users, Validator);
            this.profiles = new Profiles(context);
        }
        [HttpPost]
        [ActionName("Registration")]
        public ActionResult<dynamic> Registration(UserCache cache)
        {
            string message = string.Empty;
            User user = authentication.Registrate(cache, ref message);
            if (user != null)
            {
                return RegistrationResponse(message, user.ProfileToken);
            }
            return Return500Error(message);
        }
        public dynamic RegistrationResponse(string message, string ProfileToken)
        {
            return new 
            { 
                success = true, 
                message = message,
                data = new 
                {
                    profile_token = ProfileToken,
                }
            };
        }
        [HttpPost]
        [ActionName("RegistrationEmail")]
        public ActionResult<dynamic> RegistrationEmail(UserCache cache)
        {
            string message = null;
            if (authentication.ConfirmEmail(cache.user_email, ref message))
            {
                return new 
                {   
                    success = true, 
                    message = "Send confirm email to user." 
                };
            }  
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("Login")]
        public ActionResult<dynamic> Login(UserCache cache)
        {
            string message = null;
            User user = authentication.Login(cache.user_email, cache.user_password, ref message);
            if (user != null)
            {
                return new 
                { 
                    success = true, 
                    data = UserResponse(user)
                };
            }
            return Return500Error(message);
        }
        public dynamic UserResponse(User user)
        {
            if (user != null)
            {
                if (user.Profile != null)
                {
                    return new 
                    { 
                        user_id = user.UserId,
                        user_token = user.UserToken,
                        user_email = user.UserEmail,
                        user_login = user.UserLogin,
                        created_at = user.CreatedAt,
                        last_login_at = user.LastLoginAt,
                        user_public_token = user.UserPublicToken,
                        profile = new
                        {
                            url_photo = user.Profile.UrlPhoto == null ? "" : Config.AwsPath + user.Profile.UrlPhoto,
                            profile_age = user.Profile.ProfileAge == null ? -1 : user.Profile.ProfileAge,
                            profile_gender = user.Profile.ProfileGender,
                            profile_city = user.Profile.ProfileCity == null  ? "" : user.Profile.ProfileCity
                        }    
                    };
                }
            }
            return null;
        }
        [HttpPut]
        [ActionName("LogOut")]
        public ActionResult<dynamic> LogOut(UserCache cache)
        {
            string message = null;
            if (authentication.LogOut(cache.user_token, ref message))
            {
                return new 
                { 
                    success = true, 
                    message = "Log out is successfully." 
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("RecoveryPassword")]
        public ActionResult<dynamic> RecoveryPassword(UserCache cache)
        {
            string message = null;
            if (authentication.RecoveryPassword(cache.user_email, ref message))
            {
                return new 
                { 
                    success = true, 
                    message = "Recovery password. Send message with code to email=" + cache.user_email + "." 
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("CheckRecoveryCode")]
        public ActionResult<dynamic> CheckRecoveryCode(UserCache cache)
        {
            string message = null;
            string RecoveryToken = authentication.CheckRecoveryCode(cache.user_email, cache.recovery_code, ref message);
            if (!string.IsNullOrEmpty(RecoveryToken))
            {
                return new 
                { 
                    success = true, 
                    data = new 
                    { 
                        recovery_token = RecoveryToken 
                    }
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("ChangePassword")]
        public ActionResult<dynamic> ChangePassword(UserCache cache)
        {
            string message = null;
            if (authentication.ChangePassword(
                cache.recovery_token, cache.user_password, 
                cache.user_confirm_password, ref message))
            {
                return new { success = true, message = "Change user password." };
            }
            return Return500Error(message);
        }
        [HttpGet]
        [ActionName("Activate")]
        public ActionResult<dynamic> Activate([FromQuery] string hash)
        {
            string message = null;
            if (authentication.Activate(hash, ref message))
            {
                return new { success = true, message = "User account is successfully active." };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult<dynamic> Delete(UserCache cache)
        { 
            string message = null;
            if (authentication.Delete(cache.user_token, ref message))
            {
                return new { success = true, message = "Account was successfully deleted." };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("UpdateProfile")]
        public ActionResult<dynamic> UpdateProfile(IFormFile profile_photo)
        {
            string message = null;
            string userToken = Request.Form["user_token"];
            User user = users.GetUserByToken(userToken, ref message);
            if (user != null)
            {
                user.Profile = profiles.UpdateProfile(user.UserId, ref message, 
                profile_photo, Request.Form["profile_gender"],
                Request.Form["profile_age"], Request.Form["profile_city"]);
                if (user.Profile != null)
                {    
                    Log.Info("Update profile.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    return new 
                    { 
                        success = true, 
                        data = ProfileToResponse(user.Profile)
                    };
                }
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("RegistrateProfile")]
        public ActionResult<dynamic> RegistrateProfile(IFormFile profile_photo)
        {
            string message = null;
            string profileToken = Request.Form["profile_token"];
            User user = context.User.Where(u => u.ProfileToken == profileToken.ToString()).FirstOrDefault();
            if (user != null)
            {
                user.Profile = profiles.UpdateProfile(user.UserId, ref message, 
                profile_photo, Request.Form["profile_gender"],
                Request.Form["profile_age"], Request.Form["profile_city"]);
                if (user.Profile != null)
                {
                    Log.Info("Registrate new profile.", user.UserId);
                    return new 
                    { 
                        success = true, 
                        message = "User account was successfully registered. See your email to activate account by link.",
                        data = ProfileToResponse(user.Profile)
                    };
                }
            }
            else 
            { 
                message = "No user with that profile_token."; 
            }
            return Return500Error(message);
        }
        public dynamic ProfileToResponse(Profile profile)
        {
            return new 
            {
                url_photo = profile.UrlPhoto == null ? "" : Config.savePath + profile.UrlPhoto,
                profile_age = profile.ProfileAge == null ? -1 : profile.ProfileAge,
                profile_gender = profile.ProfileGender,
                profile_city = profile.ProfileCity == null  ? "" : profile.ProfileCity
            };
        }
        [HttpPost]
        [ActionName("Profile")]
        public ActionResult<dynamic> Profile(UserCache userCache)
        {
            string message = null;
            User user = users.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                user.Profile = authentication.CreateIfNotExistProfile(user.UserId);
                Log.Info("Select profile.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                return new 
                { 
                    success = true, 
                    data = ProfileToResponse(user.Profile)
                };
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("GetUsersList")]
        public ActionResult<dynamic> GetUsersList(UserCache cache)
        {
            string message = null;
            User user = users.GetUserByToken(cache.user_token, ref message);
            if (user != null)
            {
                List<dynamic> data = new List<dynamic>();  
                
                List<User> usersData = context.User.Where(u 
                => u.UserId != user.UserId)
                .OrderByDescending(u => u.UserId)
                .Skip(cache.page * 30).Take(30).ToList();
                
                List<int> blocked = context.BlockedUsers.Where(b 
                => b.UserId == user.UserId 
                && b.BlockedDeleted == false)
                .Select(b => b.BlockedUserId).ToList();
                
                foreach(User publicUser in usersData)
                {
                    if (!blocked.Contains(publicUser.UserId))
                    {
                        data.Add(users.UserResponse(publicUser));
                    }
                }
                Log.Info("Get users list.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId); 
                context.SaveChanges();
                return new { success = true, data = data };
            }
            return Return500Error(message);
        }
        /// <summary>
        /// Select list of chats. Get last message data, user's data of chat and chat data.
        /// </summary>
        /// <param name="request">Request.</param>
        [HttpPut]
        [ActionName("SelectChats")]
        public ActionResult<dynamic> SelectChats(UserCache userCache)
        {
            string message = null;
            User user = users.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                List<dynamic> chats = new List<dynamic>();

                List<Participants> participants = context.Participants.Where(p 
                => p.UserId == user.UserId).ToList();

                List<int> blocked = context.BlockedUsers.Where(b 
                => b.UserId == user.UserId 
                && b.BlockedDeleted == false)
                .Select(b => b.BlockedUserId).ToList();

                foreach(Participants participant in participants)
                {
                    if (!blocked.Contains(participant.OpposideId))
                    {
                        Chatroom room = context.Chatroom.Where(ch 
                        => ch.ChatId == participant.ChatId).First();

                        User opposide = context.User.Where(u 
                        => u.UserId == participant.OpposideId).First();
                        
                        Message lastMessage = context.Messages.Where(m 
                        => m.ChatId == room.ChatId)
                        .OrderByDescending(m => m.MessageId).FirstOrDefault();

                        var unit = new  
                        {
                            user = new 
                            {
                                user_id = opposide.UserId,
                                user_email = opposide.UserEmail,
                                user_public_token = opposide.UserPublicToken,
                                user_login = opposide.UserLogin,
                                last_login_at = opposide.LastLoginAt,
                            },
                            chat = new 
                            {
                                chat_id = room.ChatId,
                                chat_token = room.ChatToken,
                                created_at = room.CreatedAt,
                            },
                            last_message = lastMessage == null ? null : new
                            {
                                message_id = lastMessage.MessageId,
                                chat_id = lastMessage.ChatId,
                                user_id = lastMessage.UserId,
                                message_text = lastMessage.MessageText,
                                message_viewed = lastMessage.MessageViewed,
                                created_at = lastMessage.CreatedAt
                            }
                        };
                        chats.Add(unit);
                    }
                }
                Log.Info("Get list of chats.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId); 
                context.SaveChanges();
                return new { success = true, data = chats };
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("SelectMessages")]
        public ActionResult<dynamic> SelectMessages(UserCache userCache)
        {
            string messageReturn = null;
            User user = users.GetUserByToken(userCache.user_token, ref messageReturn);
            if (user != null)
            {
                Chatroom room = context.Chatroom.Where(r 
                => r.ChatToken == userCache.chat_token).FirstOrDefault();
                if (room != null)
                {
                    var messages = context.Messages.Where(m 
                    => m.ChatId == room.ChatId)
                    .OrderByDescending(m => m.MessageId)
                    .Skip(userCache.page * 50).Take(50)
                    .Select(m 
                    => new 
                    { 
                        message_id = m.MessageId,
                        chat_id = m.ChatId,
                        user_id = m.UserId,
                        message_type = m.MessageType,
                        message_text = m.MessageText,
                        url_file = string.IsNullOrEmpty(m.UrlFile) ? "" : Config.AwsPath + m.UrlFile,
                        message_viewed = m.MessageViewed,
                        created_at = m.CreatedAt
                    }).ToList(); 
                    var data = (from m in context.Messages 
                    where m.ChatId == room.ChatId 
                    && m.UserId != user.UserId 
                    select m).ToList();
                    data.ForEach(m => m.MessageViewed = true);
                    context.SaveChanges();
                    Log.Info("Get list of messages, chat_id->" + room.ChatId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId); 
                    return new { success = true, data = messages };
                }
                else 
                { 
                    messageReturn = "Server can't define chat by chat_token."; 
                }
            }
            return Return500Error(messageReturn);
        }
        [HttpPost]
        [ActionName("CreateChat")]
        public ActionResult<dynamic> CreateChat(UserCache cache)
        {
            string message = null;
            Chatroom room = chats.CreateChat(cache.user_token, cache.opposide_public_token, ref message);
            if (room != null)
            {
                return new 
                { 
                    success = true, 
                    data = new 
                    {
                        chat_id = room.ChatId,
                        chat_token = room.ChatToken,
                        created_at = room.CreatedAt 
                    } 
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("SendMessage")]
        public ActionResult<dynamic> SendMessage(UserCache userCache)
        {
            string answer = null;
            if (!string.IsNullOrEmpty(userCache.message_text))
            {
                string messageText = WebUtility.UrlDecode(userCache.message_text);
                User user = users.GetUserByToken(userCache.user_token, ref answer);
                if (user != null)
                {
                    Chatroom room = context.Chatroom.Where(ch 
                    => ch.ChatToken == userCache.chat_token).FirstOrDefault();
                    if (room != null)
                    {
                        Message message = new Message();
                        message.ChatId = room.ChatId;
                        message.UserId = user.UserId;
                        message.MessageType = "text";
                        message.MessageText = messageText;
                        message.MessageViewed = false;
                        message.UrlFile = "";
                        message.CreatedAt = DateTime.Now;
                        context.Messages.Add(message);
                        context.SaveChanges();
                        Log.Info("Message was handled, message_id->" + message.MessageId + ".", 
                        HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        return new 
                        { 
                            success = true, 
                            data = chats.ResponseMessage(message)
                        };
                    } 
                    else 
                    { 
                        answer = "Server can't define chat by chat_token."; 
                    }
                } 
            } 
            else 
            {
                answer = "Message is empty. Server woundn't upload this message."; 
            }        
            return Return500Error(answer);
        }
        [HttpPost]
        [ActionName("MessagePhoto")]
        public ActionResult<dynamic> MessagePhoto(IFormFile photo)
        {
            string message = null;
            string data = Request.Form["data"];
            if (!string.IsNullOrEmpty(data))
            {
                UserCache cache  = JsonConvert.DeserializeObject<UserCache>(data);
                Message result = chats.UploadMessagePhoto(photo, cache, ref message);
                if (result != null)
                {
                    return new { success = true, data = chats.ResponseMessage(result) };
                }
            } 
            else 
            {
                message = "Input field 'data' is null or empty."; 
            }        
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("BlockUser")]
        public ActionResult<dynamic> BlockUser(UserCache cache)
        {
            string message = null;
            if (users.BlockUser(cache, ref message))
            {
                Log.Info("Block user.", HttpContext.Connection.RemoteIpAddress.ToString());
                return new { success = true, message = "Block user - successed." };
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("GetBlockedUsers")]
        public ActionResult<dynamic> GetBlockedUsers(UserCache cache)
        {
            var blockedUsers = (from user in context.User 
            join blocked in context.BlockedUsers on user.UserId equals blocked.UserId
            where user.UserToken == cache.user_token
            && blocked.BlockedDeleted == false
            select new
            { 
                user_email = blocked.Blocked.UserEmail,
                user_login =  blocked.Blocked.UserLogin,
                last_login_at = blocked.Blocked.LastLoginAt,
                user_public_token = blocked.Blocked.UserPublicToken,
                blocked_reason = blocked.BlockedReason
            }
            ).ToList();
            Log.Info("Get blocked users.", HttpContext.Connection.RemoteIpAddress.ToString());
            return new 
            { 
                success = true, 
                data = blockedUsers 
            };
        }
        [HttpPost]
        [ActionName("UnblockUser")]
        public ActionResult<dynamic> UnblockUser(UserCache userCache)
        {
            string message = null;
            User user = users.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                User interlocutor = context.User.Where(u 
                => u.UserPublicToken == userCache.opposide_public_token).FirstOrDefault();
                if (interlocutor != null)
                {
                    BlockedUser blockedUser = context.BlockedUsers.Where(b => b.UserId == user.UserId 
                    && b.BlockedUserId == interlocutor.UserId && b.BlockedDeleted == false).FirstOrDefault();
                    if (blockedUser != null)
                    {
                        blockedUser.BlockedDeleted = true;
                        context.BlockedUsers.UpdateRange(blockedUser);
                        context.SaveChanges();
                        Log.Info("Delete blocked user; user->user_id->" + user.UserId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        return new { success = true, message = "Unblock user - successed." };
                    }
                    else 
                    { 
                        message = "User didn't block current user; user->user_id->" + user.UserId + "."; 
                    }
                }
                else 
                { 
                    message = "No user with that opposide_public_token; user->user_id->" + user.UserId + "."; 
                }
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("ComplaintContent")]
        public ActionResult<dynamic> ComplaintContent(UserCache userCache)
        {
            string message = null;   
            string complaint = WebUtility.UrlDecode(userCache.complaint);
            User user = users.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Message messageChat = context.Messages.Where(m 
                => m.MessageId == userCache.message_id).FirstOrDefault();
                if (messageChat != null)
                {
                    if (complaint.Length < 100)
                    {
                        if (messageChat.UserId != user.UserId)
                        {
                            User interlocutor = context.User.Where(u => u.UserId == messageChat.UserId).FirstOrDefault();
                            BlockedUser blockedUser = context.BlockedUsers.Where(b => b.UserId == user.UserId 
                            && b.BlockedUserId == interlocutor.UserId && b.BlockedDeleted == false).FirstOrDefault();
                            if (blockedUser == null)
                            {
                                blockedUser = new BlockedUser();
                                blockedUser.UserId = user.UserId;
                                blockedUser.BlockedUserId = interlocutor.UserId;
                                blockedUser.BlockedReason = complaint;
                                blockedUser.BlockedDeleted = false;
                                context.BlockedUsers.Add(blockedUser);
                                Complaints complaintUser = new Complaints();
                                complaintUser.UserId = user.UserId;
                                complaintUser.BlockedId = blockedUser.BlockedId;
                                complaintUser.MessageId = messageChat.MessageId;
                                complaintUser.Complaint = complaint;
                                complaintUser.CreatedAt = System.DateTime.Now;
                                context.Complaints.Add(complaintUser);
                                context.SaveChanges();
                                Log.Info("Create complaint; user->user_id->" + user.UserId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                                return new { success = true, message = "Complain content - successed." };
                            }
                            else 
                            { 
                                message = "User blocked current user."; 
                            }
                        }
                        else 
                        { 
                            message = "User can't complain on himself."; 
                        }
                    }
                    else 
                    { 
                        message = "Complaint message can't be longer than 100 characters."; 
                    }
                }
                else 
                { 
                    message = "Unknow message_id. Server can't define message."; 
                }
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("GetUsersByGender")]
        public ActionResult<dynamic> GetUsersByGender(UserCache cache)
        {
            string message = null;
            int count = cache.count == 0 ? 30 : cache.count;
            var userData = users.GetUserWithProfile(cache.user_token, ref message);
            if (userData != null)
            {
                var usersData = (from users in context.User
                join profile in context.Profile on users.UserId equals profile.UserId
                join likesProfile in context.LikeProfile on users.UserId equals likesProfile.ToUserId into likes
                join blockedUser in context.BlockedUsers on users.UserId equals blockedUser.BlockedUserId into blockedUsers
                where users.UserId != userData.UserId && profile.ProfileGender != userData.Profile.ProfileGender
                && users.Activate == 1 && (!likes.Any(l => l.UserId == userData.UserId && l.Like))
                && (blockedUsers.All(b => b.UserId == userData.UserId && b.BlockedDeleted == true)
                || blockedUsers.Count() == 0)
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
                        url_photo = profile.UrlPhoto == null ? "" : Config.AwsPath + profile.UrlPhoto,
                        profile_age = profile.ProfileAge == null ? -1 : profile.ProfileAge,
                        profile_gender = profile.ProfileGender,
                        profile_city = profile.ProfileCity == null  ? "" : profile.ProfileCity
                    },
                    liked_user = likes.Any(l => l.Like) ? true : false,
                    disliked_user = likes.Any(l => l.Dislike) ? true : false
                }).Skip(cache.page * 30).Take(30).ToList();
                Log.Info("Get users list.", HttpContext.Connection.RemoteIpAddress.ToString(), userData.UserId);
                context.SaveChanges();
                return new { success = true, data = usersData };
            }
            return Return500Error(message);
        }
        /// <summary>
        /// Select list of chats. Get last message data, user's data of chat and chat data.
        /// </summary>
        /// <param name="request">Request.</param>
        [HttpPut]
        [ActionName("SelectChatsByGender")]
        public ActionResult<dynamic> SelectChatsByGender(UserCache cache)
        {
            string message = null;
            int count = cache.count == 0 ? 30 : cache.count;
            var user = users.GetUserWithProfile(cache.user_token, ref message);
            if (user != null)
            {
                var data = (
                from participant in context.Participants
                join users in context.User on participant.OpposideId equals users.UserId
                join profile in context.Profile on users.UserId equals profile.UserId
                join chats in context.Chatroom on participant.ChatId equals chats.ChatId
                join likesProfile in context.LikeProfile on users.UserId equals likesProfile.ToUserId into likes
                join messageChat in context.Messages on chats.ChatId equals messageChat.ChatId into messages
                join blockedUser in context.BlockedUsers on users.UserId equals blockedUser.BlockedUserId into blockedUsers
                where participant.UserId == user.UserId && profile.ProfileGender != user.Profile.ProfileGender
                && (blockedUsers.All(b => b.UserId == user.UserId && b.BlockedDeleted == true)
                || blockedUsers.Count() == 0) && users.Activate == 1
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
                        chat_id = participant.ChatId,
                        profile = new 
                        {
                            url_photo = profile.UrlPhoto == null ? "" : Config.AwsPath + profile.UrlPhoto,
                            profile_age = profile.ProfileAge == null ? -1 : profile.ProfileAge,
                            profile_gender = profile.ProfileGender,
                            profile_city = profile.ProfileCity == null  ? "" : profile.ProfileCity
                        }
                    },
                    chat = new 
                    {
                        chat_id = chats.ChatId,
                        chat_token = chats.ChatToken,
                        created_at = chats.CreatedAt,
                    },
                    last_message = messages == null || messages.Count() == 0 ? null : new 
                    {
                        message_id = messages.ToList()[messages.Count() - 1].MessageId,
                        chat_id = messages.ToList()[messages.Count() - 1].ChatId,
                        user_id = messages.ToList()[messages.Count() - 1].UserId,
                        message_text = messages.ToList()[messages.Count() - 1].MessageText,
                        message_viewed = messages.ToList()[messages.Count() - 1].MessageViewed,
                        created_at = messages.ToList()[messages.Count() - 1].CreatedAt
                    } ,
                    liked_user = likes.Any(l => l.Like) ? true : false,
                    disliked_user = likes.Any(l => l.Dislike) ? true : false
                }).Skip(cache.page * count).Take(count).ToList();           
                Log.Info("Get list of chats.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId); 
                context.SaveChanges();
                return new { success = true, data = data };
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("ReciprocalUsers")]
        public ActionResult<dynamic> ReciprocalUsers(UserCache cache)
        {
            string message = null;
            int count = cache.count == 0 ? 30 : cache.count;
            User user = users.GetUserWithProfile(cache.user_token, ref message);
            if (user != null)
            {
                dynamic data = chats.ReciprocalUsers(user.UserId, user.Profile.ProfileGender, cache.page, count);
                Log.Info("Get reciprocal users.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId); 
                return new { success = true, data = data };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("LikeUsers")]
        public ActionResult<dynamic> LikeUnlikeUsers(UserCache cache)
        {
            string message = null;
            LikeProfiles like = users.LikeUser(cache, ref message);
            if (like != null)
            {
                return new 
                { 
                    success = true,
                    data = new 
                    {
                        disliked_user = like.Dislike,
                        liked_user = like.Like
                    }
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("DislikeUsers")]
        public ActionResult<dynamic> DislikeUsers(UserCache cache)
        {
            string message = null;
            LikeProfiles dislike = users.DislikeUser(cache, ref message);
            if (dislike != null)
            {
                return new 
                { 
                    success = true,
                    data = new 
                    {
                        disliked_user = dislike.Dislike,
                        liked_user = dislike.Like
                    }
                };
            } return Return500Error(message);
        }
        public dynamic Return500Error(string message)
        {
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
        }
    }
}