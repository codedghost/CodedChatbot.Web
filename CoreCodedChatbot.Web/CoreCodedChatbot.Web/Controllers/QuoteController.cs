using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CodedChatbot.TwitchFactories.Interfaces;
using CoreCodedChatbot.ApiClient.DataHelper;
using CoreCodedChatbot.ApiContract.RequestModels.Quotes;
using CoreCodedChatbot.ApiContract.ResponseModels.Quotes;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Extensions;
using CoreCodedChatbot.Web.ViewModels.Quote;
using CoreCodedChatbot.Web.ViewModels.Quote.ChildModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoreCodedChatbot.Web.Controllers
{
    [Authorize]
    [EnableCors]
    public class QuoteController : Controller
    {
        private readonly HttpClient _quoteApiClient;
        private readonly IConfigService _configService;
        private readonly ITwitchClientFactory _twitchClientFactory;
        private readonly ILogger<QuoteController> _logger;

        public QuoteController(
            IConfigService configService,
            ISecretService secretService,
            ITwitchClientFactory twitchClientFactory,
            ILogger<QuoteController> logger)
        {
            _quoteApiClient = HttpClientHelper.BuildClient(configService, secretService, "Quote");
            _configService = configService;
            _twitchClientFactory = twitchClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var quotes = await _quoteApiClient.GetAsync<GetQuotesResponse>("GetQuotes", _logger);
            var isMod = false;

            if (User.Identity.IsAuthenticated)
            {
                isMod = User.Identities.IsMod();
            }

            var userSubmittedQuotes = quotes.Quotes.Select(q => new UserSubmittedQuote
            {
                QuoteId = q.QuoteId,
                QuoteText = q.QuoteText,
                CreatedBy = q.CreatedBy,
                Disabled = q.Disabled,
                EditedBy = q.LastEditedBy,
                EditedAt = q.EditedAt,
                ShowEditOrDelete = isMod || (User.Identity.IsAuthenticated &&
                                             q.CreatedBy.ToLower() == User.Identity.Name.ToLower())
            }).ToList();

            var quoteViewModels = new QuoteViewModel
            {
                UserSubmittedQuotes = userSubmittedQuotes,
                IsMod = isMod
            };

            return View(quoteViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> SendQuote([FromBody] QuoteActionModel quoteActionModel)
        {
            try
            {
                if (ModelState.IsValid && HttpContext.User.Identity.IsAuthenticated)
                {
                    var quote = await _quoteApiClient.GetAsync<GetQuoteResponse>(
                        $"GetQuote?quoteId={quoteActionModel.QuoteId}", _logger);

                    var client = _twitchClientFactory.Get();
                    client.SendMessage(_configService.Get<string>("StreamerChannel"),
                        $"Hey @{HttpContext.User.Identity.Name}, Here is Quote {quote.Quote.QuoteId}: {quote.Quote.QuoteText}");
                    return Ok();
                }
            }
            catch (Exception){ }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> EditQuote([FromBody] QuoteActionModel quoteActionModel)
        {
            var success = false;
            try
            {
                if (ModelState.IsValid && HttpContext.User.Identity.IsAuthenticated && !string.IsNullOrWhiteSpace(quoteActionModel.Text))
                {
                    success = await _quoteApiClient.PostAsync<EditQuoteRequest, bool>("EditQuote", new EditQuoteRequest
                    {
                        QuoteId = quoteActionModel.QuoteId,
                        QuoteText = quoteActionModel.Text,
                        Username = HttpContext.User.Identity.Name,
                        IsMod = User.Identities.IsMod()
                    }, _logger);
                }
            } catch (Exception)
            {
                success = false;
            }

            if (!success) return BadRequest();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuote([FromBody] QuoteActionModel quoteActionModel)
        {
            var success = false;
            try
            {
                if (ModelState.IsValid && HttpContext.User.Identity.IsAuthenticated)
                {
                    success = await _quoteApiClient.PostAsync<RemoveQuoteRequest, bool>("RemoveQuote", new RemoveQuoteRequest
                    {
                        QuoteId = quoteActionModel.QuoteId,
                        Username = HttpContext.User.Identity.Name,
                        IsMod = User.Identities.IsMod()
                    }, _logger);
                }
            }
            catch (Exception)
            {
                success = false;
            }

            if (!success) return BadRequest();

            return Ok();
        }
    }
}