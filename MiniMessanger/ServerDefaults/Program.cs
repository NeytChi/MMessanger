using Common;
using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using miniMessanger.Models;

namespace Instasoft
{
    public class Program
    {
        public static bool request_view = false;    
        public static void Main(string[] args)
        {
            using (Context context = new Context(true))
            {
                context.Database.EnsureCreated();
            }
            Log.Info("Start server program.");
            Config.Initialization();
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
                        Console.WriteLine("Database 'Instasoft' was deleted.");
                        return;
                    }
                    if (args[0] == "-v")
                    {
                        request_view = true;
                    }
                }
            }
            MailF.Init();
            CreateWebHostBuilder(args).Build().Run();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args).UseUrls(Common.Config.GetHostsUrl(), Common.Config.GetHostsHttpsUrl()).UseStartup<Startup>();
    }
}
