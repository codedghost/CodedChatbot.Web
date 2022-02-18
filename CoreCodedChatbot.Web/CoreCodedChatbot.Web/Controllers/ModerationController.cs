using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.ApiClients;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Moderation;
using CoreCodedChatbot.ApiContract.RequestModels.Search;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
using CoreCodedChatbot.Web.Extensions;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.Interfaces.Services;
using CoreCodedChatbot.Web.Models;
using CoreCodedChatbot.Web.ViewModels.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoreCodedChatbot.Web.Controllers
{
    public class ModerationController : Controller
    {
        private readonly IModerationApiClient _moderationApiClient;
        private readonly ISearchApiClient _searchApiClient;
        private readonly IVipApiClient _vipApiClient;
        private readonly ILogger<ModerationController> _logger;

        public ModerationController(
            IModerationApiClient moderationApiClient,
            ISearchApiClient searchApiClient,
            IVipApiClient vipApiClient,
            ILogger<ModerationController> logger)
        {
            _moderationApiClient = moderationApiClient;
            _searchApiClient = searchApiClient;
            _vipApiClient = vipApiClient;
            _logger = logger;
        }

        [Authorize]
        public IActionResult TransferUser()
        {
            if (!User.Identities.IsMod())
                RedirectToAction("Index", "Home");

            return View(new TransferUserViewModel());
        }

        [Authorize]
        public async Task<IActionResult> ProcessTransferUser(TransferUserViewModel request)
        {
            if (!User.Identities.IsMod())
            {
                ControllerContext.ModelState.AddModelError("TransferStatus", "You're not a moderator! How dare you!");
                return View("TransferUser", request);
            }

            var success = await _moderationApiClient.TransferUserAccount(new TransferUserAccountRequest
            {
                RequestingModerator = User.Identity.Name.ToLower(),
                OldUsername = request.OldUsername,
                NewUsername = request.NewUsername
            });

            if (!success)
            {
                ControllerContext.ModelState.AddModelError("TransferStatus",
                    "Could not transfer the user's data at this time");
                return View("TransferUser", request);
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Search()
        {
            if (!User.Identities.IsMod())
                RedirectToAction("Index", "Home");

            var model = new SearchViewModel
            {
                SongName= string.Empty,
                ArtistName = string.Empty,
                SearchResults = new List<SearchResult>()
            };

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> SubmitSearch(SearchViewModel search)
        {
            if (string.IsNullOrWhiteSpace(search.SongName) && string.IsNullOrWhiteSpace(search.ArtistName))
                return View("Search", new SearchViewModel
                {
                    SongName = string.Empty,
                    ArtistName = string.Empty,
                    SearchResults = new List<SearchResult>()
                });

            var searchResults = await _searchApiClient.FormattedSongSearch(new FormattedSongSearchRequest
            {
                SongName = search.SongName,
                ArtistName = search.ArtistName
            });

            var searchResultsViewModel = new List<SearchResult>();
            foreach (var result in searchResults.SearchResults)
            {
                searchResultsViewModel.Add(new SearchResult
                {
                    SongId = result.SongId,
                    SongName = result.SongName,
                    CharterUsername = result.CharterUsername,
                    SongArtist = result.ArtistName,
                    IsOfficial = result.IsOfficial,
                    IsDownloaded = result.IsDownloaded,
                    IsLinkDead = result.IsLinkDead
                });
            }

            return View("Search", new SearchViewModel
            {
                SongName = search.SongName,
                ArtistName = search.SongName,
                SearchResults = searchResultsViewModel
            });
        }

        [HttpPost]
        public async Task<IActionResult> DownloadToOneDrive([FromBody] SearchDownloadModel model)
        {
            if (model == null || model.SongId == 0)
                return BadRequest();

            var response = await _searchApiClient.DownloadToOneDrive(new DownloadToOneDriveRequest
            {
                SongId = model.SongId
            });

            if (!response)
                return BadRequest();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAuthBaseModel()
        {
            if (!User.Identity.IsAuthenticated) return Json(null);

            return Json(new TwitchAuthBaseModel
            {
                Username = User.Identity.Name,
                IsModerator = User.Identities.IsMod()
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SongSearch([FromBody] SongSearchRequestModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SongName) && string.IsNullOrWhiteSpace(model.ArtistName))
                return BadRequest("No Search Criteria Provided");

            var searchResults = await _searchApiClient.FormattedSongSearch(new FormattedSongSearchRequest
            {
                SongName = model.SongName,
                ArtistName = model.ArtistName
            });

            var searchResultsViewModel = searchResults.SearchResults.Select(
                result => new SearchResult
                {
                    SongId = result.SongId,
                    SongName = result.SongName,
                    CharterUsername = result.CharterUsername,
                    SongArtist = result.ArtistName,
                    IsOfficial = result.IsOfficial,
                    IsDownloaded = result.IsDownloaded,
                    IsLinkDead = result.IsLinkDead
                });

            return Json(searchResultsViewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ModerationTransferUser([FromBody] TransferUserViewModel request)
        {
            if (!User.Identities.IsMod())
            {
                return BadRequest("You are not authorized to do this");
            }

            var success = await _moderationApiClient.TransferUserAccount(new TransferUserAccountRequest
            {
                RequestingModerator = User.Identity.Name.ToLower(),
                OldUsername = request.OldUsername,
                NewUsername = request.NewUsername
            });

            if (!success)
            {
                return BadRequest("Could not transfer the user's data at this time");
            }

            return Ok(success);
        }
    }
}