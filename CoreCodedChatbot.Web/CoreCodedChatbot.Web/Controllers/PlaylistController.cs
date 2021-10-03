﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.Enums.Playlist;
using CoreCodedChatbot.ApiContract.RequestModels.Playlist;
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

            if (allCurrentSongRequests == null)
            {
                return Json(new UiPlaylistStateModel
                {
                    CurrentSong = null,
                    RegularQueue = new List<SongRequest>(),
                    VipQueue = new List<SongRequest>()
                });
            }

            var currentSong = _reactUiService.FormatUiModel(allCurrentSongRequests.CurrentSong, true,
                false);

            var regularQueue =
                allCurrentSongRequests.RegularList.Where(r => r.songRequestId != (currentSong?.SongId ?? 0)).Select(r =>
                    _reactUiService.FormatUiModel(r, false, true));

            var vipQueue =
                allCurrentSongRequests.VipList.Where(r => r.songRequestId != (currentSong?.SongId ?? 0))
                    .Select(r => _reactUiService.FormatUiModel(r, false, false));

            return Json(new UiPlaylistStateModel
            {
                CurrentSong = currentSong,
                RegularQueue = regularQueue.ToList(),
                VipQueue = vipQueue.ToList()
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserInfo()
        {
            var playlistState = await _playlistApiClient.IsPlaylistOpen();

            var userPlaylistInfo = new UserPlaylistInfo
            {
                Vips = 0,
                PlaylistState = playlistState switch
                {
                    PlaylistState.Open => "Open",
                    PlaylistState.Closed => "VIP Only",
                    _ => "Closed"
                }
            };

            if (!User.Identity.IsAuthenticated) return Json(userPlaylistInfo);

            var vipCount = await _vipApiClient.GetUserVipCount(new GetUserVipCountRequest
            {
                Username = User.Identity.Name
            }).ConfigureAwait(false);

            userPlaylistInfo.Vips = vipCount?.Vips ?? 0;

            var byteCount = await _vipApiClient.GetUserByteCount(User.Identity.Name);

            userPlaylistInfo.Bytes = byteCount.Bytes;

            return Json(userPlaylistInfo);
        }

        [HttpPost]
        public async Task<IActionResult> AddRequest([FromBody] UiSongRequestModel request)
        {
            if (!User.Identities.Any(id => id.IsAuthenticated))
                return BadRequest();

            var response = await _playlistApiClient.AddWebRequest(new AddWebSongRequest
            {
                Title = request.songName,
                Artist = request.artistName,
                IsVip = request.useVipToken,
                IsSuperVip = request.useSuperVipToken,
                SelectedInstrument = request.instrument,
                Username = User.Identity.Name
            }).ConfigureAwait(false);

            var errorMessage = response.Result switch
            {
                AddRequestResult.MaximumRegularRequests =>
                "You already have the maximum number of regular requests in the queue! Please wait until your song has been played, edit your song, or use a VIP token",
                AddRequestResult.NoRequestEntered => "Please enter more information",
                AddRequestResult.NotEnoughVips => "You do not have enough VIPs to request this song at the moment",
                AddRequestResult.OnlyOneSuper =>
                "There can only be 1 Super VIP request in the queue at one time. Please wait a moment for this song to be played",
                AddRequestResult.PlaylistClosed => "The playlist is currently only accepting VIP requests",
                AddRequestResult.PlaylistVeryClosed => "The playlist is currently completely closed",
                AddRequestResult.UnSuccessful => "An error has occurred, please wait a moment and try again",
                AddRequestResult.Success => "Success",
                _ => string.Empty
            };
            return Ok(errorMessage);
        }

        [HttpPost]
        public async Task<IActionResult> EditRequest([FromBody] UiSongRequestModel request)
        {
            if (!User.Identities.Any(id => id.IsAuthenticated))
                return BadRequest();

            var response = await _playlistApiClient.EditWebRequest(new EditWebRequestRequestModel
            {
                SongRequestId = request.songRequestId,
                Title = request.songName,
                Artist = request.artistName,
                SelectedInstrument = request.instrument,
                IsVip = request.useVipToken,
                IsSuperVip = request.useSuperVipToken,
                IsMod = User.Identities.IsMod(),
                Username = User.Identity.Name
            }).ConfigureAwait(false);

            var errorMessage = response.EditResult switch
            {
                EditRequestResult.NoRequestEntered => "Please enter more information",
                EditRequestResult.NotYourRequest => "This doesn't appear to be your song, please refresh and try again",
                EditRequestResult.RequestAlreadyRemoved => "Sorry, this song has been removed from the playlist",
                EditRequestResult.UnSuccessful => "An error has occurred, please wait a moment and try again",
                EditRequestResult.Success => "Success",
                _ => string.Empty
            };

            return Ok(errorMessage);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRequest([FromBody] UiSongRemoveRequestModel removeRequest)
        {
            if (User.Identity.IsAuthenticated)
            {
                var request = _playlistApiClient.GetRequestById(removeRequest.songId).Result.Request;

                if (User.Identities.IsMod() ||
                    string.Equals(User.Identity.Name, request.songRequester))
                {
                    if (_playlistApiClient.ArchiveRequestById(removeRequest.songId).Result)
                        return Ok("Success");
                }
            }

            return Ok("Could not remove this request, please try again in a moment");
        }
    }
}