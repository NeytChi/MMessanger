using System;
using Common;
using System.Linq;
using miniMessanger.Models;

namespace miniMessanger.Authentication
{
    public class Authentication
    {
        public Context context;
        public Validator validator;
        public Authentication(Context context, Validator validator)
        {
            this.context = context;
            this.validator = validator;
        }
        public dynamic Registrate(UserCache cache, ref string message)
        {
            if (validator.ValidateUser(cache, ref message))
            {
                User user = GetUserByEmail(cache.user_email, ref message);
                if (user == null)
                {
                    user = CreateUser(cache);
                    return RegistrationResponse(
                        "User account was successfully registered. See your email to activate account by link."
                        , user.ProfileToken);
                }
                else
                {
                    if (RestoreUser(user, ref message))
                    {
                        return RegistrationResponse("User account was successfully restored.", user.ProfileToken);
                    }
                }
            }
            return null;
        }
        public User CreateUser(UserCache cache)
        {
            User user = new User();
            user.UserEmail = cache.user_email;
            user.UserLogin = cache.user_login;
            user.UserPassword = validator.HashPassword(cache.user_password);
            user.UserHash = validator.GenerateHash(100);
            user.CreatedAt = (int)(DateTime.Now - Config.unixed).TotalSeconds;
            user.Activate = 0;
            user.Deleted = false;
            user.LastLoginAt = user.CreatedAt;
            user.UserToken = validator.GenerateHash(40);
            user.UserPublicToken = validator.GenerateHash(20);
            user.ProfileToken = validator.GenerateHash(50);
            context.User.Add(user);
            context.SaveChanges();
            SendConfirmEmail(user.UserEmail, user.UserHash);           
            Log.Info("Registrate new user.", user.UserId);
            return user;
        }
        public bool RestoreUser(User user, ref string message)
        {
            if (user != null)
            {
                if (user.Deleted == true)
                {
                    user.Deleted = false;
                    user.UserToken = validator.GenerateHash(40); 
                    context.User.Update(user);
                    context.SaveChanges();
                    Log.Info("Restored old user, user_id->" + user.UserId + ".", user.UserId);
                    return true;
                }
                else 
                {
                    message =  "Have exists account with email ->" + user.UserEmail + ".";
                }  
            }
            return false;
        }
        public User GetUserByEmail(string UserEmail, ref string message)
        {
            if (!string.IsNullOrEmpty(UserEmail))
            {
                User user = context.User.Where(u => u.UserEmail == UserEmail).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by email.";
                }
                return user;
            }
            return null;
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
        public User GetUserByPublicToken(string userPublicToken, ref string message)
        {
            User user = context.User.Where(u => u.UserPublicToken == userPublicToken).FirstOrDefault();
            if (user == null)
            {
                message = "Server can't define user by public token";
            }
            return user;
        }
        public void SendConfirmEmail(string UserEmail, string UserHash)
        {
            MailF.SendEmail(UserEmail, "Confirm account", 
            "Confirm account: <a href=http://" + Config.IP + ":" + Config.Port
            + "/v1.0/users/Activate/?hash=" + UserHash + ">Confirm url!</a>");    
        }
        public Profile CreateIfNotExistProfile(int UserId)
        {
            Profile profile = context.Profile.Where(p => p.UserId == UserId).FirstOrDefault();
            if (profile == null)
            {
                profile = new Profile();
                profile.UserId = UserId;
                profile.ProfileGender = true;
                context.Add(profile);
                context.SaveChanges();
            }
            return profile;
        }
        public User GetUserWithProfile(string userToken, ref string message)
        {
            if (!string.IsNullOrEmpty(userToken))
            {
                User user = context.User.Where(u => 
                u.UserToken == userToken).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by token.";
                }
                else
                {
                    user.Profile = context.Profile.Where(p 
                    => p.UserId == user.UserId).First();
                }
                return user;
            }
            return null;
        }
    }
}