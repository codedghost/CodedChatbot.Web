using System;
using CoreCodedChatbot.ApiClient;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Database;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Interfaces;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using CoreCodedChatbot.Library;
using CoreCodedChatbot.Logging;
using CoreCodedChatbot.Printful;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.SignalRHubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace CoreCodedChatbot.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var configService = new ConfigService();

            services.AddOptions();
            services.AddMemoryCache();

            services
                .AddChatbotConfigService()
                .AddChatbotSecretServiceCollection(
                    configService.Get<string>("KeyVaultAppId"),
                    configService.Get<string>("KeyVaultCertThumbprint"),
                    configService.Get<string>("KeyVaultBaseUrl")
                );

            //var builder = new ConfigurationBuilder().SetBasePath(Environment.GetEnvironmentVariable("ASPNETCORE_CONTENTROOT"));
            //var configuration = builder.Build();

            //services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            //services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

            //services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            //.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            var provider = services.BuildServiceProvider();
            var secretService = provider.GetService<ISecretService>();

            services
                .AddChatbotNLog(secretService)
                .AddChatbotWebAuth(configService, secretService);

            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddTwitchServices(configService, secretService)
                .AddDbContextFactory()
                .AddLibraryServices()
                .AddSignalRServices()
                .AddApiClientServices()
                .AddChatbotPrintfulService();

            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddSignalR();

            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            using (var context = (ChatbotContext)serviceProvider.GetService<IChatbotContextFactory>().Create())
            {
                context.Database.Migrate();
            }

            if (env.IsDevelopment() || string.Equals(env.EnvironmentName, "Local", StringComparison.InvariantCultureIgnoreCase))
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            //app.UseIpRateLimiting();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
                endpoints.MapHub<SongList>("/SongList");
            });

            var heartbeatService = serviceProvider.GetService<ISignalRHeartbeatService>();
            heartbeatService.NotifyClients();
            var chatterService = serviceProvider.GetService<IChatterService>();
            chatterService.UpdateChatters();
        }
    }
}
