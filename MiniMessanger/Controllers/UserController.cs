using System;
using System.IO;
using System.Net;
using Controllers;
using System.Linq;
using miniMessanger;
using Newtonsoft.Json;
using miniMessanger.Models;
using miniMessanger.Manage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using miniMessanger.Authentication;

namespace Common.Functional.UserF
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
        public Authentication authentication;
        public Validator Validator;
        public UsersController(Context context)
        {
            this.context = context;
            this.Validator = new Validator();
            jsonHandler = new Controllers.JsonVariableHandler();
            this.users = new Users(context, Validator);
            this.chats = new Chats(context, users, Validator);
        }
        [HttpPost]
        [ActionName("Registration")]
        public ActionResult<dynamic> Registration(UserCache cache)
        {
            string message = string.Empty;
            dynamic response = authentication.Registrate(cache, ref message);
            return response != null ? response : Return500Error(message);
        }
        
        [HttpPost]
        [ActionName("RegistrationEmail")]
        public ActionResult<dynamic> RegistrationEmail(UserCache cache)
        {
            string message = null;
            User user = users.GetUserByEmail(cache.user_email, ref message);
            if (user != null)
            {
                if (!user.Deleted)
                {                            
                    users.SendConfirmEmail(user.UserEmail, user.UserHash);
                    Log.Info("Send registration email to user.", 
                        HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    return new 
                    {   
                        success = true, 
                        message = "Send confirm email to user." 
                    };
                }
                else 
                { 
                    message = "Unknow email -> " + user.UserEmail + "."; 
                }
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("Login")]
        public ActionResult<dynamic> Login(UserCache cache)
        {
            string message = null;
            User user = users.GetUserByEmail(cache.user_email, ref message);
            if (user != null)
            {
                if (Validator.VerifyHashedPassword(user.UserPassword, cache.user_password))
                {
                    if (user.Activate == 1 && user.Deleted == false)
                    {
                        user.LastLoginAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        context.User.Update(user);
                        context.SaveChanges();
                        user.Profile = users.CreateIfNotExistProfile(user.UserId);
                        Log.Info("User login.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        return new 
                        { 
                            success = true, 
                            data = UserResponse(user)
                        };
                    }
                    users.SendConfirmEmail(user.UserEmail, user.UserHash);
                    message =  "User's account isn't confirmed."; 
                }
                else 
                { 
                    message = "Wrong password."; 
                }
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
        public ActionResult<dynamic> LogOut(UserCache userCache)
        {
            string message = null;
            User user = users.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                user.UserToken = Validator.GenerateHash(40);
                context.User.Update(user);
                context.SaveChanges();
                Log.Info("User log out.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                return new { success = true, message = "Log out is successfully." };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("RecoveryPassword")]
        public ActionResult<dynamic> RecoveryPassword(UserCache cache)
        {
            string message = null;
            User user = authentication.GetUserByEmail(cache.user_email, ref message);
            if (user != null)
            {
                if (!user.Deleted && user.Activate == 1)
                {
                    user.RecoveryCode = Validator.random.Next(100000, 999999);
                    MailF.SendEmail(user.UserEmail, "Recovery password", "Recovery code=" + user.RecoveryCode);
                    context.User.Update(user);
                    context.SaveChanges();
                    Log.Info("Recovery password.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    return new 
                    { 
                        success = true, 
                        message = "Recovery password. Send message with code to email=" + user.UserEmail + "." 
                    };
                }
                else
                {
                    message = "User account is non activate.";
                }
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("CheckRecoveryCode")]
        public ActionResult<dynamic> CheckRecoveryCode(UserCache cache)
        {
            string message = null;
            User user = authentication.GetUserByEmail(cache.user_email, ref message);
            if (user != null)
            {
                if (!user.Deleted && user.Activate == 1)
                {
                    if (user.RecoveryCode == cache.recovery_code)
                    {
                        user.RecoveryToken = Validator.GenerateHash(40);
                        user.RecoveryCode = 0;
                        context.User.Update(user);
                        context.SaveChanges();
                        Log.Info("Check recovery code - successed.", 
                        HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        return new 
                        { 
                            success = true, 
                            data = new 
                            { 
                                recovery_token = user.RecoveryToken 
                            }
                        };
                    }
                    else 
                    {
                        message = "Wrong code."; 
                    }
                }
                else
                {
                    message = "User account is non activate.";
                }
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("ChangePassword")]
        public ActionResult<dynamic> ChangePassword(UserCache cache)
        {
            string message = null;
            User user = context.User.Where(u
            => u.RecoveryToken == cache.recovery_token
            && !u.Deleted 
            && u.Activate == 1).FirstOrDefault();
            if (user != null)
            {
                if (cache.user_password.Equals(cache.user_confirm_password))
                {
                    if (Validator.ValidatePassword(cache.user_password, ref message))
                    {
                        user.UserPassword = Validator.HashPassword(cache.user_password);
                        user.RecoveryToken  = "";
                        context.User.Update(user);
                        context.SaveChanges();
                        Log.Info("Change user password.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        return new { success = true, message = "Change user password, user_id=" + user.UserId + "." };
                    }
                    else 
                    { 
                        message = "Validation password - unsuccessfully. " + message; 
                    }
                }
                else 
                { 
                    message = "Password are not match to each other."; 
                }
            }
            return Return500Error(message);
        }
        [HttpGet]
        [ActionName("Activate")]
        public ActionResult<dynamic> Activate([FromQuery] string hash)
        {
            string message = null;
            User user = context.User.Where(u => u.UserHash == hash
            && !u.Deleted).FirstOrDefault();
            if (user != null)
            {
                user.Activate = 1;
                context.User.Update(user);
                context.SaveChanges();
                Log.Info("Active user account.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                return new { success = true, message = "User account is successfully active." };
            }
            else 
            { 
                message = "Can't activate account. Unknow hash in request parameters."; 
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult<dynamic> Delete(UserCache userCache)
        { 
            string message = null;
            User user = users.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                user.Deleted = true;
                user.UserToken = null;
                context.User.Update(user);
                context.SaveChanges();
                Log.Info("Account was successfully deleted.", 
                    HttpContext.Connection.RemoteIpAddress.ToString(), 
                    user.UserId); 
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
            if (userToken != null)
            {
                User user = users.GetUserByToken(userToken, ref message);
                if (user != null)
                {
                    Profile profile = authentication.CreateIfNotExistProfile(user.UserId);
                    string profileGender = Request.Form["profile_gender"];
                    if (profileGender != null)
                    {
                        if (profileGender == "1")
                        {
                            profile.ProfileGender = true;
                            Log.Info("Update profile gender.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                        else if (profileGender == "0")
                        {
                            profile.ProfileGender = false;
                            Log.Info("Update profile gender.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                        else
                        {
                            Log.Warn("Incorrect value to uUpdate profile gender.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                    }
                    string profileAge = Request.Form["profile_age"];
                    if (profileAge != null)
                    {
                        short ProfileAge = 0;
                        if (System.Int16.TryParse(profileAge, out ProfileAge))
                        {
                            if (ProfileAge > 0 && ProfileAge < 100)
                            {
                                profile.ProfileAge = (sbyte)ProfileAge;
                                Log.Info("Update profile age.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                            }
                        }
                    }
                    string profileCity = Request.Form["profile_city"];
                    if (profileCity != null)
                    {
                        if (profileCity.Length > 3 && profileCity.Length < 50)
                        {
                            profile.ProfileCity = profileCity;
                            Log.Info("Update profile city.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                    }
                    if (profile_photo != null)
                    {
                        if (profile_photo.ContentType.Contains("image"))
                        {
                            if (System.IO.File.Exists(Config.savePath + profile.UrlPhoto))
                            {
                                System.IO.File.Delete(Config.savePath + profile.UrlPhoto);
                            }
                            Directory.CreateDirectory(Config.savePath + "/ProfilePhoto/" +
                             DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day);
                            profile.UrlPhoto = "/ProfilePhoto/" + DateTime.Now.Year + "-" + DateTime.Now.Month 
                            + "-" + DateTime.Now.Day + "/" + Validator.GenerateHash(10);
                            profile_photo.CopyTo(new System.IO.FileStream(Config.savePath + profile.UrlPhoto,
                            System.IO.FileMode.Create));
                            Log.Info("Update profile photo.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                    }
                    context.Profile.Update(profile);
                    context.SaveChanges();
                    Log.Info("Update profile.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    return new { success = true, data = new 
                    {
                        url_photo = profile.UrlPhoto == null ? "" : Config.AwsPath + profile.UrlPhoto,
                        profile_age = profile.ProfileAge == null ? -1 : profile.ProfileAge,
                        profile_gender = profile.ProfileGender,
                        profile_city = profile.ProfileCity == null  ? "" : profile.ProfileCity
                    } };
                }
            }
            else 
            {
                message = "Request doesn't contains 'user_token' key.";
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("RegistrateProfile")]
        public ActionResult<dynamic> RegistrateProfile(IFormFile profile_photo)
        {
            string message = null;
            string profileToken = Request.Form["profile_token"];
            if (profileToken != null)
            {
                User user = context.User.Where(u => u.ProfileToken == profileToken.ToString()).FirstOrDefault();
                if (user != null)
                {
                    Profile profile = users.CreateIfNotExistProfile(user.UserId);
                    string profileGender = Request.Form["profile_gender"];
                    if (profileGender != null)
                    {
                        if (profileGender == "1")
                        {
                            profile.ProfileGender = true;
                            Log.Info("Update profile gender.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                        else if (profileGender == "0")
                        {
                            profile.ProfileGender = false;
                            Log.Info("Update profile gender.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                        else
                        {
                            Log.Warn("Incorrect value to uUpdate profile gender.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                    }
                    string profileAge = Request.Form["profile_age"];
                    if (profileAge != null)
                    {
                        short ProfileAge = 0;
                        if (System.Int16.TryParse(profileAge, out ProfileAge))
                        {
                            if (ProfileAge > 0 && ProfileAge < 100)
                            {
                                profile.ProfileAge = (sbyte)ProfileAge;
                                Log.Info("Update profile age.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                            }
                        }
                    }
                    string profileCity = Request.Form["profile_city"];
                    if (profileCity != null)
                    {
                        if (profileCity.Length > 3 && profileCity.Length < 50)
                        {
                            profile.ProfileCity = profileCity;
                            Log.Info("Update profile city.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                    }
                    if (profile_photo != null)
                    {
                        if (profile_photo.ContentType.Contains("image"))
                        {
                            if (System.IO.File.Exists(Config.savePath + profile.UrlPhoto))
                            {
                                System.IO.File.Delete(Config.savePath + profile.UrlPhoto);
                            }
                            Directory.CreateDirectory(Config.savePath + "/ProfilePhoto/" +
                             DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day);
                            profile.UrlPhoto = "/ProfilePhoto/" + DateTime.Now.Year + "-" + DateTime.Now.Month 
                            + "-" + DateTime.Now.Day + "/" + Validator.GenerateHash(10);
                            profile_photo.CopyTo(new FileStream(Common.Config.savePath + profile.UrlPhoto,
                            FileMode.Create));
                            Log.Info("Update profile photo.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                    }
                    context.Profile.Update(profile);
                    context.SaveChanges();
                    Log.Info("Registrate profile.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    return new 
                    { 
                        success = true, 
                        message = "User account was successfully registered. See your email to activate account by link.",
                        data = new 
                        {
                            url_photo = profile.UrlPhoto == null ? "" : Config.savePath + profile.UrlPhoto,
                            profile_age = profile.ProfileAge == null ? -1 : profile.ProfileAge,
                            profile_gender = profile.ProfileGender,
                            profile_city = profile.ProfileCity == null  ? "" : profile.ProfileCity
                        } 
                    };
                }
                else 
                { 
                    message = "No user with that profile_token."; 
                }
            }
            else 
            {
                message = "Request doesn't contains 'profile_token' key.";
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("Profile")]
        public ActionResult<dynamic> Profile(UserCache userCache)
        {
            string message = null;
            User user = users.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Profile profile = users.CreateIfNotExistProfile(user.UserId);
                Log.Info("Select profile.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                profile.UrlPhoto = profile.UrlPhoto == null ? null : Config.AwsPath + profile.UrlPhoto;
                return new 
                { 
                    success = true, 
                    data = new 
                    {
                        url_photo = profile.UrlPhoto == null ? "" : Config.AwsPath + profile.UrlPhoto,
                        profile_age = profile.ProfileAge == null ? -1 : profile.ProfileAge,
                        profile_gender = profile.ProfileGender,
                        profile_city = profile.ProfileCity == null  ? "" : profile.ProfileCity
                    } 
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
        public ActionResult<dynamic> CreateChat(UserCache userCache)
        {
            string message = null;
            User user = users.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Chatroom room = new Chatroom();
                User interlocutor = context.User.Where(u => u.UserPublicToken == userCache.opposide_public_token).FirstOrDefault();
                if (interlocutor != null)
                {
                    Participants participant = context.Participants.Where(p => 
                    p.UserId == user.UserId &&
                    p.OpposideId == interlocutor.UserId).FirstOrDefault();
                    if (participant == null)
                    {
                        room.ChatToken = Validator.GenerateHash(20);
                        room.CreatedAt = System.DateTime.Now;
                        context.Chatroom.Add(room);
                        context.SaveChanges();
                        participant = new Participants();
                        participant.ChatId = room.ChatId;
                        participant.UserId = user.UserId;
                        participant.OpposideId = interlocutor.UserId;
                        context.Participants.Add(participant);
                        context.SaveChanges();
                        Participants opposideParticipant = new Participants();
                        opposideParticipant.ChatId = room.ChatId;
                        opposideParticipant.UserId = interlocutor.UserId;
                        opposideParticipant.OpposideId = user.UserId;
                        context.Participants.Add(opposideParticipant);
                        context.SaveChanges();
                        Log.Info("Create chat for user_id->" + user.UserId + " and opposide_id->" + interlocutor.UserId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    }
                    else
                    {
                        room = context.Chatroom.Where(ch => ch.ChatId == participant.ChatId).First();
                        Log.Info("Select exist chat for user_id->" + user.UserId + " and opposide_id->" + interlocutor.UserId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    }
                    Log.Info("Create/Select chat chat_id->" + room.ChatId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
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
                else 
                { 
                    message = "Can't define interlocutor by interlocutor_public_token from request's body."; 
                }
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
            string complaint = System.Net.WebUtility.UrlDecode(userCache.complaint);
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