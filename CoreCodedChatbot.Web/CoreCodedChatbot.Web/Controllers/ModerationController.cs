using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Moderation;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.Models;
using CoreCodedChatbot.Web.Services;
using CoreCodedChatbot.Web.ViewModels.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoreCodedChatbot.Web.Controllers
{
    public class ModerationController : Controller
    {
        private readonly IChatterService _chatterService;
        private readonly IModerationApiClient _moderationApiClient;
        private readonly ISolrService _solrService;
        private readonly IDownloadChartService _donDownloadChartService;
        private readonly IChatbotContextFactory _chatbotContextFactory;
        private readonly ILogger<ModerationController> _logger;

        public ModerationController(
            IChatterService chatterService,
            IModerationApiClient moderationApiClient,
            ISolrService solrService,
            IDownloadChartService donDownloadChartService,
            IChatbotContextFactory chatbotContextFactory,
            ILogger<ModerationController> logger)
        {
            _chatterService = chatterService;
            _moderationApiClient = moderationApiClient;
            _solrService = solrService;
            _donDownloadChartService = donDownloadChartService;
            _chatbotContextFactory = chatbotContextFactory;
            _logger = logger;
        }

        [Authorize]
        public IActionResult TransferUser()
        {
            var chattersModel = _chatterService.GetCurrentChatters();

            if (!chattersModel?.IsUserMod(User.Identity.Name) ?? false)
                RedirectToAction("Index", "Home");

            return View(new TransferUserViewModel());
        }

        [Authorize]
        public async Task<IActionResult> ProcessTransferUser(TransferUserViewModel request)
        {
            var chattersModel = _chatterService.GetCurrentChatters();

            if (!chattersModel?.IsUserMod(User.Identity.Name) ?? false)
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
            var chattersModel = _chatterService.GetCurrentChatters();

            if (!chattersModel?.IsUserMod(User.Identity.Name) ?? false)
                RedirectToAction("Index", "Home");

            var model = new SearchViewModel
            {
                SearchTerms = String.Empty,
                SearchResults = new List<SearchResults>()

            };

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> SubmitSearch(SearchViewModel search)
        {
            if (string.IsNullOrWhiteSpace(search.SearchTerms))
                return View("Search", new SearchViewModel
                {
                    SearchTerms = string.Empty,
                    SearchResults = new List<SearchResults>()
                });

            var result = await _solrService.Search(search.SearchTerms);
            if (!result.Any())
                return View("Search", new SearchViewModel
                {
                    SearchTerms = search.SearchTerms,
                    SearchResults = new List<SearchResults>()
                });

            using (var context = _chatbotContextFactory.Create())
            {
                var songIds = result.Select(r => r.SongId);

                var songs = context.Songs.Where(s => songIds.Contains(s.SongId));

                return View("Search", new SearchViewModel
                {
                    SearchTerms = search.SearchTerms,
                    SearchResults = songs.Select(s => new SearchResults
                    {
                        SongName = s.SongName,
                        SongArtist = s.SongArtist,
                        DownloadUrl = s.DownloadUrl
                    }).ToList()
                });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DownloadToOneDrive([FromBody] int songId)
        {
            using (var context = _chatbotContextFactory.Create())
            {
                var song = context.Songs.Find(songId);

                _donDownloadChartService.Download(song.DownloadUrl, song.SongId);

                return RedirectToAction("Search", "Moderation");
            }
        }
    }
}