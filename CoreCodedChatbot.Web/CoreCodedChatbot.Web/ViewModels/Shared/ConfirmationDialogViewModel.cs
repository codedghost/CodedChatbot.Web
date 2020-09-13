namespace CoreCodedChatbot.Web.ViewModels.Shared
{
    public class ConfirmationDialogViewModel
    {
        public string ModalId { get; set; }
        public string ModalTitle { get; set; }
        public string CloseButtonText { get; set; }
        public string ConfirmationButtonText { get; set; }
        public string BodyText { get; set; }
        public string CloseFunctionName { get; set; }
        public string CloseButtonAction { get; set; }
        public string ConfirmFunctionName { get; set; }
        public string ConfirmationButtonAction { get; set; }
    }
}