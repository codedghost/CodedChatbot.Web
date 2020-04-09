using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.ViewModels.NavigationViewModel;
using Microsoft.AspNetCore.Mvc;
using GetStreamStatusRequest = CoreCodedChatbot.ApiContract.RequestModels.StreamStatus.GetStreamStatusRequest;

namespace CoreCodedChatbot.Web.Components
{
    public class NavViewComponent : ViewComponent
    {
        private readonly IStreamStatusApiClient _streamStatusClient;
        private readonly IChatterService _chatterService;
        private readonly IConfigService _configService;

        public NavViewComponent(
            IStreamStatusApiClient streamStatusClient, 
            IChatterService chatterService,
            IConfigService configService)
        {
            _streamStatusClient = streamStatusClient;
            _chatterService = chatterService;
            _configService = configService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var streamerChannel = _configService.Get<string>("StreamerChannel");

            var isUserMod = User.Identity.IsAuthenticated &&
                            _chatterService.GetCurrentChatters().IsUserMod(User.Identity.Name.ToLower());

            // Get the current page so we can redirect the user back here after login/logout
            // TODO: Consider if the page we are returning to will attempt to auto login or anything similar
            var currentPage = HttpContext.Request.Path;
            
            return View(new NavigationViewModel
            {
                IsBroadcasterOnline = true,
                LoginLogoutRedirect = currentPage,
                UserIsMod = isUserMod
            });
        }
    }
}