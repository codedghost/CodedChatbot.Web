using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Factories;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.Interfaces.Factories;
using CoreCodedChatbot.Web.Interfaces.Services;
using CoreCodedChatbot.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
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
            this IServiceCollection services
            )
        {
            services.AddAuthentication(op =>
                {
                    op.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultChallengeScheme = TwitchAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddTwitch();

            services.AddOptions<TwitchAuthenticationOptions>(TwitchAuthenticationDefaults.AuthenticationScheme)
                .Configure<ISecretService, IConfigService>((options, secretService, configService) =>
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
            this IServiceCollection services
        )
        {
            services.AddSingleton<ITwitchApiFactory, TwitchApiFactory>();
            services.AddSingleton<ITwitchClientFactory, TwitchClientFactory>();
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