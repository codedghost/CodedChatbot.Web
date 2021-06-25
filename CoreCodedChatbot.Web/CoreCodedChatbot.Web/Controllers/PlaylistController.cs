using System;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
using CoreCodedChatbot.ApiContract.ResponseModels.Playlist.ChildModels;
using CoreCodedChatbot.Web.Extensions;
using CoreCodedChatbot.Web.Interfaces.Services;
using CoreCodedChatbot.Web.Models;
using CoreCodedChatbot.Web.Services;
using CoreCodedChatbot.Web.ViewModels.Playlist;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly IPlaylistApiClient _playlistApiClient;
        private readonly IVipApiClient _vipApiClient;
        private readonly IReactUiService _reactUiService;

        public PlaylistController(
            IPlaylistApiClient playlistApiClient,
            IVipApiClient vipApiClient,
            IReactUiService reactUiService
            )
        {
            _playlistApiClient = playlistApiClient;
            _vipApiClient = vipApiClient;
            _reactUiService = reactUiService;
        }

        // GET
        public async Task<IActionResult> GetPlaylist()
        {
            var allCurrentSongRequests = await _playlistApiClient.GetAllCurrentSongRequests().ConfigureAwait(false);

            var currentSong = _reactUiService.FormatUiModel(allCurrentSongRequests.CurrentSong, true,
                false);

            var regularQueue =
                allCurrentSongRequests.RegularList.Where(r => r.songRequestId != (currentSong?.SongId ?? 0)).Select(r =>
                    _reactUiService.FormatUiModel(r, false, true));

            var vipQueue =
                allCurrentSongRequests.VipList.Where(r => r.songRequestId != (currentSong?.SongId ?? 0)).Select(r => _reactUiService.FormatUiModel(r, false, false));

            return Json(new UiPlaylistStateModel
            {
                CurrentSong = currentSong,
                RegularQueue = regularQueue.ToList(),
                VipQueue = vipQueue.ToList()
            });
        }
    }
}