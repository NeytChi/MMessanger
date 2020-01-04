using Common;
using System;
using miniMessanger.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Instasoft
{
    public class Program
    {
        public static bool requestView = false;    
        public static string HostHttps;
        public static string HostHttp;
        public static void Main(string[] args)
        {
            using (Context context = new Context(true))
            {
                context.Database.EnsureCreated();
            }
            Config config = new Config();
            HostHttp = config.GetHostsUrl();
            HostHttps = config.GetHostsHttpsUrl();
            if (args != null)
            {                
                if (args.Length >= 1)
                {
                    if (args[0] == "-c")
                    {
                        using (Context context = new Context(true))
                        {
                            context.Database.EnsureDeleted();
                        }
                        Console.WriteLine("Database 'minimessanger' was deleted.");
                        return;
                    }
                    if (args[0] == "-v")
                    {
                        requestView = true;
                    }
                }
            }
            CreateWebHostBuilder(args).Build().Run();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) 
        => WebHost.CreateDefaultBuilder(args).UseUrls(HostHttp, HostHttps).UseStartup<Startup>();
    }
}
