using System;
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
using Microsoft.Extensions.Logging;

namespace CoreCodedChatbot.Web.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly IPlaylistApiClient _playlistApiClient;
        private readonly IVipApiClient _vipApiClient;
        private readonly IReactUiService _reactUiService;
        private readonly ILogger<PlaylistController> _logger;

        public PlaylistController(
            IPlaylistApiClient playlistApiClient,
            IVipApiClient vipApiClient,
            IReactUiService reactUiService,
            ILogger<PlaylistController> logger
        )
        {
            _playlistApiClient = playlistApiClient;
            _vipApiClient = vipApiClient;
            _reactUiService = reactUiService;
            _logger = logger;
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
        public async Task<IActionResult> RemoveRequest([FromBody] UiSongAtomicActionRequestModel atomicActionRequest)
        {
            if (User.Identity.IsAuthenticated)
            {
                var request = _playlistApiClient.GetRequestById(atomicActionRequest.songId).Result.Request;

                if (User.Identities.IsMod() ||
                    string.Equals(User.Identity.Name, request.songRequester))
                {
                    if (_playlistApiClient.ArchiveRequestById(atomicActionRequest.songId).Result)
                        return Ok("Success");
                }
            }

            return Ok("Could not remove this request, please try again in a moment");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCurrent([FromBody] UiSongAtomicActionRequestModel atomicActionCurrentRequest)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.Identities.IsMod())
                {
                    try
                    {
                        await _playlistApiClient.ArchiveCurrentRequest(atomicActionCurrentRequest.songId);
                        return Ok("Success");
                    }
                    catch (Exception)
                    {
                        return Ok("Encountered an error, or you are not a moderator");
                    }
                }
            }

            return Ok("Encountered an error, or you are not a moderator");
        }

        [HttpPost]
        public async Task<IActionResult> MarkInDrive([FromBody] UiSongAtomicActionRequestModel markInDriveRequest)
        {
            if (!User.Identities.IsMod())
            {
                _logger.LogError(
                    $"User {User.Identity.Name} attempted to mark a request as in the drive, this user is not a moderator");
                return Ok("You are not permitted to perform this action");
            }

            try
            {
                var success = await _playlistApiClient.AddRequestToDrive(new AddSongToDriveRequest
                {
                    SongRequestId = markInDriveRequest.songId
                }).ConfigureAwait(false);
                return Ok(success ? "Success" : "Attempt unsuccessful");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in {HttpContext.Request.RouteValues["action"]}");
                return Ok("Encountered an error, please try again in a moment");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PromoteRequest([FromBody] PromoteRequestModel promoteRequestModel)
        {
            if (!User.Identity.IsAuthenticated)
                return Ok("It looks like you're not logged in, log in and try again");

            var promoteRequestResult = await _playlistApiClient.PromoteSong(
                new PromoteSongRequest
                {
                    SongRequestId = promoteRequestModel.songId,
                    Username = User.Identity.Name.ToLower(),
                    UseSuperVip = promoteRequestModel.useSuperVip
                });

            var response = promoteRequestResult?.PromoteRequestResult switch
            {
                PromoteRequestResult.NotYourRequest => "This is not your request. Please try again",
                PromoteRequestResult.AlreadyVip => promoteRequestModel.useSuperVip ? "This request is already a SuperVIP! It'll be played next" : "This request is already a VIP! Congratulations",
                PromoteRequestResult.NoVipAvailable => promoteRequestModel.useSuperVip  ? "Sorry but you don't have enough tokens" : "Sorry but you don't seem to have a VIP token",
                PromoteRequestResult.Successful => "Success",
                _ => string.Empty
            };

            return Ok(response);
        }
    }
}