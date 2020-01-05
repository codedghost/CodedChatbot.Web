using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class DevelopmentController : Controller
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}