using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.ViewModels.Playlist;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;

namespace CoreCodedChatbot.Web.Services
{
    public class ModService : IModService
    {
        private readonly IConfigService _configService;
        private readonly ISecretService _secretService;
        private readonly TwitchAPI _twitchApi;
        private readonly ILogger<ModService> _logger;
        private ChatViewersModel Chatters { get; set; }

        private string[] _moderators { get; set; }

        private Timer chatterTimer { get; set; }

        public ModService(
            IConfigService configService,
            ISecretService secretService,
            TwitchAPI _twitchApi,
            ILogger<ModService> logger
        )
        {
            _configService = configService;
            _secretService = secretService;
            this._twitchApi = _twitchApi;
            _logger = logger;
            chatterTimer = new Timer((x) => { UpdateModList(); }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public async void UpdateModList()
        {
            var errorCounter = 0;

            try
            {
                var subs = await _twitchApi.V5.Channels.GetAllSubscribersAsync(_configService.Get<string>("ChannelId"),
                    _secretService.GetSecret<string>("ChatbotAccessToken"));
                _twitchApi.Settings.Scopes.Add(AuthScopes.Helix_Moderation_Read);
                var moderatorResponse =
                    await _twitchApi.Helix.Moderation.GetModeratorsAsync(_configService.Get<string>("StreamerChannel"),
                        accessToken: _secretService.GetSecret<string>("ChatbotAccessToken"));
                _moderators = moderatorResponse.Data.Select(m => m.UserName).ToArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not access Twitch TMI resource.");
                errorCounter++;

                if (errorCounter > 5)
                    Chatters.chatters.moderators = null;
            }
        }

        public bool IsUserModerator(string username)
        {
            return _moderators?.Contains(username?.ToLower()) ?? false;
        }
    }
}
