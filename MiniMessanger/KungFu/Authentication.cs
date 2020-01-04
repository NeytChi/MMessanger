using System;
using Common;
using Serilog;
using System.Linq;
using Serilog.Core;
using miniMessanger.Models;

namespace miniMessanger
{
    public class Authentication
    {
        public Context context;
        public Validator validator;
        public MailF mail;
        public Logger log;
        public string IP;
        public int PORT;
        public Authentication(Context context, Validator validator)
        {
            this.context = context;
            this.validator = validator;
            mail = new MailF();
            Config config = new Config();
            IP = config.IP;
            PORT = config.Port;
            log = new LoggerConfiguration()
            .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        }
        public User Login(string UserEmail, string UserPassword, ref string message)
        {
            User user = GetActiveUserByEmail(UserEmail, ref message);
            if (user != null)
            {
                if (validator.VerifyHashedPassword(user.UserPassword, UserPassword))
                {
                    user.LastLoginAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    context.User.Update(user);
                    context.SaveChanges();
                    user.Profile = CreateIfNotExistProfile(user.UserId);
                    log.Information("User login, id -> " + user.UserId);
                    return user;
                }
                else 
                { 
                    message = "Wrong password."; 
                }
            }
            return null;
        }
        public bool LogOut(string UserToken, ref string message)
        {
            User user = GetUserByToken(UserToken, ref message);
            if (user != null)
            {
                user.UserToken = validator.GenerateHash(40);
                context.User.Update(user);
                context.SaveChanges();
                log.Information("User log out, id -> " + user.UserId);
                return true;
            }   
            return false;
        }
        public bool RecoveryPassword(string UserEmail, ref string message)
        {
            User user = GetActiveUserByEmail(UserEmail, ref message);
            if (user != null)
            {
                user.RecoveryCode = validator.random.Next(100000, 999999);
                context.User.Update(user);
                context.SaveChanges();
                mail.SendEmail(user.UserEmail, "Recovery password", "Recovery code=" + user.RecoveryCode);
                log.Information("Recovery password, id -> " + user.UserId);
                return true;
            }
            return false;
        }
        public bool ConfirmEmail(string UserEmail, ref string message)
        {
            User user = GetUserByEmail(UserEmail, ref message);
            if (user != null)
            {
                if (!user.Deleted && user.Activate == 0)
                {                            
                    SendConfirmEmail(user.UserEmail, user.UserHash);
                    Log.Information("Send registration email to user, id -> " + user.UserId);
                    return true;
                }
                else 
                { 
                    message = "Unknow email -> " + user.UserEmail + "."; 
                }
            }
            return false;
        }
        public User GetUserByToken(string userToken, ref string message)
        {
            if (!string.IsNullOrEmpty(userToken))
            {
                User user = context.User.Where(u 
                => u.UserToken == userToken
                && u.Activate == 1
                && !u.Deleted).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by token.";
                }
                return user;
            }
            return null;
        }
        public string CheckRecoveryCode(string UserEmail, int RecoveryCode, ref string message)
        {
            User user = GetActiveUserByEmail(UserEmail, ref message);
            if (user != null)
            {
                if (user.RecoveryCode == RecoveryCode)
                {
                    user.RecoveryToken = validator.GenerateHash(40);
                    user.RecoveryCode = 0;
                    context.User.Update(user);
                    context.SaveChanges();
                    Log.Information("Check recovery code - successed, id -> " + user.UserId);
                    return user.RecoveryToken;
                }
                else 
                {
                    message = "Wrong code."; 
                }
            }
            return null;
        }
        public bool ChangePassword(string RecoveryToken, string Password, string ConfirmPassword, ref string message)
        {
            User user = GetUserByRecoveryToken(RecoveryToken, ref message);
            if (user != null)
            {
                if (Password.Equals(ConfirmPassword))
                {
                    if (validator.ValidatePassword(Password, ref message))
                    {
                        user.UserPassword = validator.HashPassword(Password);
                        user.RecoveryToken  = "";
                        context.User.Update(user);
                        context.SaveChanges();
                        log.Information("Change user password, id ->", user.UserId);
                        return true;
                    }
                    message = "Incorrect password. " + message; 
                }
                else 
                { 
                    message = "Passwords are not match to each other."; 
                }
            }
            return false;
        }
        public User GetUserByRecoveryToken(string RecoveryToken, ref string message)
        {
            User user = context.User.Where(u
            => u.RecoveryToken == RecoveryToken
            && !u.Deleted 
            && u.Activate == 1).FirstOrDefault();
            if (user == null)
            {
                message = "Server can't define user by recovery token";
            }
            return user;
        }
        public User Registrate(UserCache cache, ref string message)
        {
            if (validator.ValidateUser(cache, ref message))
            {
                User user = GetUserByEmail(cache.user_email, ref message);
                if (user == null)
                {
                    user = CreateUser(cache.user_email, cache.user_login, cache.user_password);
                    message = "User account was successfully registered. See your email to activate account by link.";
                    return user;
                }
                else
                {
                    if (RestoreUser(user, ref message))
                    {
                        message = "User account was successfully restored.";
                        return user;
                    }
                }
            }
            return null;
        }
        /// <exception>
        /// You can't create a new user with the same email in database. You need to check out it before use this method.
        /// </exception>
        public User CreateUser(string UserEmail, string UserLogin, string UserPassword)
        {
            if (!string.IsNullOrEmpty(UserEmail) && !string.IsNullOrEmpty(UserLogin) && !string.IsNullOrEmpty(UserPassword))
            {
                User user = new User()
                {
                    UserEmail = UserEmail,
                    UserLogin = UserLogin,
                    UserPassword = validator.HashPassword(UserPassword),
                    UserHash = validator.GenerateHash(100),
                    Activate = 0,
                    Deleted = false,
                    CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    LastLoginAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    UserToken = validator.GenerateHash(40),
                    UserPublicToken = validator.GenerateHash(20),
                    ProfileToken = validator.GenerateHash(50)
                };
                context.User.Add(user);
                context.SaveChanges();
                SendConfirmEmail(user.UserEmail, user.UserHash);
                log.Information("Registrate new user, id -> " + user.UserId);
                return user;
            }
            return null;
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
                    log.Information("Restored old user, id -> " + user.UserId);
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
        public User GetActiveUserByEmail(string UserEmail, ref string message)
        {
            if (!string.IsNullOrEmpty(UserEmail))
            {
                User user = context.User.Where(u 
                => u.UserEmail == UserEmail
                && u.Activate == 1
                && !u.Deleted).FirstOrDefault();
                if (user == null)
                {
                    message = "Server can't define user by email.";
                }
                return user;
            }
            return null;
        }
        public bool Activate(string UserHash, ref string message)
        {
            User user = context.User.Where(u 
            => u.UserHash == UserHash
            && !u.Deleted
            && u.Activate == 0).FirstOrDefault();
            if (user != null)
            {
                user.Activate = 1;
                context.User.Update(user);
                context.SaveChanges();
                log.Information("Active user account, id -> " + user.UserId);
                return true;
            }
            else 
            { 
                message = "Server can't define user by hash."; 
            }
            return false;
        }
        public bool Delete(string UserToken, ref string message)
        {
            User user = GetUserByToken(UserToken, ref message);
            if (user != null)
            {
                user.Deleted = true;
                user.UserToken = null;
                context.User.Update(user);
                context.SaveChanges();
                log.Information("Account was successfully deleted, id ->" + user.UserId);
                return true;
            }
            return false;
        }
        public void SendConfirmEmail(string email, string userhash)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(userhash))
            {
                mail.SendEmail(email, "Confirm account", 
                "Confirm account: <a href=http://" + IP + ":" + PORT
                + "/v1.0/users/Activate/?hash=" + userhash + ">Confirm url!</a>");
            }
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
    }
}