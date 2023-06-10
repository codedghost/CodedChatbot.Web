using System;
using CodedGhost.Config;
using CoreCodedChatbot.ApiClient;
using CoreCodedChatbot.Config;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using CoreCodedChatbot.Logging;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Interfaces.Services;
using CoreCodedChatbot.Web.SignalRHubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
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
                    configService.Get<string>("KeyVaultBaseUrl"),
                    configService.Get<string>("ActiveDirectoryTenantId")
                );

            //var builder = new ConfigurationBuilder().SetBasePath(Environment.GetEnvironmentVariable("ASPNETCORE_CONTENTROOT"));
            //var configuration = builder.Build();

            //services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            //services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

            //services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            //.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            services
                .AddChatbotNLog()
                .AddChatbotWebAuth();

            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddTwitchServices()
                .AddSignalRServices()
                .AddApiClientServices()
                .AddServices();
            //.AddChatbotPrintfulService();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .WithOrigins("https://codedghost.com", "https://www.codedghost.com",
                            "https://api.codedghost.com", "http://localhost:3000")
                        .WithMethods("GET", "POST", "PUT", "DELETE")
                        .AllowCredentials();
                });
            });
            services.AddControllersWithViews();
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddSignalR();

            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment() || string.Equals(env.EnvironmentName, "Local", StringComparison.InvariantCultureIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseCookiePolicy(new CookiePolicyOptions {MinimumSameSitePolicy = SameSiteMode.Lax});

            //app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            //app.UseIpRateLimiting();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
                endpoints.MapHub<SongList>("/SongList");
            });

            var heartbeatService = serviceProvider.GetService<ISignalRHeartbeatService>();
            heartbeatService.NotifyClients();
        }
    }
}
