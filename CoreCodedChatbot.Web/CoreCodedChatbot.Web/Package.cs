using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Web
{
    public static class Package
    {
        public static IServiceCollection AddChatbotWebAuth(
            this IServiceCollection services, 
            IConfigService configService, 
            ISecretService secretService
            )
        {
            services.AddAuthentication(op =>
                {
                    op.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultChallengeScheme = TwitchAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddTwitch(options =>
                {
                    options.ClientId = secretService.GetSecret<string>("TwitchWebAppClientId");
                    options.ClientSecret = secretService.GetSecret<string>("TwitchWebAppClientSecret");
                    options.Scope.Add(configService.Get<string>("TwitchWebAppScopes"));
                    options.CallbackPath =
                        PathString.FromUriComponent(configService.Get<string>("TwitchWebAppCallbackPath"));
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(TwitchAuthenticationDefaults.AuthenticationScheme,
                    builder => { builder.RequireAuthenticatedUser().Build(); });
            });

            return services;
        }

        public static IServiceCollection AddTwitchServices(
            this IServiceCollection services,
            IConfigService configService,
            ISecretService secretService
        )
        {
            var creds = new ConnectionCredentials(configService.Get<string>("ChatbotNick"), secretService.GetSecret<string>("ChatbotPass"));
            var client = new TwitchClient();
            client.Initialize(creds, configService.Get<string>("StreamerChannel"));
            client.Connect();

            var api = new TwitchAPI();
            api.Settings.AccessToken = secretService.GetSecret<string>("ChatbotAccessToken");

            services.AddSingleton(client);
            services.AddSingleton(api);

            services.AddSingleton<IModService, ModService>();

            return services;
        }

        public static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            services.AddSingleton<ISignalRHeartbeatService, SignalRHeartbeatService>();

            return services;
        }
    }
}