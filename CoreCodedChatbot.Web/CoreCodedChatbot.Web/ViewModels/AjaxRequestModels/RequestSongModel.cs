namespace CoreCodedChatbot.Web.ViewModels.AjaxRequestModels
{
    public class RequestSongModel
    {
        public string SongRequestId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string SelectedInstrument { get; set; }
        public bool IsVip { get; set; }
        public bool IsSuperVip { get; set; }
    }
}