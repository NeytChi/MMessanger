using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Common
{
    public static class Config
    {
        public static DateTime unixed = new DateTime(1970, 1, 1, 0, 0, 0);
        public static JObject server_config;
        public static JObject database_config;
        public static string conf_name = "conf.json";
        public static string dbconf_name = "dbconf.json";
        public static string IP = "127.0.0.1";
        public static string Domen = "(none)";
        public static int Port = 8023;
        public static string AwsPath = "";
        public static string savePath = "";
        public static string urlCheck = "";
        public static string urlRedirect = "";
        /// <summary>
        /// Return of the path occurs without the last '/' (pointer to the directory) 
        /// </summary>
        public static string currentDirectory = Directory.GetCurrentDirectory();
        public static bool initiated = false;

        public static void Initialization()
        {
            initiated = true;
            FileInfo confExist = new FileInfo(currentDirectory + "/" + conf_name);
            FileInfo dbconfExist = new FileInfo(currentDirectory + "/" + dbconf_name);
            if (confExist.Exists && dbconfExist.Exists)
            {
                string confInfo = ReadConfigJsonData(conf_name);
                string dbconfInfo = ReadConfigJsonData(dbconf_name);
                server_config = JObject.Parse(confInfo);
                database_config = JObject.Parse(dbconfInfo);
                if (server_config != null && database_config != null)
                {
                    Port = GetServerConfigValue("port", JTokenType.Integer);
                    IP = GetServerConfigValue("ip", JTokenType.String);
                    Domen = GetServerConfigValue("domen", JTokenType.String);
                    AwsPath = GetServerConfigValue("aws_path", JTokenType.String);
                    savePath = GetServerConfigValue("save_path", JTokenType.String);
                    urlCheck = GetServerConfigValue("url_check", JTokenType.String);
                    urlRedirect = GetServerConfigValue("url_redirect", JTokenType.String);
                }
                else 
                {
                    Console.WriteLine("Start with default config setting.");
                }
            }
            else
            {
                Console.WriteLine("Start with default config setting.");
            }
        }
        private static string ReadConfigJsonData(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (var fstream = File.OpenRead(fileName))
                {
                    byte[] array = new byte[fstream.Length];
                    fstream.Read(array, 0, array.Length);
                    string textFromFile = System.Text.Encoding.Default.GetString(array);
                    fstream.Close();
                    return textFromFile;
                }
            }
            else
            {
                Console.WriteLine("Can not read file=" + fileName + " , function Config.ReadConfigJsonData()");
                return string.Empty;
            }
        }
        public static string GetHostsUrl()
        {
            string url_connection = null;
            if (!initiated)
            {
                Initialization();
            }
            if (server_config != null)
            {
                if (server_config.ContainsKey("ip")
                && server_config.ContainsKey("port"))
                {
                    
                    url_connection = "http://" + server_config["ip"].ToString() + ":" + 
                    server_config["port"].ToString() + "/"; 
                }
                else { Console.WriteLine("Can't create url_connetion_string, one of values doesn't exist."); }
            }
            else { Console.WriteLine("Server can't define conf.json; Can't get url_connetion_string."); }
            return url_connection;
        }
        public static string GetHostsHttpsUrl()
        {
            string url_connection = null;
            if (!initiated)
            {
                Initialization();
            }
            if (server_config != null)
            {
                if (server_config.ContainsKey("ip")
                && server_config.ContainsKey("port"))
                {
                    
                    url_connection = "https://" + server_config["ip"].ToString() + ":" + 
                    (server_config["port"].ToObject<int>() + 1) + "/"; 
                }
                else { Console.WriteLine("Can't create url_connetion_string, one of values doesn't exist."); }
            }
            else { Console.WriteLine("Server can't define conf.json; Can't get url_connetion_string."); }
            return url_connection;
        }
        public static string GetDatabaseConfigConnection()
        {
            string mysql_connection = null;
            if (!initiated)
            {
                Initialization();
            }
            if (database_config != null)
            {
                if (database_config.ContainsKey("Server")
                && database_config.ContainsKey("Database")
                && database_config.ContainsKey("User")
                && database_config.ContainsKey("Password"))
                {
                    mysql_connection = "Server=" + database_config["Server"].ToString() + ";" +
                    "Database=" + database_config["Database"].ToString() + ";" + 
                    "User=" + database_config["User"].ToString() + ";" + 
                    "Pwd=" + database_config["Password"].ToString() + ";Charset=utf8;";
                }
                else { Console.WriteLine("Can't create mysql_connetion_string, one of values doesn't exist."); }
            }
            else { Console.WriteLine("Server can't define dbconf.json; Can't get mysql_connetion_string."); }
            return mysql_connection;
        }
        public static dynamic GetServerConfigValue(string conf_name, JTokenType type_value)
        {
            if (!initiated)
            {
                Initialization();
            }
            if (server_config != null)
            {
                if (server_config.ContainsKey(conf_name))
                {
                    switch (type_value)
                    {
                        case JTokenType.Integer:
                            if (server_config[conf_name].Type == JTokenType.Integer) { return server_config[conf_name].ToObject<int>(); }
                            else { return -1; }
                        case JTokenType.String:
                            if (server_config[conf_name].Type == JTokenType.String) { return server_config[conf_name].ToObject<string>(); }
                            else { return ""; }
                        default:
                            Console.WriteLine("Can not get value, type of value not define, function GetConfigValue");
                            return null;
                    }
                }
                else { Console.WriteLine("Can not get value, json doesn't have this value, value=" + conf_name + ", function GetConfigValue"); }
            }
            else { Console.WriteLine("Can not get value, Json Object did not create, function GetConfigValue"); }
            switch (type_value)
            {
                case JTokenType.Integer: return -1;
                case JTokenType.String: return null;
                default: return null;
            }
        }
    }
}