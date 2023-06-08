using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Twitch;
using CodedChatbot.TwitchFactories;
using CodedChatbot.TwitchFactories.Interfaces;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Interfaces.Services;
using CoreCodedChatbot.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CoreCodedChatbot.Web
{
    public static class Package
    {
        public static IServiceCollection AddChatbotWebAuth(
            this IServiceCollection services
            )
        {
            var serviceProvider = services.BuildServiceProvider();
            services.AddAuthentication(op =>
                {
                    op.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultChallengeScheme = TwitchAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddTwitch();

            services.AddOptions<TwitchAuthenticationOptions>(TwitchAuthenticationDefaults.AuthenticationScheme)
                .Configure<ISecretService, IConfigService, ITwitchApiFactory>((options, secretService, configService, twitchApiFactory) =>
                {
                    options.ClientId = secretService.GetSecret<string>("TwitchWebAppClientId");
                    options.ClientSecret = secretService.GetSecret<string>("TwitchWebAppClientSecret");
                    options.Scope.Add(configService.Get<string>("TwitchWebAppScopes"));
                    options.CallbackPath =
                        PathString.FromUriComponent(configService.Get<string>("TwitchWebAppCallbackPath"));

                    options.Events = new OAuthEvents
                    {
                        OnTicketReceived = async ctx =>
                        {
                            var twitchApi = twitchApiFactory.Get();

                            var moderatorResponse =
                                await twitchApi.Helix.Moderation.GetModeratorsAsync(configService.Get<string>("ChannelId"),
                                    accessToken: secretService.GetSecret<string>("ChatbotAccessToken")).ConfigureAwait(false);

                            var modClaim = new Claim("IsModerator",
                                (moderatorResponse.Data.Any(mod => string.Equals(mod.UserName,
                                     ctx.Principal.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) ||
                                 string.Equals(ctx.Principal.Identity.Name,
                                     configService.Get<string>("StreamerChannel"),
                                     StringComparison.CurrentCultureIgnoreCase))
                                .ToString());

                            var identity = new ClaimsIdentity(new[] { modClaim });

                            ctx.Principal.AddIdentity(identity);
                            await Task.CompletedTask.ConfigureAwait(false);
                        }
                    };
                });

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromMinutes(30);
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
            services.AddTwitchFactories();

            return services;
        }

        public static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            services.AddSingleton<ISignalRHeartbeatService, SignalRHeartbeatService>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IReactUiService, ReactUiService>();

            return services;
        }
    }
}