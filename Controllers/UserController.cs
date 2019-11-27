using System;
using System.IO;
using Controllers;
using System.Linq;
using miniMessanger.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net;

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
        private MMContext _context;
        private JsonVariableHandler jsonHandler;
        public UsersController(MMContext _context)
        {
            this._context = _context;
            this.awsPath = Common.Config.AwsPath;
            this.savePath = Common.Config.savePath;
            jsonHandler = new Controllers.JsonVariableHandler();
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
                                    Users user = _context.Users.Where(u => u.UserEmail == userEmail.ToString()).FirstOrDefault();
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
                                        _context.Users.Add(user);
                                        _context.SaveChanges();
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
                                            _context.Users.Update(user);
                                            _context.SaveChanges();
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
                    else
                    {
                        Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
                    }
                }
                else
                {
                    Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
                }
            }
            else
            {
                Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
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
                Users user = _context.Users.Where(u => u.UserEmail == userEmail.ToString()).FirstOrDefault();;
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
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
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
                    Users user_data = _context.Users.Where(u=> u.UserEmail == userEmail.ToString()).FirstOrDefault();
                    if (user_data != null)
                    {
                        if (Validator.VerifyHashedPassword(user_data.UserPassword, userPassword.ToString()))
                        {
                            if (user_data.Activate == 1 && user_data.Deleted == false)
                            {
                                user_data.LastLoginAt = (int)(DateTime.Now - Config.unixed).TotalSeconds;
                                _context.Users.Update(user_data);
                                Profiles profile = _context.Profiles.Where(p => p.UserId == user_data.UserId).FirstOrDefault();
                                if (profile == null)
                                {
                                    profile = new Profiles();
                                    profile.UserId = user_data.UserId;
                                    profile.ProfileGender = true;
                                    _context.Add(profile);
                                    _context.SaveChanges();
                                }
                                _context.SaveChanges();
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
                                Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString(), user_data.UserId);
                            }
                        }
                        else 
                        { 
                            message = "Wrong password."; 
                            Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString(), user_data.UserId);
                        }
                    }
                    else 
                    { 
                        message = "No user with such email."; 
                        Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
                    }
                }
                else
                {
                    Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
                }
            }
            else
            {
                Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            return new { success = false, message = message };
        }
        [HttpPut]
        [ActionName("LogOut")]
        public ActionResult<dynamic> LogOut(UserCache userCache)
        {
            string message = null;
            Users user = GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                user.UserToken = Validator.GenerateHash(40);
                _context.Users.Update(user);
                _context.SaveChanges();
                Log.Info("User log out.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                return new { success = true, message = "Log out is successfully." };
            }
            else 
            { 
                message = "Server can't get user's data by user_token from json."; 
            }   
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
        }
        [HttpPost]
        [ActionName("RecoveryPassword")]
        public ActionResult<dynamic> RecoveryPassword(JObject json)
        {
            string message = null;
            JToken userEmail = jsonHandler.handle(ref json, "user_email", JTokenType.String, ref message);
            if (userEmail != null)
            {
                Users user = _context.Users.Where(u => u.UserEmail == userEmail.ToString()).FirstOrDefault();
                if (user != null || !user.Deleted)
                {
                        user.RecoveryCode = Common.Validator.random.Next(100000, 999999);
                        MailF.SendEmail(user.UserEmail, "Recovery password", "Recovery code=" + user.RecoveryCode);
                        _context.Users.Update(user);
                        _context.SaveChanges();
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
        public dynamic Return500Error(string message)
        {
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
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
                    Users user = _context.Users.Where(u => u.UserEmail == userEmail.ToString()).FirstOrDefault();
                    if (user != null)
                    {
                        if (!user.Deleted)
                        {
                            if (user.RecoveryCode == recoveryCode.ToObject<int>())
                            {
                                user.RecoveryToken = Common.Validator.GenerateHash(40);
                                user.RecoveryCode = 0;
                                _context.Users.Update(user);
                                _context.SaveChanges();
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
                        Users user = _context.Users.Where(u=> u.RecoveryToken == recoveryToken.ToString()).FirstOrDefault();
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
                                        _context.Users.Update(user);
                                        _context.SaveChanges();
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
            Users user = _context.Users.Where(u => u.UserHash == hash).FirstOrDefault();
            if (user != null)
            {
                if (!user.Deleted)
                {
                    user.Activate = 1;
                    _context.Users.Update(user);
                    _context.SaveChanges();                
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
            Users user = GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                user.Deleted = true;
                user.UserToken = null;
                _context.Users.Update(user);
                _context.SaveChanges();
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
                Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                if (user != null)
                {
                    Profiles profile = _context.Profiles.Where(p => p.UserId == user.UserId).FirstOrDefault();
                    if (profile == null)
                    {
                        profile = new Profiles();
                        profile.UserId = user.UserId;
                        profile.ProfileGender = true;
                        _context.Add(profile);
                        _context.SaveChanges();
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
                    _context.Profiles.Update(profile);
                    _context.SaveChanges();
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
                Users user = _context.Users.Where(u => u.ProfileToken == profileToken.ToString()).FirstOrDefault();
                if (user != null)
                {
                    Profiles profile = _context.Profiles.Where(p => p.UserId == user.UserId).FirstOrDefault();
                    if (profile == null)
                    {
                        profile = new Profiles();
                        profile.UserId = user.UserId;
                        profile.ProfileGender = true;
                        _context.Add(profile);
                        _context.SaveChanges();
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
                    _context.Profiles.Update(profile);
                    _context.SaveChanges();
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
            Users user = GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Profiles profile = _context.Profiles.Where(p => p.UserId == user.UserId).FirstOrDefault();
                if (profile == null)
                {
                    profile = new Profiles();
                    profile.UserId = user.UserId;
                    profile.ProfileGender = true;
                    _context.Add(profile);
                    _context.SaveChanges();
                }
                Log.Info("Select profile.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                profile.UrlPhoto = profile.UrlPhoto == null ? null : awsPath + profile.UrlPhoto;
                return new { success = true, 
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
        public ActionResult<dynamic> GetUsersList(JObject json)
        {
            int page = 0;
            string message = null;
            JToken userToken = jsonHandler.handle(ref json, "user_token", JTokenType.String, ref message);
            if (userToken != null)
            {
                Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                if (user != null)
                {
                    JToken jPage = jsonHandler.handle(ref json, "page", JTokenType.String, ref message);
                    if (jPage != null)
                    {
                        page = jPage.ToObject<int>();
                    }
                    List<dynamic> data = new List<dynamic>();  
                    List<Users> users = _context.Users.Where(u => u.UserId != user.UserId).OrderByDescending(u => u.UserId).Skip(page * 30).Take(30).ToList();
                    List<int> blocked = _context.BlockedUsers.Where(b => b.UserId == user.UserId && b.BlockedDeleted == false).Select(b => b.BlockedUserId).ToList();
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
                    _context.SaveChanges();
                    return new { success = true, data = data };
                }
                else 
                { 
                    message = "No user with that user_token."; 
                }
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
            Users user = GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                List<dynamic> chats = new List<dynamic>();
                List<Participants> participants = _context.Participants.Where(p 
                => p.UserId == user.UserId).ToList();
                List<int> blocked = _context.BlockedUsers.Where(b 
                => b.UserId == user.UserId 
                && b.BlockedDeleted == false)
                .Select(b => b.BlockedUserId).ToList();
                foreach(Participants participant in participants)
                {
                    if (!blocked.Contains(participant.OpposideId))
                    {
                        Chatroom room = _context.Chatroom.Where(ch 
                        => ch.ChatId == participant.ChatId).First();
                        Users opposide = _context.Users.Where(u 
                        => u.UserId == participant.OpposideId).First();
                        Messages last_message = _context.Messages.Where(m 
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
                _context.SaveChanges();
                return new { success = true, data = chats };
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("SelectMessages")]
        public ActionResult<dynamic> SelectMessages(UserCache userCache)
        {
            string messageReturn = null;
            Users user = GetUserByToken(userCache.user_token, ref messageReturn);
            if (user != null)
            {
                Chatroom room = _context.Chatroom.Where(r 
                => r.ChatToken == userCache.chat_token).FirstOrDefault();
                if (room != null)
                {
                    var messages = _context.Messages.Where(m 
                    => m.ChatId == room.ChatId)
                    .OrderByDescending(m => m.MessageId)
                    .Skip(userCache.page * 50).Take(50)
                    .Select(m 
                    => new 
                    { 
                        message_id = m.MessageId, 
                        chat_id = m.ChatId, 
                        user_id= m.UserId,
                        message_text = m.MessageText, 
                        message_viewed = m.MessageViewed, 
                        created_at = m.CreatedAt 
                    }).ToList(); 
                    var data = (from m in _context.Messages 
                    where m.ChatId == room.ChatId 
                    && m.UserId != user.UserId 
                    select m).ToList();
                    data.ForEach(m => m.MessageViewed = true);
                    _context.SaveChanges();
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
            Users user = GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Chatroom room = new Chatroom();
                room.users = new List<dynamic>();
                Users interlocutor = _context.Users.Where(u => u.UserPublicToken == userCache.opposide_public_token).FirstOrDefault();
                if (interlocutor != null)
                {
                    Participants participant = _context.Participants.Where(p => 
                    p.UserId == user.UserId &&
                    p.OpposideId == interlocutor.UserId).FirstOrDefault();
                    if (participant == null)
                    {
                        room.ChatToken = Common.Validator.GenerateHash(20);
                        room.CreatedAt = System.DateTime.Now;
                        _context.Chatroom.Add(room);
                        _context.SaveChanges();
                        participant = new Participants();
                        participant.ChatId = room.ChatId;
                        participant.UserId = user.UserId;
                        participant.OpposideId = interlocutor.UserId;
                        _context.Participants.Add(participant);
                        _context.SaveChanges();
                        Participants opposideParticipant = new Participants();
                        opposideParticipant.ChatId = room.ChatId;
                        opposideParticipant.UserId = interlocutor.UserId;
                        opposideParticipant.OpposideId = user.UserId;
                        _context.Participants.Add(opposideParticipant);
                        _context.SaveChanges();
                        Log.Info("Create chat for user_id->" + user.UserId + " and opposide_id->" + interlocutor.UserId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    }
                    else
                    {
                        room = _context.Chatroom.Where(ch => ch.ChatId == participant.ChatId).First();
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
            string message = null;
            
            string messageText = WebUtility.UrlDecode(userCache.message_text);
            if (!string.IsNullOrEmpty(messageText))
            {
                Users user = GetUserByToken(userCache.user_token, ref message);
                if (user != null)
                {
                    Chatroom room = _context.Chatroom.Where(ch 
                    => ch.ChatToken == userCache.chat_token).FirstOrDefault();
                    if (room != null)
                    {
                        Messages chatMessage = new Messages();
                        chatMessage.ChatId = room.ChatId;
                        chatMessage.UserId = user.UserId;
                        chatMessage.MessageText = messageText;
                        chatMessage.MessageViewed = false;
                        chatMessage.CreatedAt = System.DateTime.Now;
                        _context.Messages.Add(chatMessage);
                        _context.SaveChanges();
                        Log.Info("Message was handled, message_id->" + chatMessage.MessageId + " chat.chat_id->" + room.ChatId, HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        return new 
                        { 
                            success = true, 
                            data = new
                            {
                                message_id = chatMessage.MessageId,
                                chat_id = chatMessage.ChatId,
                                user_id = chatMessage.UserId,
                                message_text = chatMessage.MessageText,
                                message_viewed = chatMessage.MessageViewed,
                                created_at = chatMessage.CreatedAt
                            }
                        };
                    } 
                    else 
                    { 
                        message = "Server can't define chat by chat_token."; 
                    }
                } 
            } 
            else 
            { 
                message = "Message is empty. Server woundn't upload this message."; 
            }        
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("BlockUser")]
        public ActionResult<dynamic> BlockUser(UserCache userCache)
        {
            string message = null;
            string blockedReason = System.Net.WebUtility.UrlDecode(userCache.blocked_reason);
            Users user = GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Users interlocutor = _context.Users.Where(u 
                => u.UserPublicToken == userCache.opposide_public_token).FirstOrDefault();
                if (interlocutor != null)
                {
                    if (blockedReason.Length < 100)
                    {
                        BlockedUsers blocked = _context.BlockedUsers.Where(b => b.UserId == user.UserId && b.BlockedUserId == interlocutor.UserId && b.BlockedDeleted == false).FirstOrDefault();
                        if (blocked == null)
                        {
                            BlockedUsers blockedUser = new BlockedUsers();
                            blockedUser.UserId = user.UserId;
                            blockedUser.BlockedUserId = interlocutor.UserId;
                            blockedUser.BlockedReason = blockedReason;
                            blockedUser.BlockedDeleted = false;
                            _context.BlockedUsers.Add(blockedUser);
                            Log.Info("Block user.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                            _context.SaveChanges();
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
        public ActionResult<dynamic> GetBlockedUsers(JObject json)
        {
            string message = null;
            JToken userToken = jsonHandler.handle(ref json, "user_token", JTokenType.String, ref message);
            if (userToken != null)
            {
                Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                if (user != null)
                {
                    var blockedUsers = (from blocked in _context.BlockedUsers
                    join users in _context.Users on blocked.BlockedUserId equals users.UserId
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
                else 
                { 
                    message = "No user with that user_token."; 
                }
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("UnblockUser")]
        public ActionResult<dynamic> UnblockUser(UserCache userCache)
        {
            string message = null;
            Users user = GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Users interlocutor = _context.Users.Where(u 
                => u.UserPublicToken == userCache.opposide_public_token).FirstOrDefault();
                if (interlocutor != null)
                {
                    BlockedUsers blockedUser = _context.BlockedUsers.Where(b => b.UserId == user.UserId 
                    && b.BlockedUserId == interlocutor.UserId && b.BlockedDeleted == false).FirstOrDefault();
                    if (blockedUser != null)
                    {
                        blockedUser.BlockedDeleted = true;
                        _context.BlockedUsers.UpdateRange(blockedUser);
                        _context.SaveChanges();
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
            Users user = GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Messages messageChat = _context.Messages.Where(m 
                => m.MessageId == userCache.message_id).FirstOrDefault();
                if (messageChat != null)
                {
                    if (complaint.Length < 100)
                    {
                        if (messageChat.UserId != user.UserId)
                        {
                            Users interlocutor = _context.Users.Where(u => u.UserId == messageChat.UserId).FirstOrDefault();
                            BlockedUsers blockedUser = _context.BlockedUsers.Where(b => b.UserId == user.UserId 
                            && b.BlockedUserId == interlocutor.UserId && b.BlockedDeleted == false).FirstOrDefault();
                            if (blockedUser == null)
                            {
                                blockedUser = new BlockedUsers();
                                blockedUser.UserId = user.UserId;
                                blockedUser.BlockedUserId = interlocutor.UserId;
                                blockedUser.BlockedReason = complaint;
                                blockedUser.BlockedDeleted = false;
                                _context.BlockedUsers.Add(blockedUser);
                                Complaints complaintUser = new Complaints();
                                complaintUser.UserId = user.UserId;
                                complaintUser.BlockedId = blockedUser.BlockedId;
                                complaintUser.MessageId = messageChat.MessageId;
                                complaintUser.Complaint = complaint;
                                complaintUser.CreatedAt = System.DateTime.Now;
                                _context.Complaints.Add(complaintUser);
                                _context.SaveChanges();
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
        public ActionResult<dynamic> GetUsersByGender(UserCache userCache)
        {
            string message = null;
            var userData = (from u in _context.Users
            join p in _context.Profiles on u.UserId equals p.UserId
            where u.UserToken == userCache.user_token
            select new { UserId = u.UserId, ProfileGender = p.ProfileGender } ).FirstOrDefault();
            if (userData != null)
            {
                var usersData = (from users in _context.Users
                join profile in _context.Profiles on users.UserId equals profile.UserId
                join likesProfile in _context.LikeProfile on users.UserId equals likesProfile.ToUserId into likes
                join blockedUser in _context.BlockedUsers on users.UserId equals blockedUser.BlockedUserId into blockedUsers
                where users.UserId != userData.UserId && profile.ProfileGender != userData.ProfileGender
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
                    liked_user = likes.Any(l => l.UserId == userData.UserId) ? true : false
                }).Skip(userCache.page * 30).Take(30).ToList();
                Log.Info("Get users list.", HttpContext.Connection.RemoteIpAddress.ToString(), userData.UserId);
                _context.SaveChanges();
                return new { success = true, data = usersData };
            }
            else 
            { 
                message = "No user with that user_token."; 
            }
            return Return500Error(message);
        }
        /// <summary>
        /// Select list of chats. Get last message data, user's data of chat and chat data.
        /// </summary>
        /// <param name="request">Request.</param>
        [HttpPut]
        [ActionName("SelectChatsByGender")]
        public ActionResult<dynamic> SelectChatsByGender(JObject json)
        {
            int page = 0;
            string message = null;
            JToken userToken = jsonHandler.handle(ref json, "user_token", JTokenType.String, ref message);
            if (userToken != null)
            {
                var user = (from u in _context.Users 
                join p in _context.Profiles on u.UserId equals p.UserId
                where u.UserToken == userToken.ToString()
                select new { UserId = u.UserId, ProfileGender = p.ProfileGender } ).FirstOrDefault();
                if (user != null)
                {
                    JToken jPage = jsonHandler.handle(ref json, "page", JTokenType.String, ref message);
                    if (jPage != null)
                    {
                        page = jPage.ToObject<int>();
                    }
                    var data = (
                    from participant in _context.Participants
                    join users in _context.Users on participant.OpposideId equals users.UserId
                    join profile in _context.Profiles on users.UserId equals profile.UserId
                    join chats in _context.Chatroom on participant.ChatId equals chats.ChatId
                    join likesProfile in _context.LikeProfile on users.UserId equals likesProfile.ToUserId into likes
                    join messageChat in _context.Messages on chats.ChatId equals messageChat.ChatId into messages
                    join blockedUser in _context.BlockedUsers on users.UserId equals blockedUser.BlockedUserId into blockedUsers
                    where participant.UserId == user.UserId && profile.ProfileGender != user.ProfileGender
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
                        liked_user = likes.Any(l => l.UserId == user.UserId) ? true : false
                    }).Skip(page * 30).Take(30).ToList();           
                    Log.Info("Get list of chats.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId); 
                    _context.SaveChanges();
                    return new { success = true, data = data };
                }
                else 
                { 
                    message = "No user with that user_token."; 
                }
            }
            return Return500Error(message);
        }
        /// <summary>
        /// Select list of chats. Get last message data, user's data of chat and chat data.
        /// </summary>
        /// <param name="request">Request.</param>
        [HttpPost]
        [ActionName("LikeUnlikeUsers")]
        public ActionResult<dynamic> LikeUnlikeUsers(UserCache userCache)
        {
            string message = null;
            Users user = GetUserByToken(userCache.user_token, ref message);
            if (user != null)
            {
                Users opposideUser = _context.Users.Where(u 
                => u.UserPublicToken == userCache.opposide_public_token).FirstOrDefault();
                if (opposideUser != null)
                {
                    if (user.UserId != opposideUser.UserId)
                    {
                        LikeProfiles like = _context.LikeProfile.Where(l => l.UserId == user.UserId && l.ToUserId == opposideUser.UserId).FirstOrDefault();
                        if (like == null)
                        {
                            like = new LikeProfiles();
                            like.UserId = user.UserId;
                            like.ToUserId = opposideUser.UserId;
                            _context.LikeProfile.Add(like);
                        }
                        else
                        {
                            _context.LikeProfile.Remove(like);
                        }
                        _context.SaveChanges();
                        return new { success = true };                
                    }
                    else
                    {
                        message = "User can't like himself.";
                    }
                }
                else 
                { 
                    message = "Can't define opposide user by opposide_public_token."; 
                }
            }
            return Return500Error(message);
        }
        public Users GetUserByToken(string userToken, ref string message)
        {
            if (!string.IsNullOrEmpty(userToken))
            {
                Users user = _context.Users.Where(u => 
                u.UserToken == userToken).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by token.";
                }
                return user;
            }
            return null;
        }
    }
}