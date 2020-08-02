namespace CoreCodedChatbot.Web.ViewModels.Quote.ChildModels
{
    public class UserSubmittedQuote
    {
        public int QuoteId { get; set; }
        public string QuoteText { get; set; }
        public string CreatedBy { get; set; }
        public bool ShowEditOrDelete { get; set; }
    }
}