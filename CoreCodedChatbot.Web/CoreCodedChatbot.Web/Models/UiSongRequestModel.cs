namespace CoreCodedChatbot.Web.Models
{
    public class UiSongRequestModel
    {
        public int songRequestId { get; set; }
        public string songName { get; set; }
        public string artistName { get; set; }
        public string instrument { get; set; }
        public bool useVipToken { get; set; }
        public bool useSuperVipToken { get; set; }
    }
}