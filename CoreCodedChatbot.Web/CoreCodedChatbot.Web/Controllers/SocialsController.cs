using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    [EnableCors]
    public class SocialsController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}