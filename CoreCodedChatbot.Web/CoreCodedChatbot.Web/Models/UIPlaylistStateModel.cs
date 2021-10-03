using System.Collections.Generic;

namespace CoreCodedChatbot.Web.Models
{
    public class UiPlaylistStateModel
    {
        public SongRequest CurrentSong { get; set; }
        public List<SongRequest> RegularQueue { get; set; }
        public List<SongRequest> VipQueue { get; set; }
    }

    public class SongRequest
    {
        public int SongId { get; set; }
        public string SongTitle { get; set; }
        public string SongArtist { get; set; }
        public string Instrument { get; set; }
        public string Requester { get; set; }
        public bool IsInDrive { get; set; }
        public bool IsInChat { get; set; }
        public bool IsVip { get; set; }
        public bool IsSuperVip { get; set; }
    }
}