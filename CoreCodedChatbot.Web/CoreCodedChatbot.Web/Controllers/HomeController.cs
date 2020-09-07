using CoreCodedChatbot.Config;
using CoreCodedChatbot.Web.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfigService _configService;

        public HomeController(IConfigService configService)
        {
            _configService = configService;
        }

        public ActionResult Index()
        {
            var viewModel = new HomepageViewModel
            {
                PreviewTwitchChannelName = _configService.Get<string>("StreamerChannel"),
                Domain = HttpContext.Request.Host.Host
            };

            return View(viewModel);
        }
    }
}
