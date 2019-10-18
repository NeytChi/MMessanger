using Common;
using System.Linq;
using miniMessanger.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

namespace Common.Functional.UserF
{
    /// <summary>
    /// User functional for general movement. This class will be generate functional for user ability.
    /// </summary>
    [Microsoft.AspNetCore.Mvc.Route("v1.0/[controller]/[action]/")]
    [Microsoft.AspNetCore.Mvc.ApiController]
    public class UsersController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private string awsPath = "none";
        private System.DateTime unixed = new System.DateTime(1970, 1, 1, 0, 0, 0);
        private miniMessanger.Models.MMContext _context;
        private Controllers.JsonVariableHandler jsonHandler;
        public UsersController(miniMessanger.Models.MMContext _context)
        {
            this._context = _context;
            this.awsPath = Common.Config.AwsPath;
            jsonHandler = new Controllers.JsonVariableHandler();
        }
        /// <summary>
        /// Registration user with user_email and user_password.
        /// </summary>
        /// <param name="user">User data for registration.</param>
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("Registration")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> Registration(Newtonsoft.Json.Linq.JObject json)
        {
            string message = string.Empty;
            Newtonsoft.Json.Linq.JToken userEmail = jsonHandler.handle(ref json, "user_email",  Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userEmail != null)
            {
                Newtonsoft.Json.Linq.JToken userLogin = jsonHandler.handle(ref json, "user_login",  Newtonsoft.Json.Linq.JTokenType.String, ref message);
                if (userLogin != null)
                {
                    Newtonsoft.Json.Linq.JToken userPassword = jsonHandler.handle(ref json, "user_password",  Newtonsoft.Json.Linq.JTokenType.String, ref message);
                    if (userPassword != null)
                    {
                        if (Common.Validator.ValidateLogin(userLogin.ToString(), ref message))
                        {
                            if (Common.Validator.ValidateEmail(userEmail.ToString()))
                            {
                                if (Common.Validator.ValidatePassword(userPassword.ToString(), ref message))
                                {
                                    miniMessanger.Models.Users user = _context.Users.Where(u => u.UserEmail == userEmail.ToString()).FirstOrDefault();
                                    if (user == null)
                                    {
                                        user = new Users();
                                        user.UserEmail = userEmail.ToString();
                                        user.UserLogin = userLogin.ToString();
                                        user.UserPassword = Common.Validator.HashPassword(userPassword.ToString());
                                        user.UserHash = Common.Validator.GenerateHash(100);
                                        user.CreatedAt = (int)(System.DateTime.Now - Common.Config.unixed).TotalSeconds;
                                        user.Activate = 0;
                                        user.Deleted = false;
                                        user.LastLoginAt = user.CreatedAt;
                                        user.UserToken = Common.Validator.GenerateHash(40);
                                        user.UserPublicToken = Common.Validator.GenerateHash(20);
                                        user.ProfileToken = Common.Validator.GenerateHash(50);
                                        _context.Users.Add(user);
                                        _context.SaveChanges();
                                        Common.MailF.SendEmail(user.UserEmail, "Confirm account", "Confirm account: <a href=http://" + Config.IP + ":" + Config.Port + "/v1.0/users/Activate/?hash=" + user.UserHash + ">Confirm url!</a>");
                                        Common.Log.Info("Registrate new user.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
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
                                            user.UserToken = Common.Validator.GenerateHash(40); 
                                            _context.Users.Update(user);
                                            _context.SaveChanges();
                                            Common.Log.Info("Restored old user, user_id->" + user.UserId + ".", HttpContext.Connection.LocalIpAddress.ToString(), user.UserId);
                                            return new { success = true, message = "User account was successfully restored." };
                                        }
                                        else 
                                        {
                                            message =  "Have exists account with email ->" + user.UserEmail + ".";
                                            Common.Log.Warn("Have exists account with email ->" + user.UserEmail + ".", HttpContext.Connection.RemoteIpAddress.ToString()); 
                                        }  
                                    }
                                }
                                else
                                {
                                    Common.Log.Warn(message + " UserEmail->" + userEmail.ToString() + ".", HttpContext.Connection.RemoteIpAddress.ToString());                        
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
                        Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
                    }
                }
                else
                {
                    Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
                }
            }
            else
            {
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            Common.Log.Warn(message, HttpContext.Connection.LocalIpAddress.ToString());
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            return new { success = false, message = message, };
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("RegistrationEmail")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> RegistrationEmail(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userEmail = jsonHandler.handle(ref json, "user_email",  Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userEmail != null)
            {
                miniMessanger.Models.Users user = _context.Users.Where(u => u.UserEmail == userEmail.ToString()).FirstOrDefault();;
                if (user != null)
                {
                    if (!user.Deleted)
                    {
                        Common.MailF.SendEmail(user.UserEmail, "Confirm account", "Confirm account url: <a href=http://" + Config.IP + ":" + Config.Port + "/v1.0/users/Activate/?hash=" + user.UserHash + ">Confirm url!</a>");
                        Common.Log.Info("Send registration email to user.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        return new { success = true, message = "Send confirm email to user." };
                    }
                    else 
                    { 
                        message = "Unknow email -> " + user.UserEmail + "."; 
                        Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    }
                }
                else 
                { 
                    message = "Can't define user data by json's key 'user_email'."; 
                    Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [Microsoft.AspNetCore.Mvc.ActionName("Login")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> Login(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userEmail = jsonHandler.handle(ref json, "user_email",  Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userEmail != null)
            {
                Newtonsoft.Json.Linq.JToken userPassword = jsonHandler.handle(ref json, "user_password",  Newtonsoft.Json.Linq.JTokenType.String, ref message);
                if (userPassword != null)
                {
                    miniMessanger.Models.Users user_data = _context.Users.Where(u=> u.UserEmail == userEmail.ToString()).FirstOrDefault();
                    if (user_data != null)
                    {
                        if (Common.Validator.VerifyHashedPassword(user_data.UserPassword, userPassword.ToString()))
                        {
                            if (user_data.Activate == 1 && user_data.Deleted == false)
                            {
                                user_data.LastLoginAt = (int)(System.DateTime.Now - Common.Config.unixed).TotalSeconds;
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
                                Common.Log.Info("User login.", HttpContext.Connection.RemoteIpAddress.ToString(), user_data.UserId);
                                return new 
                                { 
                                    success = true, data = new 
                                    { 
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
                                Common.MailF.SendEmail(user_data.UserEmail, "Confirm account", "Confirm account: <a href=http://" + Config.IP + ":" + Config.Port + "/v1.0/users/Activate/?hash=" + user_data.UserHash + ">Confirm url!</a>");
                                message =  "User's account isn't confirmed."; 
                                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString(), user_data.UserId);
                            }
                        }
                        else 
                        { 
                            message = "Wrong password."; 
                            Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString(), user_data.UserId);
                        }
                    }
                    else 
                    { 
                        message = "No user with such email."; 
                        Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
                    }
                }
                else
                {
                    Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
                }
            }
            else
            {
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [Microsoft.AspNetCore.Mvc.ActionName("LogOut")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> LogOut(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userToken != null)
            {
                miniMessanger.Models.Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                if (user != null)
                {
                    user.UserToken = Common.Validator.GenerateHash(40);
                    _context.Users.Update(user);
                    _context.SaveChanges();
                    Common.Log.Info("User log out.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    return new { success = true, message = "Log out is successfully." };
                }
                else 
                { 
                    message = "Server can't get user's data by user_token from json."; 
                }  
            } 
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("RecoveryPassword")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> RecoveryPassword(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userEmail = jsonHandler.handle(ref json, "user_email", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userEmail != null)
            {
                miniMessanger.Models.Users user = _context.Users.Where(u => u.UserEmail == userEmail.ToString()).FirstOrDefault();
                if (user != null || !user.Deleted)
                {
                        user.RecoveryCode = Common.Validator.random.Next(100000, 999999);
                        Common.MailF.SendEmail(user.UserEmail, "Recovery password", "Recovery code=" + user.RecoveryCode);
                        _context.Users.Update(user);
                        _context.SaveChanges();
                        Common.Log.Info("Recovery password.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        return new { success = true, message = "Recovery password. Send message with code to email=" + user.UserEmail + "." };
                }
                else 
                { 
                    message = "Unknow email."; 
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("CheckRecoveryCode")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> CheckRecoveryCode(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userEmail = jsonHandler.handle(ref json, "user_email", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userEmail != null)
            {
                Newtonsoft.Json.Linq.JToken recoveryCode = jsonHandler.handle(ref json, "recovery_code", Newtonsoft.Json.Linq.JTokenType.Integer, ref message);
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
                                Common.Log.Info("Check recovery code - successed.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
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
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("ChangePassword")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> ChangePassword(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken recoveryToken = jsonHandler.handle(ref json, "recovery_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (recoveryToken != null)
            {
                Newtonsoft.Json.Linq.JToken userPassword = jsonHandler.handle(ref json, "user_password", Newtonsoft.Json.Linq.JTokenType.String, ref message);
                if (userPassword != null)
                {
                    Newtonsoft.Json.Linq.JToken userConfirmPassword = jsonHandler.handle(ref json, "user_confirm_password", Newtonsoft.Json.Linq.JTokenType.String, ref message);
                    if (userConfirmPassword != null)
                    {
                        Users user = _context.Users.Where(u=> u.RecoveryToken == recoveryToken.ToString()).FirstOrDefault();
                        if (user != null)
                        {
                            if (!user.Deleted)
                            {
                                if (userPassword.ToString().Equals(userConfirmPassword.ToString()))
                                {
                                    if (Common.Validator.ValidatePassword(userPassword.ToString(), ref message))
                                    {
                                        user.UserPassword = Common.Validator.HashPassword(userPassword.ToString());
                                        user.RecoveryToken  = "";
                                        _context.Users.Update(user);
                                        _context.SaveChanges();
                                        Common.Log.Info("Change user password.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
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
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.ActionName("Activate")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> Activate([Microsoft.AspNetCore.Mvc.FromQuery] string hash)
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
                    Common.Log.Info("Active user account.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    return new { success = true, message = "User account is successfully active." };
                }
                else { message = "No user with that email."; }
            }
            else { message = "Can't activate account. Unknow hash in request parameters."; }
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("Delete")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> Delete(Newtonsoft.Json.Linq.JObject json)
        { 
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userToken != null)
            {
                Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                if (user != null)
                {
                    user.Deleted = true;
                    user.UserToken = null;
                    _context.Users.Update(user);
                    _context.SaveChanges();
                    Common.Log.Info("Account was successfully deleted.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId); 
                    return new { success = true, message = "Account was successfully deleted." };
                }
                else 
                { 
                    message = "Server can't get user's by user_token."; 
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("UpdateProfile")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> UpdateProfile(Microsoft.AspNetCore.Http.IFormFile profile_photo)
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
                            if (System.IO.File.Exists(Common.Config.currentDirectory + profile.UrlPhoto))
                            {
                                System.IO.File.Delete(Common.Config.currentDirectory + profile.UrlPhoto);
                            }
                            System.IO.Directory.CreateDirectory(Common.Config.currentDirectory + "/ProfilePhoto/" +
                             System.DateTime.Now.Year + "-" + System.DateTime.Now.Month + "-" + System.DateTime.Now.Day);
                            profile.UrlPhoto = "/ProfilePhoto/" + System.DateTime.Now.Year + "-" + System.DateTime.Now.Month 
                            + "-" + System.DateTime.Now.Day + "/" + Validator.GenerateHash(10);
                            profile_photo.CopyTo(new System.IO.FileStream(Common.Config.currentDirectory + profile.UrlPhoto,
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
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
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
                            if (System.IO.File.Exists(Common.Config.currentDirectory + profile.UrlPhoto))
                            {
                                System.IO.File.Delete(Common.Config.currentDirectory + profile.UrlPhoto);
                            }
                            Directory.CreateDirectory(Common.Config.currentDirectory + "/ProfilePhoto/" +
                             DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day);
                            profile.UrlPhoto = "/ProfilePhoto/" + DateTime.Now.Year + "-" + DateTime.Now.Month 
                            + "-" + DateTime.Now.Day + "/" + Validator.GenerateHash(10);
                            profile_photo.CopyTo(new FileStream(Common.Config.currentDirectory + profile.UrlPhoto,
                            FileMode.Create));
                            Log.Info("Update profile photo.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                        }
                    }
                    _context.Profiles.Update(profile);
                    _context.SaveChanges();
                    Log.Info("Registrate profile.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
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
                    message = "No user with that profile_token."; 
                }
            }
            else 
            {
                message = "Request doesn't contains 'profile_token' key.";
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("Profile")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> Profile(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
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
                else 
                { 
                    message = "No user with that user_token."; 
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [Microsoft.AspNetCore.Mvc.ActionName("GetUsersList")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> GetUsersList(Newtonsoft.Json.Linq.JObject json)
        {
            int page = 0;
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userToken != null)
            {
                Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                if (user != null)
                {
                    Newtonsoft.Json.Linq.JToken jPage = jsonHandler.handle(ref json, "page", Newtonsoft.Json.Linq.JTokenType.String, ref message);
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
            if (Response != null)
            {
                Response.StatusCode = 500;
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            return new { success = false, message = message };
        }
        /// <summary>
        /// Select list of chats. Get last message data, user's data of chat and chat data.
        /// </summary>
        /// <param name="request">Request.</param>
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [Microsoft.AspNetCore.Mvc.ActionName("SelectChats")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> SelectChats(Newtonsoft.Json.Linq.JObject json)
        {
            int page = 0;
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userToken != null)
            {
                Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                if (user != null)
                {
                    Newtonsoft.Json.Linq.JToken jPage = jsonHandler.handle(ref json, "page", Newtonsoft.Json.Linq.JTokenType.String, ref message);
                    if (jPage != null)
                    {
                        page = jPage.ToObject<int>();
                    }
                    List<dynamic> chats = new List<dynamic>();
                    List<Participants> participants = _context.Participants.Where(p => p.UserId == user.UserId).ToList();
                    List<int> blocked = _context.BlockedUsers.Where(b => b.UserId == user.UserId && b.BlockedDeleted == false).Select(b => b.BlockedUserId).ToList();
                    foreach(Participants participant in participants)
                    {
                        if (!blocked.Contains(participant.OpposideId))
                        {
                            Chatroom room = _context.Chatroom.Where(ch => ch.ChatId == participant.ChatId).First();
                            Users opposide = _context.Users.Where(u => u.UserId == participant.OpposideId).First();
                            Messages last_message = _context.Messages.Where(m => m.ChatId == room.ChatId).OrderByDescending(m => m.MessageId).FirstOrDefault();
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
                else 
                { 
                    message = "No user with that user_token."; 
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [Microsoft.AspNetCore.Mvc.ActionName("SelectMessages")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> SelectMessages(Newtonsoft.Json.Linq.JObject json)
        {
            int page = 0;
            string messageReturn = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref messageReturn);
            if (userToken != null)
            {
                Newtonsoft.Json.Linq.JToken chatToken = jsonHandler.handle(ref json, "chat_token", Newtonsoft.Json.Linq.JTokenType.String, ref messageReturn);
                if (userToken != null)
                {
                    Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                    if (user != null)
                    {
                        Chatroom room = _context.Chatroom.Where(r => r.ChatToken == chatToken.ToString()).FirstOrDefault();
                        if (room != null)
                        {
                            Newtonsoft.Json.Linq.JToken jPage = jsonHandler.handle(ref json, "page", Newtonsoft.Json.Linq.JTokenType.String, ref messageReturn);
                            if (jPage != null)
                            {
                                page = jPage.ToObject<int>();
                            }
                            var messages = _context.Messages.Where(m => m.ChatId == room.ChatId)
                            .OrderByDescending(m => m.MessageId).Skip(page * 50).Take(50)
                            .Select(m => new { message_id = m.MessageId, chat_id = m.ChatId, user_id= m.UserId,
                            message_text = m.MessageText, message_viewed =m.MessageViewed, created_at = m.CreatedAt }).ToList(); 
                            (from m in _context.Messages where m.ChatId == room.ChatId && m.UserId != user.UserId select m).ToList().ForEach(m => m.MessageViewed = true);
                            _context.SaveChanges();
                            Log.Info("Get list of messages, chat_id->" + room.ChatId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId); 
                            return new { success = true, data = messages };
                        }
                        else 
                        { 
                            messageReturn = "Server can't define chat by chat_token."; 
                        }
                    }
                    else 
                    { 
                        messageReturn = "No user with that user_token."; 
                    }
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
                Common.Log.Warn(messageReturn, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            return new { success = false, message = messageReturn };
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("CreateChat")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> CreateChat(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userToken != null)
            {
                Newtonsoft.Json.Linq.JToken opposidePublicToken = jsonHandler.handle(ref json, "opposide_public_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
                if (userToken != null)
                {
                    Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                    if (user != null)
                    {
                        Chatroom room = new Chatroom();
                        room.users = new List<dynamic>();
                        Users interlocutor = _context.Users.Where(u => u.UserPublicToken == opposidePublicToken.ToString()).FirstOrDefault();
                        if (interlocutor != null)
                        {
                            Participants participant = _context.Participants.Where(p => p.OpposideId == interlocutor.UserId).FirstOrDefault();
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
                                Participants opposide_participant = new Participants();
                                opposide_participant.ChatId = room.ChatId;
                                opposide_participant.UserId = interlocutor.UserId;
                                opposide_participant.OpposideId = user.UserId;
                                _context.Participants.Add(opposide_participant);
                                Log.Info("Create chat for user_id->" + user.UserId + " and opposide_id->" + interlocutor.UserId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                            }
                            else
                            {
                                room = _context.Chatroom.Where(ch => ch.ChatId == participant.ChatId).First();
                                Log.Info("Select exist chat for user_id->" + user.UserId + " and opposide_id->" + interlocutor.UserId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                            }
                            Log.Info("Create/Select chat chat_id->" + room.ChatId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                            _context.SaveChanges();
                            return new 
                            { 
                                success = true, data = new 
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
                    else 
                    { 
                        message = "No user with that user_token."; 
                    }
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("SendMessage")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> SendMessage(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userToken != null)
            {
                Newtonsoft.Json.Linq.JToken chatToken = jsonHandler.handle(ref json, "chat_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
                if (chatToken != null)
                {
                    Newtonsoft.Json.Linq.JToken jMessageText = jsonHandler.handle(ref json, "message_text", Newtonsoft.Json.Linq.JTokenType.String, ref message);
                    if (jMessageText != null)
                    {
                        string messageText = System.Net.WebUtility.UrlDecode(jMessageText.ToString());
                        if (!string.IsNullOrEmpty(messageText))
                        {
                            Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                            if (user != null)
                            {
                                Chatroom room = _context.Chatroom.Where(ch => ch.ChatToken == chatToken.ToString()).FirstOrDefault();
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
                                    return new { success = true, data = new
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
                            else 
                            { 
                                message = "No user with that user_token."; 
                            }
                        } 
                        else 
                        { 
                            message = "Message is empty. Server willn't upload this message."; 
                        }
                    }
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("BlockUser")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> BlockUser(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userToken != null)
            {
                Newtonsoft.Json.Linq.JToken opposidePublicToken = jsonHandler.handle(ref json, "opposide_public_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
                if (opposidePublicToken != null)
                {
                    Newtonsoft.Json.Linq.JToken jBlockedReason = jsonHandler.handle(ref json, "blocked_reason", Newtonsoft.Json.Linq.JTokenType.String, ref message);
                    if (jBlockedReason != null)
                    {
                        string blockedReason = System.Net.WebUtility.UrlDecode(jBlockedReason.ToString());
                        Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                        if (user != null)
                        {
                            Users interlocutor = _context.Users.Where(u => u.UserPublicToken == opposidePublicToken.ToString()).FirstOrDefault();
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
                                        Log.Info("Block user; user->user_id->" + user.UserId + ".", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
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
                        else 
                        { 
                            message = "No user with that user_token."; 
                        }
                    }
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [Microsoft.AspNetCore.Mvc.ActionName("GetBlockedUsers")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> GetBlockedUsers(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
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
            if (Response != null)
            {
                Response.StatusCode = 500;
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("UnblockUser")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> UnblockUser(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userToken != null)
            {
                Newtonsoft.Json.Linq.JToken opposidePublicToken = jsonHandler.handle(ref json, "opposide_public_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
                if (userToken != null)
                {
                    Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                    if (user != null)
                    {
                        Users interlocutor = _context.Users.Where(u => u.UserPublicToken == opposidePublicToken.ToString()).FirstOrDefault();
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
                            else { message = "User didn't block current user; user->user_id->" + user.UserId + "."; }
                        }
                        else { message = "No user with that opposide_public_token; user->user_id->" + user.UserId + "."; }
                    }
                    else { message = "No user with that user_token."; }
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("ComplaintContent")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> ComplaintContent(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userToken != null)
            {
                Newtonsoft.Json.Linq.JToken messageId = jsonHandler.handle(ref json, "message_id", Newtonsoft.Json.Linq.JTokenType.Integer, ref message);
                if (messageId != null)
                {
                    Newtonsoft.Json.Linq.JToken jComplaint = jsonHandler.handle(ref json, "complaint", Newtonsoft.Json.Linq.JTokenType.String, ref message);
                    if (messageId != null)
                    {   
                        string complaint = System.Net.WebUtility.UrlDecode(jComplaint.ToString());
                        Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                        if (user != null)
                        {
                            Messages messageChat = _context.Messages.Where(m => m.MessageId == messageId.ToObject<long>()).FirstOrDefault();
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
                        else 
                        {
                            message = "No user with that user_token."; 
                        }
                    }
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            return new { success = false, message = message };
        }
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [Microsoft.AspNetCore.Mvc.ActionName("GetUsersByGender")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> GetUsersByGender(Newtonsoft.Json.Linq.JObject json)
        {
            int page = 0;
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userToken != null)
            {
                var user = (from u in _context.Users
                join p in _context.Profiles on u.UserId equals p.UserId
                where u.UserToken == userToken.ToString()
                select new { UserId = u.UserId, ProfileGender = p.ProfileGender } ).FirstOrDefault();
                if (user != null)
                {
                    Newtonsoft.Json.Linq.JToken jPage = jsonHandler.handle(ref json, "page", Newtonsoft.Json.Linq.JTokenType.Integer, ref message);
                    if (jPage != null)
                    {
                        page = jPage.ToObject<int>();
                    }
                    var usersData = (from users in _context.Users
                    join profile in _context.Profiles on users.UserId equals profile.UserId
                    join likesProfile in _context.LikeProfile on users.UserId equals likesProfile.ToUserId into likes
                    join blockedUser in _context.BlockedUsers on users.UserId equals blockedUser.BlockedUserId into blockedUsers
                    where users.UserId != user.UserId && profile.ProfileGender != user.ProfileGender
                    && users.Activate == 1
                    && (blockedUsers.All(b => b.UserId == user.UserId && b.BlockedDeleted == true)
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
                        liked_user = likes.Any(l => l.UserId == user.UserId) ? true : false
                    }).Skip(page * 30).Take(30).ToList();
                    Log.Info("Get users list.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                    _context.SaveChanges();
                    return new { success = true, data = usersData };
                }
                else 
                { 
                    message = "No user with that user_token."; 
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            return new { success = false, message = message };
        }
        /// <summary>
        /// Select list of chats. Get last message data, user's data of chat and chat data.
        /// </summary>
        /// <param name="request">Request.</param>
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [Microsoft.AspNetCore.Mvc.ActionName("SelectChatsByGender")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> SelectChatsByGender(Newtonsoft.Json.Linq.JObject json)
        {
            int page = 0;
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userToken != null)
            {
                var user = (from u in _context.Users 
                join p in _context.Profiles on u.UserId equals p.UserId
                where u.UserToken == userToken.ToString()
                select new { UserId = u.UserId, ProfileGender = p.ProfileGender } ).FirstOrDefault();
                if (user != null)
                {
                    Newtonsoft.Json.Linq.JToken jPage = jsonHandler.handle(ref json, "page", Newtonsoft.Json.Linq.JTokenType.String, ref message);
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
            if (Response != null)
            {
                Response.StatusCode = 500;
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            return new { success = false, message = message };
        }
        /// <summary>
        /// Select list of chats. Get last message data, user's data of chat and chat data.
        /// </summary>
        /// <param name="request">Request.</param>
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ActionName("LikeUnlikeUsers")]
        public Microsoft.AspNetCore.Mvc.ActionResult<dynamic> LikeUnlikeUsers(Newtonsoft.Json.Linq.JObject json)
        {
            string message = null;
            Newtonsoft.Json.Linq.JToken userToken = jsonHandler.handle(ref json, "user_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
            if (userToken != null)
            {
                Newtonsoft.Json.Linq.JToken opposidePublicToken = jsonHandler.handle(ref json, "opposide_public_token", Newtonsoft.Json.Linq.JTokenType.String, ref message);
                if (opposidePublicToken != null)
                {
                    Users user = _context.Users.Where(u => u.UserToken == userToken.ToString()).FirstOrDefault();
                    if (user != null)
                    {
                        Users opposideUser = _context.Users.Where(u => u.UserPublicToken == opposidePublicToken.ToString()).FirstOrDefault();
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
                    else 
                    { 
                        message = "No user with that user_token."; 
                    }
                }
            }
            if (Response != null)
            {
                Response.StatusCode = 500;
                Common.Log.Warn(message, HttpContext.Connection.RemoteIpAddress.ToString());
            }
            return new { success = false, message = message };
        }
    }
}