using Newtonsoft.Json;

namespace CoreCodedChatbot.Web.ViewModels.SongLibrary
{
    public class SongLibraryRecords
    {
        /// <summary>
        /// List of Song Library Records
        /// </summary>
        [JsonProperty("sgvSongsMaster")]
        public SongLibraryRecord[] Songs { get; set; }
    }
}