using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Controllers
{
    public static class AuthOptions
    {
        public static string ISSUER = Common.Config.GetServerConfigValue("issuer", Newtonsoft.Json.Linq.JTokenType.String);
        public static string AUDIENCE = Common.Config.GetServerConfigValue("audience", Newtonsoft.Json.Linq.JTokenType.String);
        private static string KEY = Common.Config.GetServerConfigValue("auth_key", Newtonsoft.Json.Linq.JTokenType.String);
        public static int LIFETIME = Common.Config.GetServerConfigValue("auth_lifetime", Newtonsoft.Json.Linq.JTokenType.Integer);
        public static Microsoft.IdentityModel.Tokens.SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(KEY));
        }
    }
    /// <summary>
    /// The functional part of the admin panel.
    /// </summary>
    [Microsoft.AspNetCore.Mvc.Route("v1.0/[controller]/[action]/")]
    [Microsoft.AspNetCore.Mvc.ApiController]
    public class AdminController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly miniMessanger.Models.MMContext _context;

        public AdminController(miniMessanger.Models.MMContext _context)
        {
            this._context = _context;
        }
    }
}