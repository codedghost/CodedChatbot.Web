namespace CoreCodedChatbot.Web.Models
{
    public class SearchResult
    {
        public int SongId { get; set; }
        public string SongName { get; set; }
        public string CharterUsername { get; set; }
        public string SongArtist { get; set; }
        public bool IsDownloaded { get; set; }
        public bool IsOfficial { get; set; }
        public bool IsLinkDead { get; set; }
        public string DownloadUrl { get; set; }
    }
}