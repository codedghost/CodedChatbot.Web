using CoreCodedChatbot.ApiContract.ResponseModels.Playlist.ChildModels;
using CoreCodedChatbot.Library.Models.View;

namespace CoreCodedChatbot.Web.ViewModels.Playlist
{
    public class PlaylistViewModel
    {
        public PlaylistItem CurrentSong { get; set; }
        public PlaylistItem[] VipList { get; set; }
        public PlaylistItem[] RegularList { get; set; }
        public LoggedInTwitchUser TwitchUser { get; set; }
    }
}