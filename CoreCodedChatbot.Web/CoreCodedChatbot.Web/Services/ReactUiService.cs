using System;
using CoreCodedChatbot.ApiContract.ResponseModels.Playlist.ChildModels;
using CoreCodedChatbot.Web.Models;

namespace CoreCodedChatbot.Web.Services
{
    public class ReactUiService : IReactUiService
    {
        public SongRequest FormatUiModel(PlaylistItem item, bool isCurrent, bool isRegularQueue)
        {
            if (item == null) return null;

            return new SongRequest
            {
                SongId = item.songRequestId,
                SongTitle = item.FormattedRequest.SongName,
                SongArtist = item.FormattedRequest.SongArtist,
                Instrument = item.FormattedRequest.InstrumentName,
                Requester = item.songRequester,
                IsInDrive = item.isInDrive,
                IsInChat = item.isInChat,
                IsVip = item.isVip,
                IsSuperVip = item.isSuperVip
            };
        }
    }
}