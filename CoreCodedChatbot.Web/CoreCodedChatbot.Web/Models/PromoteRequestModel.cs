namespace CoreCodedChatbot.Web.Models
{
    public class PromoteRequestModel : UiSongAtomicActionRequestModel
    {
        public bool useVip { get; set; }
        public bool useSuperVip { get; set; }
    }
}