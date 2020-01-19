using CoreCodedChatbot.Web.ViewModels.Playlist;

namespace CoreCodedChatbot.Web.Interfaces
{
    public interface IChatterService
    {
        void UpdateChatters();
        ChatViewersModel GetCurrentChatters();
    }
}
