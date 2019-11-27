using System;
using Controllers;
using System.Linq;
using miniMessanger.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Common;

namespace Common
{
    [Route("v1.0/[controller]/[action]/")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private MMContext context;
        public ManagerController(MMContext context)
        {
            this.context = context;
        }
        [HttpGet]
        [ActionName("State")]
        public ActionResult<dynamic> State([FromQuery] string userToken)
        {
            string message = string.Empty;
            Users user = context.Users.Where(u => u.UserToken == userToken).FirstOrDefault();
            if (user != null)
            {
                Log.Info("Return state urls.", HttpContext.Connection.RemoteIpAddress.ToString(), user.UserId);
                return new 
                { 
                    success = true,
                };
            }
            else
            {
                message = "Server can't define user by user token.";
            }
            return Return500Error(message);
        }
        public dynamic Return500Error(string message)
        {
            Log.Warn(message, HttpContext.Connection.LocalIpAddress.ToString());
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            return new { success = false, message = message, };
        }
    }
}