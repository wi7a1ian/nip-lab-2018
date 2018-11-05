using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nip.Blog.Services.Posts.API.Data;

namespace Nip.Blog.Services.Posts.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().InitializeDatabase().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("dbsettings.json", optional: false, reloadOnChange: true);
                    // Never store passwords or other sensitive data in source code or config files.
                    // To access DB either run app in user mode (Individual User Accounts) or use Secret Manager tool (> secrets.json)
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddFile("Logs/blogposts-{Date}.log");
                });
    }

    internal static class ConfigureWebHostHelper
    {
        public static IWebHost InitializeDatabase(this IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var env = services.GetRequiredService<IHostingEnvironment>();
                    var context = services.GetRequiredService<BlogPostContext>();
                    BlogPostContextInitializer.Initialize(context, env);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            return host;
        }
    }
}
