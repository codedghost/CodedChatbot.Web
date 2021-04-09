using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class StatusController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return new JsonResult(true);
        }
    }
}
