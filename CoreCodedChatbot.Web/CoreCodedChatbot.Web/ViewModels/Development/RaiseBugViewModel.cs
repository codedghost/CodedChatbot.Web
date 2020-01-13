using System.ComponentModel.DataAnnotations;

namespace CoreCodedChatbot.Web.ViewModels.Development
{
    public class RaiseBugViewModel
    {
        [StringLength(100, MinimumLength = 10)]
        [Display(Name = "Please enter a short description of the issue *")]
        [Required(ErrorMessage = "This field is required")]
        public string Title { get; set; }

        [Display(Name = "What should have happened?")]
        public string AcceptanceCriteria { get; set; }

        [Display(Name = "What happened? * (Please be as detailed as possible including what you were doing, when etc)")]
        [Required(ErrorMessage = "Please enter some way of making this bug happen again")]
        public string ReproSteps { get; set; }

        public bool FailedToRaise { get; set; }
    }
}