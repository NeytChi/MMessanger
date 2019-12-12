using Common;
using System.Text;
using miniMessanger.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Controllers
{
    public class AuthOptions
    {
        public AuthOptions(Config config)
        {
            config.Initialization();
            ISSUER = config.GetServerConfigValue("issuer", JTokenType.String);
            AUDIENCE = config.GetServerConfigValue("audience", JTokenType.String);
            KEY = config.GetServerConfigValue("auth_key", JTokenType.String);
            LIFETIME = config.GetServerConfigValue("auth_lifetime", JTokenType.Integer);
        }
        public string ISSUER;
        public string AUDIENCE;
        private string KEY;
        public int LIFETIME;
        public SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
    /// <summary>
    /// The functional part of the admin panel.
    /// </summary>
    [Route("v1.0/[controller]/[action]/")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly Context context;

        public AdminController(Context context)
        {
            this.context = context;
        }
    }
}