using TwitchLib.Client;

namespace CoreCodedChatbot.Web.Interfaces.Factories
{
    public interface ITwitchClientFactory
    {
        TwitchClient GetClient();
    }
}