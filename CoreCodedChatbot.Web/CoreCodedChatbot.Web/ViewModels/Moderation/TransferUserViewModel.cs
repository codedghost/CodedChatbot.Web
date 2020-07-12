using System.ComponentModel.DataAnnotations;

namespace CoreCodedChatbot.Web.ViewModels.Moderation
{
    public class TransferUserViewModel
    {
        [StringLength(50)]
        [Display(Name = "Old User Account name")]
        [Required(ErrorMessage = "This field is required")]
        public string OldUsername { get; set; }

        [StringLength(50)]
        [Display(Name = "New User Account name")]
        [Required(ErrorMessage = "This field is required")]
        public string NewUsername { get; set; }
    }
}