using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.StreamStatus;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class StatusController : Controller
    {
        private readonly IStreamStatusApiClient _streamStatusApiClient;

        public StatusController(IStreamStatusApiClient streamStatusApiClient)
        {
            _streamStatusApiClient = streamStatusApiClient;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return new JsonResult(true);
        }

        [HttpGet]
        public async Task<IActionResult> Streamer(string broadcaster)
        {
            var status = await _streamStatusApiClient.GetStreamStatus(new GetStreamStatusRequest
            {
                BroadcasterUsername = broadcaster
            });

            return Ok(status);
        }
    }
}
