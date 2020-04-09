using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Moderation;
using CoreCodedChatbot.Web.Interfaces;
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
        private readonly ILogger<ModerationController> _logger;

        public ModerationController(
            IChatterService chatterService,
            IModerationApiClient moderationApiClient,
            ILogger<ModerationController> logger)
        {
            _chatterService = chatterService;
            _moderationApiClient = moderationApiClient;
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
    }
}