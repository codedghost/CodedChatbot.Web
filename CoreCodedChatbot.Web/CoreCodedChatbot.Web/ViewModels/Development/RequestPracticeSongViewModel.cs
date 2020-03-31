using System.ComponentModel.DataAnnotations;

namespace CoreCodedChatbot.Web.ViewModels.Development
{
    public class RequestPracticeSongViewModel
    {
        [StringLength(100, MinimumLength = 10)]
        [Display(Name = "Please enter the song name you wish Coded to learn *")]
        [Required(ErrorMessage = "This field is required")]
        public string SongName { get; set; }

        [Display(Name = "Any extra info for learning this song?")]
        public string ExtraInformation { get; set; }
    }
}