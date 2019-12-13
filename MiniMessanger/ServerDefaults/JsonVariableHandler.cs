using Newtonsoft.Json.Linq;

namespace Controllers
{
    public class JsonVariableHandler
    {
        public JToken handle(ref JObject json, string name, JTokenType tokenType, ref string message)
        {
            if (json.ContainsKey(name))
            {
                JToken jsonVarible = json.GetValue(name);
                if (jsonVarible.Type == tokenType)
                {
                    return jsonVarible;
                }
                else 
                { 
                    message = "Json key -> '" + name + "' has incorrect type. Required type -> " + tokenType.ToString() + "."; 
                }
            }   
            else 
            { 
                message = "Json without key -> '"+ name + "'."; 
            }
            return null;
        }
    }
}