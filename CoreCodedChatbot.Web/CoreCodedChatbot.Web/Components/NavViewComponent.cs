using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Library.Models.View.ComponentViewModels;
using Microsoft.AspNetCore.Mvc;
using GetStreamStatusRequest = CoreCodedChatbot.ApiContract.RequestModels.StreamStatus.GetStreamStatusRequest;

namespace CoreCodedChatbot.Web.Components
{
    public class NavViewComponent : ViewComponent
    {
        private readonly IStreamStatusClient _streamStatusClient;
        private readonly IConfigService _configService;

        public NavViewComponent(IStreamStatusClient streamStatusClient, IConfigService configService)
        {
            _streamStatusClient = streamStatusClient;
            _configService = configService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var streamerChannel = _configService.Get<string>("StreamerChannel");

            var streamStatus = await _streamStatusClient.GetStreamStatus(
                new GetStreamStatusRequest
                {
                    BroadcasterUsername = streamerChannel
                });
            
            return View(new NavigationViewModel
            {
                IsBroadcasterOnline = streamStatus.IsOnline
            });
        }
    }
}