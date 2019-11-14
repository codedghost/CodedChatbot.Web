using System;
using System.IO;
using System.Linq;
using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.Enums.Playlist;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.Enums;
using CoreCodedChatbot.Library.Models.SongLibrary;
using CoreCodedChatbot.Library.Models.View;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.ViewModels.AjaxRequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CoreCodedChatbot.Web.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly IPlaylistService playlistService;
        private readonly IVipService vipService;

        private readonly IChatterService chatterService;
        private readonly IPlaylistApiClient _playlistApiClient;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(
            IPlaylistService playlistService, 
            IVipService vipService,
            IChatterService chatterService,
            IPlaylistApiClient playlistApiClient,
            ILogger<ChatbotController> logger)
        {
            this.playlistService = playlistService;
            this.vipService = vipService;

            this.chatterService = chatterService;
            _playlistApiClient = playlistApiClient;
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

            var playlistModel = playlistService.GetAllSongs(twitchUser);
            playlistModel.RegularList =
                playlistModel.RegularList.Where(r => r.songRequestId != playlistModel.CurrentSong.songRequestId)
                    .ToArray();
            playlistModel.VipList =
                playlistModel.VipList.Where(r => r.songRequestId != playlistModel.CurrentSong.songRequestId)
                    .ToArray();

            ViewBag.UserHasVip = User.Identity.IsAuthenticated && vipService.HasVip(User.Identity.Name.ToLower());

            ViewBag.IsPlaylistOpen = playlistService.GetPlaylistState() == PlaylistState.Open;

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

                ViewBag.UserHasVip = User.Identity.IsAuthenticated && vipService.HasVip(User.Identity.Name.ToLower());

                ViewBag.IsPlaylistOpen = playlistService.GetPlaylistState() == PlaylistState.Open;

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

                ViewBag.UserHasVip = User.Identity.IsAuthenticated && vipService.HasVip(User.Identity.Name.ToLower());

                ViewBag.IsPlaylistOpen = playlistService.GetPlaylistState() == PlaylistState.Open;
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

            var requestToDelete = playlistService.GetRequestById(int.Parse(songId));
            
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

            var requestToDelete = playlistService.GetRequestById(int.Parse(songId));

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
        public IActionResult RenderCurrentSong([FromBody] PlaylistItem data)
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
                }

                ViewBag.UserHasVip = User.Identity.IsAuthenticated && vipService.HasVip(User.Identity.Name.ToLower());

                ViewBag.IsPlaylistOpen = playlistService.GetPlaylistState() == PlaylistState.Open;
                return PartialView("Partials/List/CurrentSong", data);
            }
            catch (Exception)
            {
                return Json(new {Success = false, Message = "Encountered an error"});
            }
        }

        [HttpPost]
        public IActionResult RenderRequestModal()
        {
            try
            {
                var requestViewModel = playlistService.GetNewRequestSongViewModel(User.Identity.Name.ToLower());

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
                var requestViewModel =
                    playlistService.GetEditRequestSongViewModel(User.Identity.Name.ToLower(), int.Parse(songId),
                        ViewBag.UserIsMod ?? false);

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
                var requestToPromote = playlistService.GetRequestById(int.Parse(songId));

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
                var request = playlistService.GetRequestById(int.Parse(songId));

                if ((chattersModel?.IsUserMod(User.Identity.Name) ?? false) ||
                    string.Equals(User.Identity.Name, request.songRequester))
                {
                    if (playlistService.ArchiveRequestById(int.Parse(songId)))
                        return Ok();
                }
            }

            return Json(new {Success = false, Message = "Encountered an error, are you certain you're logged in?"});
        }

        [HttpPost]
        public IActionResult RemoveCurrentSong([FromBody] string songId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var chattersModel = chatterService.GetCurrentChatters();

                if (chattersModel?.IsUserMod(User.Identity.Name) ?? false)
                {
                    try
                    {
                        playlistService.ArchiveCurrentRequest(int.Parse(songId));
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
                var requestModel = new RequestSongViewModel
                {
                    SongRequestId = int.Parse(requestData.SongRequestId),
                    Title = requestData.Title,
                    Artist = requestData.Artist,
                    SelectedInstrument = requestData.SelectedInstrument,
                    IsVip = requestData.IsVip,
                    IsSuperVip = requestData.IsSuperVip
                };

                var requestResult = playlistService.AddWebRequest(requestModel, User.Identity.Name);

                switch (requestResult)
                {
                    case AddRequestResult.Success:
                        return Ok();
                    case AddRequestResult.NoMultipleRequests:
                        return BadRequest(new
                        {
                            Message =
                                $"You cannot have more than {playlistService.GetMaxUserRequests()} regular request{(playlistService.GetMaxUserRequests() > 1 ? "s" : "")}"
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

                var requestModel = new RequestSongViewModel
                {
                    SongRequestId = int.Parse(requestData.SongRequestId),
                    Title = requestData.Title,
                    Artist = requestData.Artist,
                    SelectedInstrument = requestData.SelectedInstrument,
                    IsVip = requestData.IsVip,
                    IsSuperVip = requestData.IsSuperVip
                };

                var editRequestResult =
                    playlistService.EditWebRequest(requestModel, User.Identity.Name.ToLower(),
                        chatters?.IsUserMod(User.Identity.Name.ToLower()) ?? false);

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
                var promoteRequestResult = playlistService.PromoteWebRequest(int.Parse(songId), User.Identity.Name.ToLower());

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
                var requestToAddToDrive = playlistService.GetRequestById(int.Parse(songId));

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
                playlistService.AddSongToDrive(int.Parse(songId));
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
                playlistService.ClearRockRequests();
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in {HttpContext.Request.RouteValues["action"]}");
                return BadRequest();
            }
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
                if (playlistService.OpenPlaylistWeb())
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
                if (playlistService.VeryClosePlaylistWeb())
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
