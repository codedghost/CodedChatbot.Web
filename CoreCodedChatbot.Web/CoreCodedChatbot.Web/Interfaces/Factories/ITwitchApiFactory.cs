using TwitchLib.Api;

namespace CoreCodedChatbot.Web.Interfaces.Factories
{
    public interface ITwitchApiFactory
    {
        TwitchAPI Get();
    }
}