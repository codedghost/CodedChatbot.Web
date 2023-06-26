using System.Net.Http;
using System.Threading.Tasks;
using CodedGhost.Config;
using CoreCodedChatbot.ApiClient.DataHelper;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.StreamStatus;
using CoreCodedChatbot.ApiContract.ResponseModels.StreamStatus;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Secrets;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoreCodedChatbot.Web.Controllers
{
    [EnableCors]
    public class StatusController : Controller
    {
        private readonly ILogger<StatusController> _logger;

        private readonly HttpClient _streamStatusApiClient
            ;

        public StatusController(IConfigService configService, ISecretService secretService,
            ILogger<StatusController> logger)
        {
            _logger = logger;
            _streamStatusApiClient = HttpClientHelper.BuildClient(configService, secretService, "StreamStatus")
        }

        [HttpGet]
        public IActionResult Index()
        {
            return new JsonResult(true);
        }

        [HttpGet]
        public async Task<IActionResult> Streamer(string broadcaster)
        {
            var status = await _streamStatusApiClient.GetAsync<GetStreamStatusResponse>($"GetStreamStatus?broadcasterUsername={broadcaster}", _logger);

            return Ok(status.IsOnline);
        }
    }
}
