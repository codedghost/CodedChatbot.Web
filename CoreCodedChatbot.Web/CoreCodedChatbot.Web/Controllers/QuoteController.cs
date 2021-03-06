﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CodedChatbot.TwitchFactories.Interfaces;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Quotes;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Web.Extensions;
using CoreCodedChatbot.Web.Interfaces.Services;
using CoreCodedChatbot.Web.ViewModels.Quote;
using CoreCodedChatbot.Web.ViewModels.Quote.ChildModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwitchLib.Client;

namespace CoreCodedChatbot.Web.Controllers
{
    [Authorize]
    public class QuoteController : Controller
    {
        private readonly IQuoteApiClient _quoteApiClient;
        private readonly IConfigService _configService;
        private readonly ITwitchClientFactory _twitchClientFactory;

        public QuoteController(
            IQuoteApiClient quoteApiClient,
            IConfigService configService,
            ITwitchClientFactory twitchClientFactory)
        {
            _quoteApiClient = quoteApiClient;
            _configService = configService;
            _twitchClientFactory = twitchClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var quotes = await _quoteApiClient.GetQuotes();
            var isMod = false;

            if (User.Identity.IsAuthenticated)
            {
                isMod = User.Identities.IsMod();
            }

            var userSubmittedQuotes = quotes.Quotes.Select(q => new UserSubmittedQuote
            {
                QuoteId = q.QuoteId,
                QuoteText = q.QuoteText,
                CreatedBy = q.CreatedBy,
                Disabled = q.Disabled,
                EditedBy = q.LastEditedBy,
                EditedAt = q.EditedAt,
                ShowEditOrDelete = isMod || (User.Identity.IsAuthenticated &&
                                             q.CreatedBy.ToLower() == User.Identity.Name.ToLower())
            }).ToList();

            var quoteViewModels = new QuoteViewModel
            {
                UserSubmittedQuotes = userSubmittedQuotes,
                IsMod = isMod
            };

            return View(quoteViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> SendQuote([FromBody] QuoteActionModel quoteActionModel)
        {
            try
            {
                if (ModelState.IsValid && HttpContext.User.Identity.IsAuthenticated)
                {
                    var quote = await _quoteApiClient.GetQuote(new GetQuoteRequest
                    {
                        QuoteId = quoteActionModel.QuoteId
                    });

                    var client = _twitchClientFactory.Get();
                    client.SendMessage(_configService.Get<string>("StreamerChannel"),
                        $"Hey @{HttpContext.User.Identity.Name}, Here is Quote {quote.Quote.QuoteId}: {quote.Quote.QuoteText}");
                    return Ok();
                }
            }
            catch (Exception){ }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> EditQuote([FromBody] QuoteActionModel quoteActionModel)
        {
            var success = false;
            try
            {
                if (ModelState.IsValid && HttpContext.User.Identity.IsAuthenticated && !string.IsNullOrWhiteSpace(quoteActionModel.Text))
                {
                    success = await _quoteApiClient.EditQuote(new EditQuoteRequest
                    {
                        QuoteId = quoteActionModel.QuoteId,
                        QuoteText = quoteActionModel.Text,
                        Username = HttpContext.User.Identity.Name,
                        IsMod = User.Identities.IsMod()
                    });
                }
            } catch (Exception)
            {
                success = false;
            }

            if (!success) return BadRequest();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuote([FromBody] QuoteActionModel quoteActionModel)
        {
            var success = false;
            try
            {
                if (ModelState.IsValid && HttpContext.User.Identity.IsAuthenticated)
                {
                    success = await _quoteApiClient.RemoveQuote(new RemoveQuoteRequest
                    {
                        QuoteId = quoteActionModel.QuoteId,
                        Username = HttpContext.User.Identity.Name,
                        IsMod = User.Identities.IsMod()
                    });
                }
            }
            catch (Exception)
            {
                success = false;
            }

            if (!success) return BadRequest();

            return Ok();
        }
    }
}