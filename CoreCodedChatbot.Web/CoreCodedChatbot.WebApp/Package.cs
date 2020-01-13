using Microsoft.Extensions.DependencyInjection;

namespace CoreCodedChatbot.WebApp
{
    public static class Package
    {
        public static IServiceCollection AddWebAppServices(this IServiceCollection services)
        {
            return services;
        }
    }
}