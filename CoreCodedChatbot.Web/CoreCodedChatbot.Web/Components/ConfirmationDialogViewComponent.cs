using System.Threading.Tasks;
using CoreCodedChatbot.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Components
{
    public class ConfirmationDialogViewComponent : ViewComponent
    {
        public ConfirmationDialogViewComponent()
        {
            
        }

        public async Task<IViewComponentResult> InvokeAsync(ConfirmationDialogViewModel confirmationDialogViewComponent)
        {
            return View(confirmationDialogViewComponent);
        }
    }
}