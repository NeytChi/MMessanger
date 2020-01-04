using Serilog;
using System.IO;
using System.Net;
using Serilog.Core;
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
        private string UrlRedirect = "";
        private string UrlCheck = "";
        private Context context;
        public Logger log = new LoggerConfiguration()
            .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
		
        public ManagerController(Context context)
        {
            this.context = context;
            Config config = new Config();
            UrlRedirect = config.urlRedirect;
            UrlCheck = config.urlCheck;
        }
        [HttpGet]
        [ActionName("State")]
        public ActionResult<dynamic> State()
        {
            bool result = CheckUrlState();
            log.Information("Return state urls, IP -> " 
            + HttpContext.Connection.RemoteIpAddress.ToString());
            return ReturnStateUrl(result);
        }
        public dynamic ReturnStateUrl(bool result)
        {
            return new 
            { 
                success = result,
                data = new 
                {
                    url = result ? UrlRedirect : ""
                }
            };    
        }
        public bool CheckUrlState()
        {
            string result = GetRequest(UrlCheck);
            if (result != null)
            {
                JObject json = JsonConvert.DeserializeObject<JObject>(result);
                if (json.ContainsKey("success") 
                && json["success"].Type == JTokenType.Boolean)
                {
                    log.Information("Check url state");
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
                log.Information("Send GET request");
                return result;
            }
            return null;
        }
        public dynamic Return500Error(string message)
        {
            log.Warning(message + " IP -> "
             + HttpContext.Connection.LocalIpAddress.ToString());
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            return new { success = false, message = message, };
        }
    }
}