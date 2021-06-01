using CoreCodedChatbot.ApiContract.ResponseModels.Playlist.ChildModels;
using CoreCodedChatbot.Web.Models;

namespace CoreCodedChatbot.Web.Services
{
    public interface IReactUiService
    {
        SongRequest FormatUiModel(PlaylistItem item, bool isCurrent, bool isRegularQueue);
    }
}