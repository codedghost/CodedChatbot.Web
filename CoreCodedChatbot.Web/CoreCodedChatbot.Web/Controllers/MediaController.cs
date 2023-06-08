using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    [EnableCors("Default")]
    public class MediaController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}