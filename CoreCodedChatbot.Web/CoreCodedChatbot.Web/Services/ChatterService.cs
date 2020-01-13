using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Web.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CoreCodedChatbot.Web.Services
{
    public class ChatterService : IChatterService
    {
        private readonly IConfigService _configService;
        private readonly ILogger<ChatterService> _logger;
        private ChatViewersModel Chatters { get; set; }

        private Timer chatterTimer { get; set; }

        public ChatterService(
            IConfigService configService,
            ILogger<ChatterService> logger
        )
        {
            _configService = configService;
            _logger = logger;
            chatterTimer = new Timer((x) => { UpdateChatters(); }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public async void UpdateChatters()
        {
            var errorCounter = 0;

            try
            {
                errorCounter = 0;
                var httpClient = new HttpClient();
                var request =
                    await httpClient.GetAsync($"https://tmi.twitch.tv/group/user/{_configService.Get<string>("StreamerChannel")}/chatters");

                if (!request.IsSuccessStatusCode) return;

                var currentChattersJson = await request.Content.ReadAsStringAsync();
                // process json into username list.
                Chatters = JsonConvert.DeserializeObject<ChatViewersModel>(currentChattersJson);

                // Add broadcaster to modlist
                Chatters.chatters.moderators =
                    Chatters.chatters.moderators.Union(Chatters.chatters.broadcaster).ToArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not access Twitch TMI resource.");
                errorCounter++;

                if (errorCounter > 5)
                    Chatters.chatters.moderators = null;
            }
        }

        public ChatViewersModel GetCurrentChatters()
        {
            return Chatters;
        }
    }
}
