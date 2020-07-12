using System.ComponentModel.DataAnnotations;

namespace CoreCodedChatbot.Web.ViewModels.Chatbot
{
    public class RequestSearchSynonymViewModel
    {
        [Display(Name = "Please enter your request in the format: A7X - Avenged Sevenfold")]
        public string SynonymRequest { get; set; }
    }
}