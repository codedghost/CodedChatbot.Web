using System.Threading.Tasks;
using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.ApiClient.ApiClients;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Search;
using CoreCodedChatbot.Web.ViewModels.Chatbot;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoreCodedChatbot.Web.Controllers
{
    [EnableCors("Default")]
    public class ChatbotController : Controller
    {
        private readonly IPlaylistApiClient _playlistApiClient;
        private readonly IVipApiClient _vipApiClient;
        private readonly ISearchApiClient _searchApiClient;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(
            IPlaylistApiClient playlistApiClient,
            IVipApiClient vipApiClient,
            ISearchApiClient searchApiClient,
            ILogger<ChatbotController> logger)
        {
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
    }
}
