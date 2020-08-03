using System;

namespace CoreCodedChatbot.Web.ViewModels.Quote.ChildModels
{
    public class UserSubmittedQuote
    {
        public int QuoteId { get; set; }
        public string QuoteText { get; set; }
        public string CreatedBy { get; set; }
        public bool Disabled { get; set; }
        public string EditedBy { get; set; }
        public DateTime EditedAt { get; set; }
        public bool ShowEditOrDelete { get; set; }
    }
}