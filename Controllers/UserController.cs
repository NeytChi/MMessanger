using System;
using System.IO;
using System.Net;
using Controllers;
using System.Linq;
using miniMessanger;
using miniMessanger.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Functional.UserF
{
    /// <summary>
    /// User functional for general movement. This class will be generate functional for user ability.
    /// </summary>
    [Route("v1.0/[controller]/[action]/")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private string awsPath = "none";
        private string savePath = "none";
        private DateTime unixed = new DateTime(1970, 1, 1, 0, 0, 0);
        private MMContext context;
        private JsonVariableHandler jsonHandler;
        public UserManager manager;
        public ChatManager chatManager;
        public UsersController(MMContext context)
        {
            this.context = context;
            this.awsPath = Common.Config.AwsPath;
            this.savePath = Common.Config.savePath;
            jsonHandler = new Controllers.JsonVariableHandler();
            manager = new UserManager(context);
        }
        /// <summary>
        /// Registration user with user_email and user_password.
        /// </summary>
        /// <param name="user">User data for registration.</param>
        [HttpPost]
        [ActionName("Registration")]
        public ActionResult<dynamic> Registration(JObject json)
        {
            string message = string.Empty;
            JToken userEmail = jsonHandler.handle(ref json, "user_email", JTokenType.String, ref message);
            if (userEmail != null)
            {
                JToken userLogin = jsonHandler.handle(ref json, "user_login", JTokenType.String, ref message);
                if (userLogin != null)
                {
                    JToken userPassword = jsonHandler.handle(ref json, "user_password", JTokenType.String, ref message);
                    if (userPassword != null)
                    {
                        if (Validator.ValidateLogin(userLogin.ToString(), ref message))
                        {
                            if (Validator.ValidateEmail(userEmail.ToString()))
                            {
                                if (Validator.ValidatePassword(userPassword.ToString(), ref message))
                                {
                                    Users user = context.Users.Where(u => u.UserEmail == userEmail.ToString()).FirstOrDefault();
                                    if (user == null)
                                    {
                                        user = new Users();
                                        user.UserEmail = userEmail.ToString();
                                        user.UserLogin = userLogin.ToString();
                                        user.UserPassword = Validator.HashPassword(userPassword.ToString());
                                        user.UserHash = Validator.GenerateHash(100);
                                        user.CreatedAt = (int)(DateTime.Now - Config.unixed).TotalSeconds;
                                        user.Activate = 0;
                                        user.Deleted = false;
                                        user.LastLoginAt = user.CreatedAt;
                                        user.UserToken = Validator.GenerateHash(40);
                                        user.UserPublicToken = Validator.GenerateHash(20);
                                        user.ProfileToken = Validator.GenerateHash(50);
                                        context.Users.Add(user);
                                        context.SaveChanges();
                                        MailF.SendEmail(user.UserEmail, "Confirm account", "Confirm account: <a href=http://" + Config.IP + ":" + Config.Port + "/v1.0/users/Activate/?hash=" + user.UserHash + ">Confirm url!</a>");
                                        Log.Info("Registrate new user.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                                        return new 
                                        { 
                                            success = true, 
                                            message = "User account was successfully registered. See your email to activate account by link.",
                                            data = new 
                                            {
                                                profile_token = user.ProfileToken,
                                            }
                                        };
                                    }
                                    else
                                    {
                                        if (user.Deleted == true)
                                        {
                                            user.Deleted = false;
                                            user.UserToken = Validator.GenerateHash(40); 
                                            context.Users.Update(user);
                                            context.SaveChanges();
                                            Log.Info("Restored old user, user_id->" + user.UserId + ".", HttpContext.Connection.LocalIpAddress.ToString(), user.UserId);
                                            return new { success = true, message = "User account was successfully restored." };
                                        }
                                        else 
                                        {
                                            message =  "Have exists account with email ->" + user.UserEmail + ".";
                                            Log.Warn("Have exists account with email ->" + user.UserEmail + ".", HttpContext.Connection.RemoteIpAddress.ToString()); 
                                        }  
                                    }
                                }
                                else
                                {
                                    Log.Warn(message + " UserEmail->" + userEmail.ToString() + ".", HttpContext.Connection.RemoteIpAddress.ToString());                        
                                }
                            }
                            else 
                            { 
                                message = "Not valid email ->" + userEmail.ToString() + ".";
                            }
                        }
                    }
                }
            }
            Log.Warn(message, HttpContext.Connection.LocalIpAddress.ToString());
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            return new { success = false, message = message, };
        }
        [HttpPost]
        [ActionName("RegistrationEmail")]
        public ActionResult<dynamic> RegistrationEmail(JObject json)
        {
            string message = null;
            JToken userEmail = jsonHandler.handle(ref json, "user_email", JTokenType.String, ref message);
            if (userEmail != null)
            {
                Users user = context.Users.Where(u => u.UserEmail == userEmail.ToString()).FirstOrDefault();;
                if (user != null)
                {
                    if (!user.Deleted)
                    {
                        MailF.SendEmail(user.UserEmail, "Confirm account", "Confirm account url: <a href=http://" + Config.IP + ":" + Config.Port + "/v1.0/users/Activate/?hash=" + user.UserHash + ">Confirm url!</a>");
                        Log.Info("Send registration email to user.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        return new { success = true, message = "Send confirm email to user." };
                    }
                    else 
                    { 
                        message = "Unknow email -> " + user.UserEmail + "."; 
                        Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    }
                }
                else 
                { 
                    message = "Can't define user data by json's key 'user_email'."; 
                    Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                }
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("Login")]
        public ActionResult<dynamic> Login(JObject json)
        {
            string message = null;
            JToken userEmail = jsonHandler.handle(ref json, "user_email",  JTokenType.String, ref message);
            if (userEmail != null)
            {
                JToken userPassword = jsonHandler.handle(ref json, "user_password",  JTokenType.String, ref message);
                if (userPassword != null)
                {
                    Users user_data = context.Users.Where(u=> u.UserEmail == userEmail.ToString()).FirstOrDefault();
                    if (user_data != null)
                    {
                        if (Validator.VerifyHashedPassword(user_data.UserPassword, userPassword.ToString()))
                        {
                            if (user_data.Activate == 1 && user_data.Deleted == false)
                            {
                                user_data.LastLoginAt = (int)(DateTime.Now - Config.unixed).TotalSeconds;
                                context.Users.Update(user_data);
                                Profiles profile = context.Profiles.Where(p => p.UserId == user_data.UserId).FirstOrDefault();
                                if (profile == null)
                                {
                                    profile = new Profiles();
                                    profile.UserId = user_data.UserId;
                                    profile.ProfileGender = true;
                                    context.Add(profile);
                                    context.SaveChanges();
                                }
                                context.SaveChanges();
                                Log.Info("User login.", HttpContext.Connection.RemoteIpAddress.ToString(), user_data.UserId);
                                return new 
                                { 
                                    success = true, data = new 
                                    { 
                                        user_id = user_data.UserId,
                                        user_token = user_data.UserToken,
                                        user_email = user_data.UserEmail,
                                        user_login = user_data.UserLogin,
                                        created_at = user_data.CreatedAt,
                                        last_login_at = user_data.LastLoginAt,
                                        user_public_token = user_data.UserPublicToken,
                                        profile = new
                                        {
                                            url_photo = profile.UrlPhoto == null ? "" : awsPath + profile.UrlPhoto,
                                            profile_age = profile.ProfileAge == null ? -1 : profile.ProfileAge,
                                            profile_gender = profile.ProfileGender,
                                            profile_city = profile.ProfileCity == null  ? "" : profile.ProfileCity
                                        }    
                                    } 
                                };
                            }
                            else 
                            { 
                                MailF.SendEmail(user_data.UserEmail, "Confirm account", "Confirm account: <a href=http://" + Config.IP + ":" + Config.Port + "/v1.0/users/Activate/?hash=" + user_data.UserHash + ">Confirm url!</a>");
                                message =  "User's account isn't confirmed."; 
                            }
                        }
                        else 
                        { 
                            message = "Wrong password."; 
                        }
                    }
                    else 
                    { 
                        message = "No user with such email."; 
                    }
                }
            }            
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("LogOut")]
        public ActionResult<dynamic> LogOut(UserCache userCache)
        {
            string message = null;
            Users user = manager.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                user.UserToken = Validator.GenerateHash(40);
                context.Users.Update(user);
                context.SaveChanges();
                Log.Info("User log out.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                return new { success = true, message = "Log out is successfully." };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("RecoveryPassword")]
        public ActionResult<dynamic> RecoveryPassword(JObject json)
        {
            string message = null;
            JToken userEmail = jsonHandler.handle(ref json, "user_email", JTokenType.String, ref message);
            if (userEmail != null)
            {
                Users user = context.Users.Where(u => u.UserEmail == userEmail.ToString()).FirstOrDefault();
                if (user != null || !user.Deleted)
                {
                    user.RecoveryCode = Common.Validator.random.Next(100000, 999999);
                    MailF.SendEmail(user.UserEmail, "Recovery password", "Recovery code=" + user.RecoveryCode);
                    context.Users.Update(user);
                    context.SaveChanges();
                    Log.Info("Recovery password.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    return new { success = true, message = "Recovery password. Send message with code to email=" + user.UserEmail + "." };
                }
                else 
                { 
                    message = "Unknow email."; 
                }
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("CheckRecoveryCode")]
        public ActionResult<dynamic> CheckRecoveryCode(JObject json)
        {
            string message = null;
            JToken userEmail = jsonHandler.handle(ref json, "user_email", JTokenType.String, ref message);
            if (userEmail != null)
            {
                JToken recoveryCode = jsonHandler.handle(ref json, "recovery_code", JTokenType.Integer, ref message);
                if (userEmail != null)
                {
                    Users user = context.Users.Where(u => u.UserEmail == userEmail.ToString()).FirstOrDefault();
                    if (user != null)
                    {
                        if (!user.Deleted)
                        {
                            if (user.RecoveryCode == recoveryCode.ToObject<int>())
                            {
                                user.RecoveryToken = Common.Validator.GenerateHash(40);
                                user.RecoveryCode = 0;
                                context.Users.Update(user);
                                context.SaveChanges();
                                Log.Info("Check recovery code - successed.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                                return new { success = true, data = new { recovery_token = user.RecoveryToken }};
                            }
                            else 
                            { 
                                message = "Wrong recovery code."; 
                            }
                        }
                        else 
                        { 
                            message = "Unknow email."; 
                        }
                    }
                    else 
                    { 
                        message = "Can't define user by key->user_email."; 
                    }
                }
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("ChangePassword")]
        public ActionResult<dynamic> ChangePassword(JObject json)
        {
            string message = null;
            JToken recoveryToken = jsonHandler.handle(ref json, "recovery_token", JTokenType.String, ref message);
            if (recoveryToken != null)
            {
                JToken userPassword = jsonHandler.handle(ref json, "user_password", JTokenType.String, ref message);
                if (userPassword != null)
                {
                    JToken userConfirmPassword = jsonHandler.handle(ref json, "user_confirm_password", JTokenType.String, ref message);
                    if (userConfirmPassword != null)
                    {
                        Users user = context.Users.Where(u=> u.RecoveryToken == recoveryToken.ToString()).FirstOrDefault();
                        if (user != null)
                        {
                            if (!user.Deleted)
                            {
                                if (userPassword.ToString().Equals(userConfirmPassword.ToString()))
                                {
                                    if (Validator.ValidatePassword(userPassword.ToString(), ref message))
                                    {
                                        user.UserPassword = Validator.HashPassword(userPassword.ToString());
                                        user.RecoveryToken  = "";
                                        context.Users.Update(user);
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
                            else 
                            { 
                                message = "No user with that email."; 
                            }
                        }
                        else 
                        { 
                            message = "Can't find user by recovery_token."; 
                        }
                    }
                }
            }
            return Return500Error(message);
        }
        [HttpGet]
        [ActionName("Activate")]
        public ActionResult<dynamic> Activate([FromQuery] string hash)
        {
            string message = null;
            Users user = context.Users.Where(u => u.UserHash == hash).FirstOrDefault();
            if (user != null)
            {
                if (!user.Deleted)
                {
                    user.Activate = 1;
                    context.Users.Update(user);
                    context.SaveChanges();                
                    Log.Info("Active user account.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    return new { success = true, message = "User account is successfully active." };
                }
                else 
                { 
                    message = "No user with that email."; 
                }
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
            Users user = manager.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                user.Deleted = true;
                user.UserToken = null;
                context.Users.Update(user);
                context.SaveChanges();
                Log.Info("Account was successfully deleted.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId); 
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
                Users user = context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                if (user != null)
                {
                    Profiles profile = context.Profiles.Where(p => p.UserId == user.UserId).FirstOrDefault();
                    if (profile == null)
                    {
                        profile = new Profiles();
                        profile.UserId = user.UserId;
                        profile.ProfileGender = true;
                        context.Add(profile);
                        context.SaveChanges();
                    }
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
                            if (System.IO.File.Exists(savePath + profile.UrlPhoto))
                            {
                                System.IO.File.Delete(savePath + profile.UrlPhoto);
                            }
                            Directory.CreateDirectory(savePath + "/ProfilePhoto/" +
                             DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day);
                            profile.UrlPhoto = "/ProfilePhoto/" + DateTime.Now.Year + "-" + DateTime.Now.Month 
                            + "-" + DateTime.Now.Day + "/" + Validator.GenerateHash(10);
                            profile_photo.CopyTo(new System.IO.FileStream(savePath + profile.UrlPhoto,
                            System.IO.FileMode.Create));
                            Log.Info("Update profile photo.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                    }
                    context.Profiles.Update(profile);
                    context.SaveChanges();
                    Log.Info("Update profile.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    return new { success = true, data = new 
                    {
                        url_photo = profile.UrlPhoto == null ? "" : awsPath + profile.UrlPhoto,
                        profile_age = profile.ProfileAge == null ? -1 : profile.ProfileAge,
                        profile_gender = profile.ProfileGender,
                        profile_city = profile.ProfileCity == null  ? "" : profile.ProfileCity
                    } };
                }
                else 
                { 
                    message = "No user with that user_token."; 
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
                Users user = context.Users.Where(u => u.ProfileToken == profileToken.ToString()).FirstOrDefault();
                if (user != null)
                {
                    Profiles profile = context.Profiles.Where(p => p.UserId == user.UserId).FirstOrDefault();
                    if (profile == null)
                    {
                        profile = new Profiles();
                        profile.UserId = user.UserId;
                        profile.ProfileGender = true;
                        context.Add(profile);
                        context.SaveChanges();
                    }
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
                            if (System.IO.File.Exists(savePath + profile.UrlPhoto))
                            {
                                System.IO.File.Delete(savePath + profile.UrlPhoto);
                            }
                            Directory.CreateDirectory(savePath + "/ProfilePhoto/" +
                             DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day);
                            profile.UrlPhoto = "/ProfilePhoto/" + DateTime.Now.Year + "-" + DateTime.Now.Month 
                            + "-" + DateTime.Now.Day + "/" + Validator.GenerateHash(10);
                            profile_photo.CopyTo(new FileStream(Common.Config.savePath + profile.UrlPhoto,
                            FileMode.Create));
                            Log.Info("Update profile photo.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                    }
                    context.Profiles.Update(profile);
                    context.SaveChanges();
                    Log.Info("Registrate profile.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    return new 
                    { 
                        success = true, 
                        message = "User account was successfully registered. See your email to activate account by link.",
                        data = new 
                        {
                            url_photo = profile.UrlPhoto == null ? "" : awsPath + profile.UrlPhoto,
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
            Users user = manager.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Profiles profile = context.Profiles.Where(p => p.UserId == user.UserId).FirstOrDefault();
                if (profile == null)
                {
                    profile = new Profiles();
                    profile.UserId = user.UserId;
                    profile.ProfileGender = true;
                    context.Add(profile);
                    context.SaveChanges();
                }
                Log.Info("Select profile.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                profile.UrlPhoto = profile.UrlPhoto == null ? null : awsPath + profile.UrlPhoto;
                return new 
                { 
                    success = true, 
                    data = new 
                    {
                        url_photo = profile.UrlPhoto == null ? "" : awsPath + profile.UrlPhoto,
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
            Users user = manager.GetUserByToken(cache.user_token, ref message);
            if (user != null)
            {
                List<dynamic> data = new List<dynamic>();  
                List<Users> users = context.Users.Where(u 
                => u.UserId != user.UserId)
                .OrderByDescending(u => u.UserId)
                .Skip(cache.page * 30).Take(30).ToList();
                List<int> blocked = context.BlockedUsers.Where(b 
                => b.UserId == user.UserId 
                && b.BlockedDeleted == false)
                .Select(b => b.BlockedUserId).ToList();
                foreach(Users publicUser in users)
                {
                    if (!blocked.Contains(publicUser.UserId))
                    {
                        var userData = new 
                        {
                            user_id = publicUser.UserId,
                            user_email = publicUser.UserEmail,
                            user_login = publicUser.UserLogin,
                            created_at = publicUser.CreatedAt,
                            last_login_at = publicUser.LastLoginAt,
                            user_public_token = publicUser.UserPublicToken
                        };
                        data.Add(userData);
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
            Users user = manager.GetUserByToken(userCache.user_token, ref message);
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
                        Users opposide = context.Users.Where(u 
                        => u.UserId == participant.OpposideId).First();
                        Messages last_message = context.Messages.Where(m 
                        => m.ChatId == room.ChatId)
                        .OrderByDescending(m => m.MessageId).FirstOrDefault();
                        dynamic lastMessage = last_message;
                        if (last_message != null)
                        {
                            lastMessage = new
                            {
                                message_id = last_message.MessageId,
                                chat_id = last_message.ChatId,
                                user_id = last_message.UserId,
                                message_text = last_message.MessageText,
                                message_viewed = last_message.MessageViewed,
                                created_at = last_message.CreatedAt
                            };
                        }
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
                            last_message = lastMessage
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
            Users user = manager.GetUserByToken(userCache.user_token, ref messageReturn);
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
                        url_file = string.IsNullOrEmpty(m.UrlFile) ? "" : awsPath + m.UrlFile,
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
            Users user = manager.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Chatroom room = new Chatroom();
                room.users = new List<dynamic>();
                Users interlocutor = context.Users.Where(u => u.UserPublicToken == userCache.opposide_public_token).FirstOrDefault();
                if (interlocutor != null)
                {
                    Participants participant = context.Participants.Where(p => 
                    p.UserId == user.UserId &&
                    p.OpposideId == interlocutor.UserId).FirstOrDefault();
                    if (participant == null)
                    {
                        room.ChatToken = Common.Validator.GenerateHash(20);
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
                Users user = manager.GetUserByToken(userCache.user_token, ref answer);
                if (user != null)
                {
                    Chatroom room = context.Chatroom.Where(ch 
                    => ch.ChatToken == userCache.chat_token).FirstOrDefault();
                    if (room != null)
                    {
                        Messages message = new Messages();
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
                            data = chatManager.ResponseMessage(message)
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
                Messages result = chatManager.UploadMessagePhoto(photo, cache, ref message);
                if (result != null)
                {
                    return new { success = true, data = chatManager.ResponseMessage(result) };
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
        public ActionResult<dynamic> BlockUser(UserCache userCache)
        {
            string message = null;
            string blockedReason = System.Net.WebUtility.UrlDecode(userCache.blocked_reason);
            Users user = manager.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Users interlocutor = context.Users.Where(u 
                => u.UserPublicToken == userCache.opposide_public_token).FirstOrDefault();
                if (interlocutor != null)
                {
                    if (blockedReason.Length < 100)
                    {
                        BlockedUsers blocked = context.BlockedUsers.Where(b => b.UserId == user.UserId && b.BlockedUserId == interlocutor.UserId && b.BlockedDeleted == false).FirstOrDefault();
                        if (blocked == null)
                        {
                            BlockedUsers blockedUser = new BlockedUsers();
                            blockedUser.UserId = user.UserId;
                            blockedUser.BlockedUserId = interlocutor.UserId;
                            blockedUser.BlockedReason = blockedReason;
                            blockedUser.BlockedDeleted = false;
                            context.BlockedUsers.Add(blockedUser);
                            Log.Info("Block user.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                            context.SaveChanges();
                            return new { success = true, message = "Block user - successed." };
                        }
                        else 
                        { 
                            message = "User blocked current user."; 
                        }
                    }
                    else 
                    { 
                        message = "Reason message can't be longer than 100 characters."; 
                    }
                }
                else 
                { 
                    message = "No user with that opposide_public_token."; 
                }
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("GetBlockedUsers")]
        public ActionResult<dynamic> GetBlockedUsers(UserCache cache)
        {
            string message = null;
            Users user = manager.GetUserByToken(cache.user_token, ref message);
            if (user != null)
            {
                var blockedUsers = (from blocked in context.BlockedUsers
                join users in context.Users on blocked.BlockedUserId equals users.UserId
                where blocked.UserId == user.UserId && blocked.BlockedDeleted == false
                select new
                { 
                    user_email = users.UserEmail,
                    user_login =  users.UserLogin,
                    last_login_at = users.LastLoginAt,
                    user_public_token = users.UserPublicToken,
                    blocked_reason = blocked.BlockedReason 
                }
                ).ToList();         
                Log.Info("Block user; user->user_id->" + user.UserId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                return new { success = true, data = blockedUsers };;
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("UnblockUser")]
        public ActionResult<dynamic> UnblockUser(UserCache userCache)
        {
            string message = null;
            Users user = manager.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Users interlocutor = context.Users.Where(u 
                => u.UserPublicToken == userCache.opposide_public_token).FirstOrDefault();
                if (interlocutor != null)
                {
                    BlockedUsers blockedUser = context.BlockedUsers.Where(b => b.UserId == user.UserId 
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
            Users user = manager.GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Messages messageChat = context.Messages.Where(m 
                => m.MessageId == userCache.message_id).FirstOrDefault();
                if (messageChat != null)
                {
                    if (complaint.Length < 100)
                    {
                        if (messageChat.UserId != user.UserId)
                        {
                            Users interlocutor = context.Users.Where(u => u.UserId == messageChat.UserId).FirstOrDefault();
                            BlockedUsers blockedUser = context.BlockedUsers.Where(b => b.UserId == user.UserId 
                            && b.BlockedUserId == interlocutor.UserId && b.BlockedDeleted == false).FirstOrDefault();
                            if (blockedUser == null)
                            {
                                blockedUser = new BlockedUsers();
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
            var userData = manager.GetUserWithProfile(cache.user_token, ref message);
            if (userData != null)
            {
                var usersData = (from users in context.Users
                join profile in context.Profiles on users.UserId equals profile.UserId
                join likesProfile in context.LikeProfile on users.UserId equals likesProfile.ToUserId into likes
                join blockedUser in context.BlockedUsers on users.UserId equals blockedUser.BlockedUserId into blockedUsers
                where users.UserId != userData.UserId && profile.ProfileGender != userData.Profile.ProfileGender
                && users.Activate == 1
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
                        url_photo = profile.UrlPhoto == null ? "" : awsPath + profile.UrlPhoto,
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
            var user = manager.GetUserWithProfile(cache.user_token, ref message);
            if (user != null)
            {
                var data = (
                from participant in context.Participants
                join users in context.Users on participant.OpposideId equals users.UserId
                join profile in context.Profiles on users.UserId equals profile.UserId
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
                            url_photo = profile.UrlPhoto == null ? "" : awsPath + profile.UrlPhoto,
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
                }).Skip(cache.page * 30).Take(30).ToList();           
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
            Users user = manager.GetUserWithProfile(cache.user_token, ref message);
            if (user != null)
            {
                dynamic data = manager.ReciprocalUsers(user.UserId, cache.page, cache.count);
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
            LikeProfiles like = manager.LikeUser(cache, ref message);
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
            LikeProfiles dislike = manager.DislikeUser(cache, ref message);
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