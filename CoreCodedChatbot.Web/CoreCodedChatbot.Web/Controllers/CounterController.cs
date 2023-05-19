using System.Net.Http;
using System.Threading.Tasks;
using CodedChatbot.TwitchFactories.Interfaces;
using CoreCodedChatbot.ApiClient.DataHelper;
using CoreCodedChatbot.ApiContract.ResponseModels.Counters;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.ViewModels.Counter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoreCodedChatbot.Web.Controllers
{
    [Authorize]
    [EnableCors("Default")]
    public class CounterController : Controller
    {
        private readonly HttpClient _counterHttpClient;
        private readonly IConfigService _configService;
        private readonly ILogger<CounterController> _logger;

        public CounterController(
            IConfigService configService,
            ISecretService secretService,
            ILogger<CounterController> logger)
        {
            _counterHttpClient = HttpClientHelper.BuildClient(configService, secretService, "Counters");
            _configService = configService;
            _logger = logger;
        }

        public async Task<IActionResult> Index([FromQuery] string counterName)
        {
            var counterModel =
                await _counterHttpClient.GetAsync<GetCounterResponse>($"GetCounter?counterName={counterName}",
                    _logger);

            return View(new CounterViewModel
            {
                CounterText = $"{counterModel.Counter.CounterPreText}: {counterModel.Counter.CounterValue}"
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCounter([FromQuery] string counterName)
        {
            var counterModel =
                await _counterHttpClient.GetAsync<GetCounterResponse>($"GetCounters?counterName={counterName}",
                    _logger);

            return Json(counterModel);
        }
    }
}