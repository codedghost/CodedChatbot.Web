using System;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.ClientId;
using CoreCodedChatbot.Web.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace CoreCodedChatbot.Web.SignalRHubs
{
    public abstract class CGHub : Hub
    {
        private readonly IClientIdClient _clientIdClient;

        public CGHub(IClientIdClient clientIdClient)
        {
            _clientIdClient = clientIdClient;
        }

        public override async Task OnConnectedAsync()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                var hubType = GetType().Name;
                var clientId = Context.ConnectionId;
                var username = Context.User.GetTwitchUsername();

                if (!string.IsNullOrEmpty(username))
                {
                    await _clientIdClient.Set(new SetClientIdRequestModel
                    {
                        HubType = hubType,
                        ClientId = clientId,
                        Username = username
                    });
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var hubType = GetType().Name;
            var clientId = Context.ConnectionId;

            await _clientIdClient.Remove(new RemoveClientIdRequestModel
            {
                HubType = hubType,
                ClientId = clientId
            });

            await  base.OnDisconnectedAsync(exception);
        }
    }
}