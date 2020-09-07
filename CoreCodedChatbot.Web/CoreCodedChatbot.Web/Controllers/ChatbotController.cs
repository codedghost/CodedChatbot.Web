using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.ApiClient.ApiClients;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.Enums.Playlist;
using CoreCodedChatbot.ApiContract.RequestModels.Playlist;
using CoreCodedChatbot.ApiContract.RequestModels.Search;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
using CoreCodedChatbot.ApiContract.ResponseModels.Playlist.ChildModels;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.Interfaces.Services;
using CoreCodedChatbot.Web.ViewModels.AjaxRequestModels;
using CoreCodedChatbot.Web.ViewModels.Chatbot;
using CoreCodedChatbot.Web.ViewModels.Playlist;
using CoreCodedChatbot.Web.ViewModels.Shared;
using CoreCodedChatbot.Web.ViewModels.SongLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RequestSongViewModel = CoreCodedChatbot.Web.ViewModels.Playlist.RequestSongViewModel;

namespace CoreCodedChatbot.Web.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly IModService _modService;
        private readonly IPlaylistApiClient _playlistApiClient;
        private readonly IVipApiClient _vipApiClient;
        private readonly ISearchApiClient _searchApiClient;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(
            IModService modService,
            IPlaylistApiClient playlistApiClient,
            IVipApiClient vipApiClient,
            ISearchApiClient searchApiClient,
            ILogger<ChatbotController> logger)
        {
            this._modService = modService;
            _playlistApiClient = playlistApiClient;
            _vipApiClient = vipApiClient;
            _searchApiClient = searchApiClient;
            _logger = logger;
        }

        public IActionResult Synonym()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public async Task<IActionResult> SubmitSynonym(RequestSearchSynonymViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                    ?.Value;
                var result = await _searchApiClient.SaveSearchSynonymRequest(new SaveSearchSynonymRequest
                {
                    SearchSynonymRequest = model.SynonymRequest,
                    Username = username
                });

                if (result)
                {
                    return RedirectToAction("Synonym", "Chatbot");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> List()
        {
            LoggedInTwitchUser twitchUser = null;

            ViewBag.UserIsMod = User.Identity.IsAuthenticated;

            if (User.Identity.IsAuthenticated)
            {
                var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                    ?.Value;

                var userVips = await _vipApiClient.GetUserVipCount(new GetUserVipCountRequest
                {
                    Username = username
                });

                twitchUser = new LoggedInTwitchUser
                {
                    Username = username,
                    IsMod = _modService.IsUserModerator(username),
                    Vips = userVips?.Vips ?? 0
                };

                ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
                ViewBag.Username = twitchUser?.Username ?? string.Empty;
            }

            var allCurrentSongRequests = await _playlistApiClient.GetAllCurrentSongRequests();

            var playlistModel = new PlaylistViewModel
            {
                TwitchUser = twitchUser
            };

            if (allCurrentSongRequests != null)
            {
                playlistModel.CurrentSong = allCurrentSongRequests.CurrentSong;
                playlistModel.RegularList = allCurrentSongRequests.RegularList
                    .Where(r => r.songRequestId != allCurrentSongRequests.CurrentSong.songRequestId).ToArray();
                playlistModel.VipList = allCurrentSongRequests.VipList
                    .Where(r => r.songRequestId != allCurrentSongRequests.CurrentSong.songRequestId).ToArray();
            }

            ViewBag.UserHasVip = User.Identity.IsAuthenticated && (_vipApiClient.DoesUserHaveVip(
                new DoesUserHaveVipRequestModel
                {
                    Username = User.Identity.Name.ToLower()
                })?.Result?.HasVip ?? false);

            var playlistTask = await _playlistApiClient.IsPlaylistOpen();

            ViewBag.IsPlaylistOpen = playlistTask == PlaylistState.Open;

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
                ViewBag.UserIsMod = User.Identity.IsAuthenticated;

                if (User.Identity.IsAuthenticated)
                {
                    var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value;
                    var twitchUser = new LoggedInTwitchUser
                    {
                        Username = username,
                        IsMod = _modService.IsUserModerator(username)
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
                ViewBag.UserIsMod = User.Identity.IsAuthenticated;

                if (User.Identity.IsAuthenticated)
                {
                    var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value;
                    var twitchUser = new LoggedInTwitchUser
                        {
                            Username = username,
                            IsMod = _modService.IsUserModerator(username)
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
                ViewBag.UserIsMod = User.Identity.IsAuthenticated;

                if (User.Identity.IsAuthenticated)
                {
                    var username = User.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                        ?.Value;
                    var twitchUser = new LoggedInTwitchUser
                        {
                            Username = username,
                            IsMod = _modService.IsUserModerator(username)
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
                var request = _playlistApiClient.GetRequestById(int.Parse(songId)).Result.Request;

                if (_modService.IsUserModerator(User.Identity.Name) ||
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
                if (_modService.IsUserModerator(User.Identity.Name))
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
                    case AddRequestResult.MaximumRegularRequests:
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
                var requestModel = new EditWebRequestRequestModel
                {
                    SongRequestId = int.Parse(requestData.SongRequestId),
                    Title = requestData.Title,
                    Artist = requestData.Artist,
                    SelectedInstrument = requestData.SelectedInstrument,
                    IsVip = requestData.IsVip,
                    IsSuperVip = requestData.IsSuperVip,
                    Username = User.Identity.Name.ToLower(),
                    IsMod = _modService.IsUserModerator(User.Identity.Name)
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
        public async Task<IActionResult> PromoteRequest([FromBody] string songId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var promoteRequestResult = await _playlistApiClient.PromoteSong(
                    new PromoteSongRequest
                    {
                        SongRequestId = int.Parse(songId),
                        Username = User.Identity.Name.ToLower()
                    });


                switch (promoteRequestResult.PromoteRequestResult)
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

            if (User.Identity.IsAuthenticated)
            {
                var username = User?.FindFirst(c => c.Type == TwitchAuthenticationConstants.Claims.DisplayName)
                                   ?.Value ?? string.Empty;
                var twitchUser = new LoggedInTwitchUser
                {
                    Username = username,
                    IsMod = _modService.IsUserModerator(username)
                };

                ViewBag.UserIsMod = twitchUser?.IsMod ?? false;
            }

            return ViewBag.UserIsMod;
        }
    }
}
