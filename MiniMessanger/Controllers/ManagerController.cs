using System.IO;
using System.Net;
using Newtonsoft.Json;
using miniMessanger.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Common
{
    [Route("v1.0/[controller]/[action]/")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private Context context;
        public ManagerController(Context context)
        {
            this.context = context;
        }
        [HttpGet]
        [ActionName("State")]
        public ActionResult<dynamic> State()
        {
            bool result = CheckUrlState();
            Log.Info("Return state urls.", HttpContext.Connection.RemoteIpAddress.ToString());
            return ReturnStateUrl(result);
        }
        public dynamic ReturnStateUrl(bool result)
        {
            return new 
            { 
                success = result,
                data = new 
                {
                    url = result ? Config.urlRedirect : ""
                }
            };    
        }
        public bool CheckUrlState()
        {
            string result = GetRequest(Config.urlCheck);
            if (result != null)
            {
                JObject json = JsonConvert.DeserializeObject<JObject>(result);
                if (json.ContainsKey("success") 
                && json["success"].Type == JTokenType.Boolean)
                {
                    Log.Info("Check url state.");
                    return json["success"].ToObject<bool>();
                }
            }
            return false;
        }
        public string GetRequest(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {            
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", 
                "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                Stream data = client.OpenRead(url);
                StreamReader reader = new StreamReader(data);
                string result = reader.ReadToEnd();
                data.Close();
                reader.Close();
                Log.Info("Send get request.");
                return result;
            }
            return null;
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