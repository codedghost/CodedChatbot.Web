using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.ViewModels.Quote;
using CoreCodedChatbot.Web.ViewModels.Quote.ChildModels;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class QuoteController : Controller
    {
        private readonly IQuoteApiClient _quoteApiClient;
        private readonly IModService _modService;

        public QuoteController(
            IQuoteApiClient quoteApiClient, 
            IModService modService)
        {
            _quoteApiClient = quoteApiClient;
            _modService = modService;
        }

        public async Task<IActionResult> Index()
        {
            var quotes = await _quoteApiClient.GetQuotes();
            var isMod = false;

            if (User.Identity.IsAuthenticated)
            {
                isMod = _modService.IsUserModerator(User.Identity.Name);
            }

            var quoteViewModels = new QuoteViewModel
            {
                UserSubmittedQuotes = quotes.Quotes.Select(q => new UserSubmittedQuote
                {
                    QuoteId = q.QuoteId,
                    QuoteText = q.QuoteText,
                    CreatedBy = q.CreatedBy,
                    ShowEditOrDelete = isMod || (User.Identity.IsAuthenticated && q.CreatedBy.ToLower() == User.Identity.Name.ToLower())
                }).ToList()
            };

            return View(quoteViewModels);
        }

        public async void SendQuote(int quoteId)
        {

        }
    }
}