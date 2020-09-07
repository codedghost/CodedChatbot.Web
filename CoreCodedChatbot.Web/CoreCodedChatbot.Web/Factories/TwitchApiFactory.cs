using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Interfaces.Factories;
using TwitchLib.Api;

namespace CoreCodedChatbot.Web.Factories
{
    public class TwitchApiFactory : ITwitchApiFactory
    {
        private readonly ISecretService _secretService;

        private TwitchAPI _api;

        public TwitchApiFactory(ISecretService secretService)
        {
            _secretService = secretService;
        }

        public TwitchAPI Get()
        {
            if (_api == null || _api.Settings == null || string.IsNullOrWhiteSpace(_api.Settings.ClientId) || string.IsNullOrWhiteSpace(_api.Settings.AccessToken))
                Reconnect();

            return _api;
        }

        public void Reconnect()
        {
            _api = new TwitchAPI();
            _api.Settings.ClientId = _secretService.GetSecret<string>("ChatbotAccessClientId");
            _api.Settings.AccessToken = _secretService.GetSecret<string>("ChatbotAccessToken");
        }
    }
}