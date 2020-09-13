using System.Collections.Generic;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Moderation;
using CoreCodedChatbot.ApiContract.RequestModels.Search;
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
        private readonly IModService _modService;
        private readonly IModerationApiClient _moderationApiClient;
        private readonly ISearchApiClient _searchApiClient;
        private readonly ILogger<ModerationController> _logger;

        public ModerationController(
            IModService modService,
            IModerationApiClient moderationApiClient,
            ISearchApiClient searchApiClient,
            ILogger<ModerationController> logger)
        {
            _modService = modService;
            _moderationApiClient = moderationApiClient;
            _searchApiClient = searchApiClient;
            _logger = logger;
        }

        [Authorize]
        public IActionResult TransferUser()
        {
            if (!_modService.IsUserModerator(User.Identity.Name))
                RedirectToAction("Index", "Home");

            return View(new TransferUserViewModel());
        }

        [Authorize]
        public async Task<IActionResult> ProcessTransferUser(TransferUserViewModel request)
        {
            if (!_modService.IsUserModerator(User.Identity.Name))
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
            if (!_modService.IsUserModerator(User.Identity.Name))
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
    }
}