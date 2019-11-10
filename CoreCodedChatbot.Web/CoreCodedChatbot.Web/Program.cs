
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

using CoreCodedChatbot.Database.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


namespace CoreCodedChatbot.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .Build();

            BuildHost(args, config).Run();
        }

        public static IHost BuildHost(string[] args, IConfigurationRoot config) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseUrls(config["server.urls"]);
                    builder.PreferHostingUrls(true);
                    builder.UseStartup<Startup>();
                })
                .Build();
    }
}
