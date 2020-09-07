using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.Interfaces.Factories;
using CoreCodedChatbot.Web.Interfaces.Services;
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
        private readonly ITwitchApiFactory _twitchApiFactory;
        private readonly ILogger<ModService> _logger;
        private ChatViewersModel Chatters { get; set; }

        private string[] _moderators { get; set; }

        private Timer chatterTimer { get; set; }

        public ModService(
            IConfigService configService,
            ISecretService secretService,
            ITwitchApiFactory twitchApiFactory,
            ILogger<ModService> logger
        )
        {
            _configService = configService;
            _secretService = secretService;
            _twitchApiFactory = twitchApiFactory;
            _logger = logger;
            chatterTimer = new Timer((x) => { UpdateModList(); }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public async void UpdateModList()
        {
            var errorCounter = 0;

            try
            {
                var twitchApi = _twitchApiFactory.Get();
                var moderatorResponse =
                    await twitchApi.Helix.Moderation.GetModeratorsAsync(_configService.Get<string>("ChannelId"),
                        accessToken: _secretService.GetSecret<string>("ChatbotAccessToken"));
                _moderators = moderatorResponse.Data.Select(m => m.UserName.ToLower()).ToArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not access Twitch Helix API resource.");
                errorCounter++;

                if (errorCounter > 5)
                    Chatters.chatters.moderators = null;
            }
        }

        public bool IsUserModerator(string username)
        {
            var channelName = _configService.Get<string>("StreamerChannel");
            return (_moderators?.Contains(username?.ToLower()) ?? false) ||
                   string.Equals(channelName, username, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
