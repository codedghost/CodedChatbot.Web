using CoreCodedChatbot.Web.ViewModels.Playlist;

namespace CoreCodedChatbot.Web.Interfaces
{
    public interface IModService
    {
        void UpdateModList();
        bool IsUserModerator(string username);
    }
}
