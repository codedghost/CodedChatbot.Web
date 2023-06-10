using System.Threading.Tasks;
using CoreCodedChatbot.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    [EnableCors]
    public class LoginController : Controller
    {
        private string _frontendUrl;

        public LoginController(IConfigService configService)
        {
            _frontendUrl = configService.Get<string>("frontendUrl");
        }

        public IActionResult Index(string redirectUrl = "/")
        {
            return Challenge(new AuthenticationProperties() { RedirectUri = $"{_frontendUrl}{redirectUrl}"});
        }

        public async Task<IActionResult> Logout(string redirectUrl = "/")
        {
            await HttpContext.SignOutAsync();
            return Redirect($"{_frontendUrl}{redirectUrl}");
        }

        [HttpGet]
        public IActionResult GetLoggedInUser()
        {
            return new JsonResult(new { username = User.Identity.IsAuthenticated ? User.Identity.Name : string.Empty});
        }
    }
}