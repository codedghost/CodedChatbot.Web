using System.Collections.Generic;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.DevOps;
using CoreCodedChatbot.ApiContract.ResponseModels.DevOps.ChildModels;
using CoreCodedChatbot.Web.ViewModels.Development;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{

    public class DevelopmentController : Controller
    {
        private readonly IDevOpsApiClient _devOpsApiClient;

        public DevelopmentController(IDevOpsApiClient devOpsApiClient)
        {
            _devOpsApiClient = devOpsApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentIterationWorkItems = await _devOpsApiClient.GetAllCurrentWorkItems();

            return View("CurrentIteration", new CurrentIterationViewModel
            {
                WorkItems = currentIterationWorkItems?.WorkItems ?? new List<DevOpsWorkItem>()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Backlog()
        {
            var backlogWorkItems = await _devOpsApiClient.GetAllBacklogWorkItems();

            return View("Backlog", new BacklogViewModel
            {
                WorkItems = backlogWorkItems.WorkItems
            });
        }

        [HttpGet]
        [Authorize]
        public IActionResult RaiseBug()
        {
            return View("RaiseBug", new RaiseBugViewModel());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitBug(RaiseBugViewModel model)
        { 
            if (!ModelState.IsValid)
                return View("RaiseBug", model);

            var success = await _devOpsApiClient.RaiseBug(new RaiseBugRequest
            {
                TwitchUsername = HttpContext.User.Identity.Name,
                BugInfo = new DevOpsBug
                {
                    Title = model.Title,
                    AcceptanceCriteria = model.AcceptanceCriteria,
                    ReproSteps = model.ReproSteps,
                    Tasks = new List<DevOpsTask>
                    {
                        new DevOpsTask
                        {
                            Title = "Investigate",
                            RemainingWork = 1
                        }
                    }
                }
            });

            if (success) return View("Thanks");

            model.FailedToRaise = true;
            return View("RaiseBug", model);


        }
    }
}