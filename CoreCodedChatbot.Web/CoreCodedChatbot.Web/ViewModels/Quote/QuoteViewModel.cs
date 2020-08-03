using System.Collections.Generic;
using CoreCodedChatbot.Web.ViewModels.Quote.ChildModels;

namespace CoreCodedChatbot.Web.ViewModels.Quote
{
    public class QuoteViewModel
    {
        public List<UserSubmittedQuote> UserSubmittedQuotes { get; set; }
        public bool IsMod { get; set; }
    }
}