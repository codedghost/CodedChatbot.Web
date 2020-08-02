﻿using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.ViewModels.NavigationViewModel;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Components
{
    public class NavViewComponent : ViewComponent
    {
        private readonly IStreamStatusApiClient _streamStatusClient;
        private readonly IModService _modService;
        private readonly IConfigService _configService;

        public NavViewComponent(
            IStreamStatusApiClient streamStatusClient, 
            IModService modService,
            IConfigService configService)
        {
            _streamStatusClient = streamStatusClient;
            _modService = modService;
            _configService = configService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var isUserMod = (HttpContext?.User?.Identity?.IsAuthenticated ?? false) &&
                            _modService.IsUserModerator(User.Identity.Name);

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