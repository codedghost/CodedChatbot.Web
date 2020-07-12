using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CoreCodedChatbot.Web.Models;

namespace CoreCodedChatbot.Web.ViewModels.Moderation
{
    public class SearchViewModel
    {
        [Display(Name = "Please enter the song name")]
        public string SongName { get; set; }

        [Display(Name = "Please enter the artist name")]
        public string ArtistName { get; set; }

        public List<SearchResult> SearchResults { get; set; }
    }
}