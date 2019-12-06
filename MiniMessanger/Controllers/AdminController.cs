using Common;
using System.Text;
using miniMessanger.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


namespace Controllers
{
    public static class AuthOptions
    {
        public static string ISSUER = Config.GetServerConfigValue("issuer", JTokenType.String);
        public static string AUDIENCE = Config.GetServerConfigValue("audience", JTokenType.String);
        private static string KEY = Config.GetServerConfigValue("auth_key", JTokenType.String);
        public static int LIFETIME = Config.GetServerConfigValue("auth_lifetime", JTokenType.Integer);
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
    /// <summary>
    /// The functional part of the admin panel.
    /// </summary>
    [Route("v1.0/[controller]/[action]/")]
    [ApiController]
    public class AdminController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly Context _context;

        public AdminController(Context _context)
        {
            this._context = _context;
        }
    }
}