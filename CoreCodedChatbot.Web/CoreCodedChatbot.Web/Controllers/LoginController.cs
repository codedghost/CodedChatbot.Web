using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class LoginController : Controller
    {
        public LoginController()
        {
        }

        public IActionResult Index(string redirectUrl = "/")
        {
            return Challenge(new AuthenticationProperties() { RedirectUri = redirectUrl });
        }

        public async Task<IActionResult> Logout(string redirectUrl = "/")
        {
            await HttpContext.SignOutAsync();
            return Redirect(redirectUrl);
        }
    }
}