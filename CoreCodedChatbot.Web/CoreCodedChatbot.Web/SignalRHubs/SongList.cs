using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.SharedExternalRequestModels;
using CoreCodedChatbot.ApiContract.SignalRHubModels;
using CoreCodedChatbot.ApiContract.SignalRHubModels.Website;
using CoreCodedChatbot.ApiContract.SignalRHubModels.Website.ClientSpecific;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace CoreCodedChatbot.Web.SignalRHubs
{
    public class SongList : CGHub
    {
        private readonly ISecretService _secretService;
        private readonly IReactUiService _reactUiService;
        private readonly ILogger<SongList> _logger;

        public SongList(
            ISecretService secretService,
            IReactUiService reactUiService,
            ILogger<SongList> logger,
            IClientIdClient clientIdClient
        ) : base(clientIdClient)
        {
            _secretService = secretService;
            _reactUiService = reactUiService;
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client {Context.ConnectionId} Connected");
            return base.OnConnectedAsync();
        }

        public async Task SendAll(SongListHubModel data)
        {
            if (IsRequestAuthenticated(data.psk))
            {
                var currentSong = data.currentSong;
                var regularRequests = data.regularRequests;
                var vipRequests = data.vipRequests;
                await this.Clients.All.SendCoreAsync("SendAll", new object[] { currentSong, regularRequests, vipRequests });
            }
        }

        public async Task UpdateClients(SongListHubModel data)
        {
            if (IsRequestAuthenticated(data.psk))
            {
                var currentSong = _reactUiService.FormatUiModel(data.currentSong, true,
                    false);

                var regularQueue =
                    data.regularRequests.Where(r => r.songRequestId != (currentSong?.SongId ?? 0)).Select(r =>
                        _reactUiService.FormatUiModel(r, false, true));

                var vipQueue =
                    data.vipRequests.Where(r => r.songRequestId != (currentSong?.SongId ?? 0)).Select(r => _reactUiService.FormatUiModel(r, false, false));

                await Clients.All.SendCoreAsync("UpdateClients", new object[] {currentSong, regularQueue, vipQueue});
            }
        }

        public async Task PlaylistState(PlaylistStateUpdateModel data)
        {
            if (IsRequestAuthenticated(data.psk))
            {
                await Clients.All.SendCoreAsync("PlaylistState", new object[] { data.playlistState });
            }
        }

        public async Task NewRequest(PlaylistNewItemModel request)
        {
            await SendNewItemTask("NewRequest", request);
        }

        public async Task PromoteRequestToVip(PlaylistHtmlEditModel request)
        {
            await SendHtmlEditTask("PromoteToVip", request);
        }

        public async Task PromoteRequestToSuperVip(PlaylistHtmlEditModel request)
        {
            await SendHtmlEditTask("PromoteToSuperVip", request);
        }

        public async Task EditRequest(PlaylistHtmlEditModel request)
        {
            await SendHtmlEditTask("EditRequest", request);
        }

        public async Task MarkRequestInDrive(PlaylistBasicEditModel request)
        {
            await SendBasicEditTask("MarkRequestInDrive", request);
        }

        public async Task MarkRequestUserLeftChat(PlaylistBasicEditModel request)
        {
            await SendBasicEditTask("MarkRequestUserLeftChat", request);
        }

        public async Task RemoveCurrentRequest(PlaylistBasicEditModel request)
        {
            await SendBasicEditTask("RemoveCurrent", request);
        }

        public async Task RemoveRequest(PlaylistBasicEditModel request)
        {
            await SendBasicEditTask("RemoveSong", request);
        }

        public async Task UpdateVips(VipTotalUpdateModel request)
        {
            await SendClientTask("UpdateVips", request.ClientId, request.psk, new object [] { request.VipTotal });
        }

        public async Task UpdateBytes(ByteTotalUpdateModel request)
        {
            var tasks = request.ClientIds.Select(async clientId =>
                await SendClientTask("UpdateBytes", clientId, request.psk, new object[] {request.TotalBytes}));

            await Task.WhenAll(tasks.ToArray());
        }

        private async Task SendClientTask(string clientMethod, string clientId, string psk, object[] args)
        {
            if (IsRequestAuthenticated(psk))
                await Clients.Client(clientId).SendCoreAsync(clientMethod, args);
        }

        private async Task SendNewItemTask(string clientMethod, PlaylistNewItemModel request)
        {
            if (IsRequestAuthenticated(request.PreSharedKey))
                await SendTask(clientMethod, new object[] {request.SongId, request.Html, (int) request.RequestType});
        }

        private async Task SendHtmlEditTask(string clientMethod, PlaylistHtmlEditModel request)
        {
            if (IsRequestAuthenticated(request.PreSharedKey))
                await SendTask(clientMethod, new object[] {request.SongId, request.Html});
        }

        private async Task SendBasicEditTask(string clientMethod, PlaylistBasicEditModel request)
        {
            if (IsRequestAuthenticated(request.PreSharedKey))
                await SendTask(clientMethod, new object[] {request.SongId});
        }

        private bool IsRequestAuthenticated(string psk)
        {
            var signalRKey = _secretService.GetSecret<string>("SignalRKey");

            return psk == signalRKey;
        }

        private async Task SendTask(string clientMethod, object[] data)
        {
            await Clients.All.SendCoreAsync(clientMethod, data);
        }
    }
}
