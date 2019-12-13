using Common;
using System.Linq;
using miniMessanger;
using Newtonsoft.Json;
using miniMessanger.Models;
using miniMessanger.Manage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Controllers
{
    /// <summary>
    /// User functional for general movement. This class will be generate functional for user ability.
    /// </summary>
    [Route("v1.0/[controller]/[action]/")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private Context context;
        public Users users;
        public Chats chats;
        public Profiles profiles;
        public Authentication authentication;
        public Blocks blocks;
        public Validator Validator;
        public string AwsPath;
        
        public UsersController(Context context)
        {
            Config config = new Config();
            this.AwsPath = config.AwsPath;
            this.context = context;
            this.Validator = new Validator();
            this.users = new Users(context, Validator);
            this.chats = new Chats(context, users, Validator);
            this.profiles = new Profiles(context);
            this.blocks = new Blocks(users, context);
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
                            url_photo = user.Profile.UrlPhoto == null ? "" : AwsPath + user.Profile.UrlPhoto,
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
                user.Profile = profiles.UpdateProfile(
                    user.UserId, 
                    ref message, 
                    profile_photo, Request.Form["profile_gender"],
                    Request.Form["profile_age"], 
                    Request.Form["profile_city"]);
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
                url_photo = profile.UrlPhoto == null ? "" : AwsPath + profile.UrlPhoto,
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
            cache.count = cache.count == 0 ? 30 : cache.count;
            User user = users.GetUserByToken(cache.user_token, ref message);
            if (user != null)
            {
                return new 
                { 
                    success = true, 
                    data = users.GetUsers(user.UserId, cache.page, cache.count) 
                };
            }
            return Return500Error(message);
        }
        public dynamic UsersResponse(User user)
        {
            if (user != null)
            {
                return new 
                {
                    user_id = user.UserId,
                    user_email = user.UserEmail,
                    user_login = user.UserLogin,
                    created_at = user.CreatedAt,
                    last_login_at = user.LastLoginAt,
                    user_public_token = user.UserPublicToken
                };        
            }
            return null;
        }
        [HttpPut]
        [ActionName("SelectChats")]
        public ActionResult<dynamic> SelectChats(ChatCache cache)
        {
            string message = null;
            cache.count = cache.count == 0 ? 30 : cache.count;
            User user = users.GetUserByToken(cache.user_token, ref message);
            if (user != null)
            {
                return new 
                { 
                    success = true,
                    data = chats.GetChats(user.UserId, cache.page, cache.count) 
                };
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("SelectMessages")]
        public ActionResult<dynamic> SelectMessages(ChatCache cache)
        {
            string message = null;
            User user = users.GetUserByToken(cache.user_token, ref message);
            if (user != null)
            {
                dynamic messages = chats.GetMessages(
                    user.UserId, cache.chat_token, cache.page, cache.count, ref message);
                if (messages != null)
                {
                    return new { success = true, data = messages };
                }

            }
            return Return500Error(message);
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
        public ActionResult<dynamic> SendMessage(ChatCache cache)
        {
            string answer = null;
            Message message = chats.CreateMessage(
                cache.message_text, cache.user_token, cache.chat_token, ref answer);
            if (message != null)
            {
                return new 
                { 
                    success = true, 
                    data = chats.ResponseMessage(message) 
                };
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
                ChatCache cache  = JsonConvert.DeserializeObject<ChatCache>(data);
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
            if (blocks.BlockUser(cache.user_token, cache.opposide_public_token, cache.blocked_reason, ref message))
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
            string message = null;
            cache.count = cache.count == 0 ? 50 : cache.count;
            User user = users.GetUserByToken(cache.user_token, ref message);
            if (user != null)
            {
                var blockedUsers = blocks.GetBlockedUsers(user.UserId, cache.page, cache.count);
                return new 
                { 
                    success = true, 
                    data = blockedUsers 
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("UnblockUser")]
        public ActionResult<dynamic> UnblockUser(UserCache cache)
        {
            string message = null;
            if (blocks.UnblockUser(cache.user_token, cache.opposide_public_token, ref message))
            {
                return new 
                { 
                    success = true, message = "Unblock user - successed." 
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("ComplaintContent")]
        public ActionResult<dynamic> ComplaintContent(UserCache cache)
        {
            string message = null;   
            if (blocks.Complaint(cache.user_token, cache.message_id, cache.complaint, ref message))
            {
                Log.Info("Create complaint.", HttpContext.Connection.RemoteIpAddress.ToString());
                return new { success = true, message = "Complain content - successed." };
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("GetUsersByGender")]
        public ActionResult<dynamic> GetUsersByGender(UserCache cache)
        {
            string message = null;
            cache.count = cache.count == 0 ? 30 : cache.count;
            User user = users.GetUserWithProfile(cache.user_token, ref message);
            if (user != null)
            {
                var data =users.GetUsersByGender(user.UserId, user.Profile.ProfileGender, cache.page, cache.count);
                return new { success = true, data = data };
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("SelectChatsByGender")]
        public ActionResult<dynamic> SelectChatsByGender(UserCache cache)
        {
            string message = null;
            int count = cache.count == 0 ? 30 : cache.count;
            var user = users.GetUserWithProfile(cache.user_token, ref message);
            if (user != null)
            {
                var data = chats.GetChatsByGender(user.UserId, user.Profile.ProfileGender, cache.page, cache.count);
                return new { success = true, data = data };
            }
            return Return500Error(message);
        }
        [HttpPut]
        [ActionName("ReciprocalUsers")]
        public ActionResult<dynamic> ReciprocalUsers(UserCache cache)
        {
            string message = null;
            cache.count = cache.count == 0 ? 30 : cache.count;
            User user = users.GetUserWithProfile(cache.user_token, ref message);
            if (user != null)
            {
                dynamic data = users.ReciprocalUsers(user.UserId, user.Profile.ProfileGender, cache.page, cache.count);
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