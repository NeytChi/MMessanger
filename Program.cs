using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Instasoft
{
    public class Program
    {
        public static bool request_view = false;    
        public static void Main(string[] args)
        {
            using (miniMessanger.Models.MMContext context = new miniMessanger.Models.MMContext(true))
            {
                context.Database.EnsureCreated();
            }
            Common.Log.Info("Start server program.");
            Common.Config.Initialization();
            if (args != null)
            {                
                if (args.Length >= 1)
                {
                    if (args[0] == "-c")
                    {
                        using (miniMessanger.Models.MMContext context = new miniMessanger.Models.MMContext(true))
                        {
                            context.Database.EnsureDeleted();
                        }
                        System.Console.WriteLine("Database 'Instasoft' was deleted.");
                        return;
                    }
                    if (args[0] == "-v")
                    {
                        request_view = true;
                    }
                }
            }
            Common.MailF.Init();
            CreateWebHostBuilder(args).Build().Run();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args).UseUrls(Common.Config.GetHostsUrl(), Common.Config.GetHostsHttpsUrl()).UseStartup<Startup>();
    }
}
