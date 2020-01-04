using System;
using Serilog;
using System.Linq;
using Serilog.Core;
using miniMessanger.Models;
using Microsoft.AspNetCore.Http;

namespace miniMessanger
{
    public class Profiles
    {
        public Context context;
        public FileSaver fileSystem = new FileSaver();
        public Logger log;
        public Profiles(Context context)
        {
            this.context = context;
            log = new LoggerConfiguration()
            .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        }
        public Profile UpdateProfile (
            int userId,
            ref string message, 
            IFormFile photo = null, 
            string profileGender = null,
            string profileCity = null,
            string profileAge = null)
        {
            Profile profile = CreateIfNotExistProfile(userId);
            if (UpdateGender(profile, profileGender, ref message))
            {
                if (UpdateAge(profile, profileAge, ref message))
                {
                    if (UpdateCity(profile, profileCity, ref message))
                    {
                        if (UpdatePhoto(photo, profile, ref message))
                        {
                            log.Information("Update profile, id -> " + userId);
                            return profile;
                        }
                    }
                }
            }
            return null;
        }
        public bool UpdateGender(Profile profile, string profileGender, ref string message)
        {
            if (profileGender != null)
            {
                if (profileGender == "1")
                {
                    profile.ProfileGender = true;
                }
                else if (profileGender == "0")
                {
                    profile.ProfileGender = false;
                }
                else
                {
                    message ="Incorrect value in variable profile gender, id -> " + profile.UserId;
                    log.Warning(message);
                    return false;
                }
                context.Profile.Update(profile);
                context.SaveChanges();
                log.Information("Update profile gender, id -> ", profile.ProfileId);
            }
            return true;
        }
        public bool UpdateAge(Profile profile, string profileAge, ref string message)
        {
            if (profileAge != null)
            {
                short ProfileAge = 0;
                if (Int16.TryParse(profileAge, out ProfileAge))
                {
                    if (ProfileAge > 0 && ProfileAge < 200)
                    {
                        profile.ProfileAge = (sbyte)ProfileAge;
                        context.Profile.Update(profile);
                        context.SaveChanges();
                        log.Information("Update profile age, id -> ", profile.UserId);
                        return true;
                    }
                    message = "Profile age can't be more that 200 and less that 0.";
                }
                else
                {
                    message = "Server can't convert profile age to short type.";
                }
                return false;
            }
            return true;
        }
        public bool UpdateCity(Profile profile, string profileCity, ref string message)
        {
            if (profileCity != null)
            {
                if (profileCity.Length > 3 && profileCity.Length < 50)
                {
                    profile.ProfileCity = profileCity;
                    context.Profile.Update(profile);
                    context.SaveChanges();
                    log.Information("Update profile city, id -> ", profile.UserId);
                    return true;
                }
                message = "Parameter 'profile_city' can't has more that 50 charaters and less that 3.";
                return false;
            }
            return true;
        }
        public bool UpdatePhoto(IFormFile photo, Profile profile, ref string message)
        {
            if (photo != null)
            {
                if (photo.ContentType.Contains("image"))
                {
                    fileSystem.DeleteFile(profile.UrlPhoto);
                    profile.UrlPhoto = fileSystem.CreateFile(photo, "/ProfilePhoto/");
                    context.Profile.Update(profile);
                    context.SaveChanges();
                    log.Information("Update profile photo, id -> ", profile.UserId);
                    return true;
                }
                else
                {
                    message = "Wrong type of file. Required type of file is image.";
                }
                return false;
            }
            return true;
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