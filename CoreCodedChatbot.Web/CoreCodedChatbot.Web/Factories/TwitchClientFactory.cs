using CoreCodedChatbot.Config;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Interfaces.Factories;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Web.Factories
{
    public class TwitchClientFactory : ITwitchClientFactory
    {
        private readonly IConfigService _configService;
        private readonly ISecretService _secretService;

        private TwitchClient _client;

        public TwitchClientFactory(
            IConfigService configService,
            ISecretService secretService
            )
        {
            _configService = configService;
            _secretService = secretService;
        }

        public TwitchClient GetClient()
        {
            if (_client == null || !_client.IsConnected || !_client.IsInitialized)
                Reconnect();

            return _client;
        }

        private void Reconnect()
        {
            var creds = new ConnectionCredentials(
                _configService.Get<string>("ChatbotNick"),
                _secretService.GetSecret<string>("ChatbotPass"));
            _client = new TwitchClient();
            _client.Initialize(creds, _configService.Get<string>("StreamerChannel"));
            _client.Connect();
        }
    }
}