using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CoreCodedChatbot.Web.Models;

namespace CoreCodedChatbot.Web.ViewModels.Moderation
{
    public class SearchViewModel
    {
        [Display(Name = "Please enter your search terms")]
        [Required(ErrorMessage = "This field is required")]
        public string SearchTerms { get; set; }

        public List<SearchResults> SearchResults { get; set; }
    }
}