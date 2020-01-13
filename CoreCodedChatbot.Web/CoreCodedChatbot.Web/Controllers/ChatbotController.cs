using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.Enums.Playlist;
using CoreCodedChatbot.ApiContract.RequestModels.Playlist;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
using CoreCodedChatbot.ApiContract.ResponseModels.Playlist.ChildModels;
using CoreCodedChatbot.Library.Models.SongLibrary;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.ViewModels.AjaxRequestModels;
using CoreCodedChatbot.Web.ViewModels.Playlist;
using CoreCodedChatbot.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RequestSongViewModel = CoreCodedChatbot.Web.ViewModels.Playlist.RequestSongViewModel;

namespace CoreCodedChatbot.Web.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly IChatterService chatterService;
        private readonly IPlaylistApiClient _playlistApiClient;
        private readonly IVipApiClient _vipApiClient;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(
            IChatterService chatterService,
            IPlaylistApiClient playlistApiClient,
            IVipApiClient vipApiClient,
                ILogger<ChatbotController> logger)
        {
            this.chatterService = chatterService;
            _playlistApiClient = playlistApiClient;
            _vipApiClient = vipApiClient;
            _logger = logger;
        }

        public ActionResult Index()
        {
            return View();
        }

        public IActionResult List()
        {
            var chattersModel = chatterService.GetCurrentChatters();
            LoggedInTwitchUser twitchUser = null;

            ViewBag.UserIsMod = User.Identity.IsAuthenticated;

            if (User.Identity.IsAuthenticated)
            {
                var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                    ?.Value;
                twitchUser = new LoggedInTwitchUser
                {
                    Username = username,
                    IsMod = chattersModel?.IsUserMod(username) ?? false
                };

                ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                ViewBag.Username = twitchUser?.Username ?? string.Empty;
            }

            var allCurrentSongRequests = _playlistApiClient.GetAllCurrentSongRequests()?.Result;

            var playlistModel = new PlaylistViewModel();

            if (allCurrentSongRequests != null)
                playlistModel = new PlaylistViewModel
                {
                    CurrentSong = allCurrentSongRequests.CurrentSong,
                    RegularList = allCurrentSongRequests.RegularList
                        .Where(r => r.songRequestId != allCurrentSongRequests.CurrentSong.songRequestId).ToArray(),
                    VipList = allCurrentSongRequests.VipList
                        .Where(r => r.songRequestId != allCurrentSongRequests.CurrentSong.songRequestId).ToArray(),
                    TwitchUser = twitchUser
                };

            ViewBag.UserHasVip = User.Identity.IsAuthenticated && _vipApiClient.DoesUserHaveVip(
                                     new DoesUserHaveVipRequestModel
                                     {
                                         Username = User.Identity.Name.ToLower()
                                     }).Result.HasVip;

            ViewBag.IsPlaylistOpen = _playlistApiClient.IsPlaylistOpen().Result == PlaylistState.Open;

            return View(playlistModel);
        }

        [HttpGet]
        public IActionResult Library()
        {
            using (var sr = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "SongsMasterGrid.json")))
            {
                return View(JsonConvert.DeserializeObject<SongLibraryRecords>(sr.ReadToEnd()));
            }
        }

        [HttpPost]
        public IActionResult RenderRegularList([FromBody] PlaylistItem[] data)
        {
            try
            {
                var chattersModel = chatterService.GetCurrentChatters();
                ViewBag.UserIsMod = User.Identity.IsAuthenticated;

                if (User.Identity.IsAuthenticated)
                {
                    var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value;
                    var twitchUser = new LoggedInTwitchUser
                    {
                        Username = username,
                        IsMod = chattersModel?.IsUserMod(username) ?? false
                    };

                    ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                    ViewBag.Username = twitchUser?.Username ?? string.Empty;
                }

                ViewBag.UserHasVip = User.Identity.IsAuthenticated && _vipApiClient.DoesUserHaveVip(
                                         new DoesUserHaveVipRequestModel
                                         {
                                             Username = User.Identity.Name.ToLower()
                                         }).Result.HasVip;

                ViewBag.IsPlaylistOpen = _playlistApiClient.IsPlaylistOpen().Result == PlaylistState.Open;

                return PartialView("Partials/List/RegularList", data);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderVipList([FromBody] PlaylistItem[] data)
        {
            try
            {
                var chattersModel = chatterService.GetCurrentChatters();
                ViewBag.UserIsMod = User.Identity.IsAuthenticated;

                if (User.Identity.IsAuthenticated)
                {
                    var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value;
                    var twitchUser = new LoggedInTwitchUser
                        {
                            Username = username,
                            IsMod = chattersModel?.IsUserMod(username) ?? false
                        };

                    ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                    ViewBag.Username = twitchUser?.Username ?? string.Empty;
                }

                ViewBag.UserHasVip = User.Identity.IsAuthenticated && _vipApiClient.DoesUserHaveVip(
                                         new DoesUserHaveVipRequestModel
                                         {
                                             Username = User.Identity.Name.ToLower()
                                         }).Result.HasVip;

                ViewBag.IsPlaylistOpen = _playlistApiClient.IsPlaylistOpen().Result == PlaylistState.Open;
                return PartialView("Partials/List/VipList", data);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderRemoveSongModal([FromBody] string songId)
        {
            CheckAndSetUserModStatus();
            if (!User.Identity.IsAuthenticated) return BadRequest();

            var requestToDelete = _playlistApiClient.GetRequestById(int.Parse(songId)).Result.Request;
            
            try
            {
                return PartialView("Partials/List/DeleteModal", requestToDelete);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderRemoveCurrentSongModal([FromBody] string songId)
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            var requestToDelete = _playlistApiClient.GetRequestById(int.Parse(songId)).Result.Request;

            try
            {
                return PartialView("Partials/List/RemoveCurrentModal", requestToDelete);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public async Task<IActionResult> RenderCurrentSong([FromBody] PlaylistItem data)
        {
            try
            {
                var chattersModel = chatterService.GetCurrentChatters();
                ViewBag.UserIsMod = User.Identity.IsAuthenticated;

                if (User.Identity.IsAuthenticated)
                {
                    var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value;
                    var twitchUser = new LoggedInTwitchUser
                        {
                            Username = username,
                            IsMod = chattersModel?.IsUserMod(username) ?? false
                        };
                    ViewBag.UserIsMod = twitchUser?.IsMod ?? false;

                    var apiResult = await _vipApiClient.DoesUserHaveVip(
                        new DoesUserHaveVipRequestModel
                        {
                            Username = User.Identity.Name.ToLower()
                        });

                    ViewBag.UserHasVip = apiResult.HasVip;
                }

                var playlistCheck = await _playlistApiClient.IsPlaylistOpen();

                ViewBag.IsPlaylistOpen = playlistCheck == PlaylistState.Open;
                return PartialView("Partials/List/CurrentSong", data);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public async Task<IActionResult> RenderRequestModal()
        {
            try
            {
                var hasVipRequest = await _vipApiClient.DoesUserHaveVip(
                    new DoesUserHaveVipRequestModel
                    {
                        Username = User.Identity.Name.ToLower()
                    });

                var hasSuperVipRequest = await _vipApiClient.DoesUserHaveSuperVip(new DoesUserHaveSuperVipRequestModel
                {
                    Username = User.Identity.Name.ToLower()
                });

                var shouldShowVip = hasVipRequest.HasVip;

                var shouldShowSuperVip = false;

                if (hasSuperVipRequest.HasSuperVip)
                {
                    var shouldShowSuperVipRequest = await _vipApiClient.IsSuperVipInQueue();
                        
                    shouldShowSuperVip = !shouldShowSuperVipRequest.IsInQueue;
                }

                var requestViewModel = RequestSongViewModel.GetNewRequestViewModel(shouldShowVip, shouldShowSuperVip);

                return PartialView("Partials/List/RequestModal", requestViewModel);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderEditRequestModal([FromBody] string songId)
        {
            CheckAndSetUserModStatus();
            if (!User.Identity.IsAuthenticated) return BadRequest();

            try
            {
                var requestToEdit = _playlistApiClient.GetRequestById(int.Parse(songId)).Result.Request;

                var requestViewModel =
                    RequestSongViewModel.GetEditRequestSongViewModel(requestToEdit);

                return PartialView("Partials/List/RequestModal", requestViewModel);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderPromoteSongModal([FromBody] string songId)
        {
            CheckAndSetUserModStatus();
            if (!User.Identity.IsAuthenticated) return BadRequest();

            try
            {
                var requestToPromote = _playlistApiClient.GetRequestById(int.Parse(songId)).Result.Request;

                return PartialView("Partials/List/PromoteSongModal", requestToPromote);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RemoveSong([FromBody] string songId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var chattersModel = chatterService.GetCurrentChatters();
                var request = _playlistApiClient.GetRequestById(int.Parse(songId)).Result.Request;

                if ((chattersModel?.IsUserMod(User.Identity.Name) ?? false) ||
                    string.Equals(User.Identity.Name, request.songRequester))
                {
                    if (_playlistApiClient.ArchiveRequestById(int.Parse(songId)).Result)
                        return Ok();
                }
            }

            return Json(new {Success = false, Message = "Encountered an error, are you certain you're logged in?"});
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCurrentSong([FromBody] string songId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var chattersModel = chatterService.GetCurrentChatters();

                if (chattersModel?.IsUserMod(User.Identity.Name) ?? false)
                {
                    try
                    {
                        await _playlistApiClient.ArchiveCurrentRequest(int.Parse(songId));
                        return Ok();
                    }
                    catch (Exception)
                    {
                        return Json(new
                            {Success = false, Message = "Encountered an error, or you are not a moderator"});
                    }
                }
            }

            return Json(new {Success = false, Message = "Encountered an error, or you are not a moderator"});
        }

        [HttpPost]
        public IActionResult RequestSong([FromBody] RequestSongModel requestData)
        {
            if (User.Identity.IsAuthenticated)
            {
                var requestModel = new AddWebSongRequest
                {
                    SongRequestId = int.Parse(requestData.SongRequestId),
                    Title = requestData.Title,
                    Artist = requestData.Artist,
                    SelectedInstrument = requestData.SelectedInstrument,
                    IsVip = requestData.IsVip,
                    IsSuperVip = requestData.IsSuperVip,
                    Username = User.Identity.Name
                };

                var requestResult = _playlistApiClient.AddWebRequest(requestModel).Result.Result;

                var maxRegularRequests = _playlistApiClient.GetMaxUserRequests().Result.MaxRequests;

                switch (requestResult)
                {
                    case AddRequestResult.Success:
                        return Ok();
                    case AddRequestResult.NoMultipleRequests:
                        return BadRequest(new
                        {
                            Message =
                                $"You cannot have more than {maxRegularRequests} regular request{(maxRegularRequests > 1 ? "s" : "")}"
                        });
                    case AddRequestResult.PlaylistClosed:
                        return BadRequest(new
                        {
                            Message =
                                "The playlist is currently closed, you can still use a VIP token to request though!"
                        });
                    case AddRequestResult.PlaylistVeryClosed:
                        return BadRequest(new
                        {
                            Message =
                                "The playlist is completely closed, please wait until the playlist opens to request a song"
                        });
                    case AddRequestResult.UnSuccessful:
                        return BadRequest(new
                        {
                            Message = "An error occurred, please wait until the issue is resolved"
                        });
                    case AddRequestResult.NoRequestEntered:
                        return BadRequest(new
                        {
                            Message = "You haven't entered a request. Please enter a Song Name and/or Artist"
                        });
                }
            }

            return BadRequest(new {Message = "It looks like you're not logged in, log in and try again"});
        }

        [HttpPost]
        public IActionResult EditSong([FromBody] RequestSongModel requestData)
        {
            if (User.Identity.IsAuthenticated)
            {
                var chatters = chatterService.GetCurrentChatters();

                var requestModel = new EditWebRequestRequestModel
                {
                    SongRequestId = int.Parse(requestData.SongRequestId),
                    Title = requestData.Title,
                    Artist = requestData.Artist,
                    SelectedInstrument = requestData.SelectedInstrument,
                    IsVip = requestData.IsVip,
                    IsSuperVip = requestData.IsSuperVip,
                    Username = User.Identity.Name.ToLower(),
                    IsMod = chatters?.IsUserMod(User.Identity.Name.ToLower()) ?? false
                };

                var editRequestResult =
                    _playlistApiClient.EditWebRequest(requestModel).Result.EditResult;

                switch (editRequestResult)
                {
                    case EditRequestResult.Success:
                        return Ok();
                    case EditRequestResult.NoRequestEntered:
                        return BadRequest(new
                        {
                            Message = "You haven't entered a request. Please enter a Song name and/or Artist"
                        });
                    case EditRequestResult.NotYourRequest:
                        return BadRequest(new
                        {
                            Message = "This doesn't seem to be your request. Please try again"
                        });
                    case EditRequestResult.RequestAlreadyRemoved:
                        return BadRequest(new
                        {
                            Message = "It seems like this song has been played or removed from the list"
                        });
                    default:
                        return BadRequest(new
                        {
                            Message = "An error occurred, please wait until the issue is resolved"
                        });
                }
            }

            return BadRequest(new {Message = "It looks like you're not logged in, log in and try again"});
        }

        [HttpPost]
        public IActionResult PromoteRequest([FromBody] string songId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var promoteRequestResult = _playlistApiClient.PromoteWebRequest(
                    new PromoteWebRequestRequestModel
                    {
                        SongRequestId = int.Parse(songId),
                        Username = User.Identity.Name.ToLower()
                    }).Result.Result;

                switch (promoteRequestResult)
                {
                    case PromoteRequestResult.NotYourRequest:
                        return BadRequest(new
                        {
                            Message = "This is not your request. Please try again"
                        });
                    case PromoteRequestResult.AlreadyVip:
                        return BadRequest(new
                        {
                            Message = "This request has already been promoted! Congratulations"
                        });
                    case PromoteRequestResult.NoVipAvailable:
                        return BadRequest(new
                        {
                            Message = "Sorry but you don't seem to have a VIP token"
                        });
                    case PromoteRequestResult.Successful:
                        return Ok();
                    default:
                        return BadRequest(new
                        {
                            Message = "An error occurred, please wait until the issue is resolved"
                        });
                }
            }

            return BadRequest(new {Message = "It looks like you're not logged in, log in and try again"});
        }

        public IActionResult RenderAddToDriveModal([FromBody] string songId)
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            try
            {
                var requestToAddToDrive = _playlistApiClient.GetRequestById(int.Parse(songId)).Result.Request;

                return PartialView("Partials/List/AddToDriveModal", requestToAddToDrive);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        public IActionResult AddSongToDrive([FromBody] string songId)
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            try
            {
                _playlistApiClient.AddRequestToDrive(new AddSongToDriveRequest
                {
                    SongRequestId = int.Parse(songId)
                });
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in {HttpContext.Request.RouteValues["action"]}");
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        public IActionResult RenderEmptyPlaylistModal()
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            try
            {
                return PartialView("Partials/List/EmptyPlaylistModal");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in {HttpContext.Request.RouteValues["action"]}");
                return BadRequest();
            }
        }

        public IActionResult EmptyPlaylist()
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            try
            {
                if (_playlistApiClient.ClearRequests().Result)
                    return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in {HttpContext.Request.RouteValues["action"]}");
            }

            return BadRequest();
        }

        public IActionResult RenderOpenPlaylistModal()
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            try
            {
                return PartialView("Partials/List/OpenPlaylistModal");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in {HttpContext.Request.RouteValues["action"]}");
                return BadRequest();
            }
        }

        public IActionResult RenderVeryClosePlaylistModal()
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            try
            {
                return PartialView("Partials/List/VeryClosePlaylistModal");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in {HttpContext.Request.RouteValues["action"]}");
                return BadRequest();
            }
        }

        public IActionResult OpenPlaylist()
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            try
            {
                if (_playlistApiClient.OpenPlaylist().Result)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in {HttpContext.Request.RouteValues["action"]}");
                return BadRequest();
            }
        }

        public IActionResult VeryClosePlaylist()
        {
            if (!CheckAndSetUserModStatus()) return BadRequest();

            try
            {
                if (_playlistApiClient.VeryClosePlaylist().Result)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in {HttpContext.Request.RouteValues["action"]}");
                return BadRequest();
            }
        }

        private bool CheckAndSetUserModStatus()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return false;
            }

            var chattersModel = chatterService.GetCurrentChatters();

            if (User.Identity.IsAuthenticated)
            {
                var username = User?.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                                   ?.Value ?? string.Empty;
                var twitchUser = new LoggedInTwitchUser
                {
                    Username = username,
                    IsMod = chattersModel.IsUserMod(username)
                };

                ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
            }

            return ViewBag.UserIsMod;
        }
    }
}
