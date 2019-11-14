using System;
using System.Threading.Tasks;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.SignalR;
using CoreCodedChatbot.Secrets;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CoreCodedChatbot.Web.SignalRHubs
{
    public class SongList : Hub
    {
        private readonly ISecretService _secretService;
        private readonly ILogger<SongList> _logger;

        public SongList(
            ISecretService secretService, 
            ILogger<SongList> logger
        )
        {
            _secretService = secretService;
            _logger = logger;
        }

        public async Task SendAll(SongListHubModel data)
        {
            var psk = _secretService.GetSecret<string>("SignalRKey");

            if (psk == data.psk)
            {
                var currentSong = data.currentSong;
                var regularRequests = data.regularRequests;
                var vipRequests = data.vipRequests;
                await this.Clients.All.SendCoreAsync("SendAll", new object[] { currentSong, regularRequests, vipRequests });
            }
        }

        public async Task UpdateById(SongListSingleSongModel updateModel)
        {
            var psk = _secretService.GetSecret<string>("SignalRKey");

            if (psk == updateModel.psk)
            {
                var updateSong = updateModel.PlaylistItem;

                await Clients.All.SendCoreAsync("UpdateSong", new object[] { updateSong });
            }
        }
        
        public async Task RemoveById(SongListSingleSongModel removeModel)
        {
            var psk = _secretService.GetSecret<string>("SignalRKey");

            if (psk == removeModel.psk)
            {
                var removeSong = removeModel.PlaylistItem.songRequestId;

                await Clients.All.SendCoreAsync("RemoveSong", new object[] { removeSong });
            }
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client {Context.ConnectionId} Connected");
            return base.OnConnectedAsync();
        }
    }
}
