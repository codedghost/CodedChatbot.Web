using Newtonsoft.Json;

namespace CoreCodedChatbot.Web.ViewModels.SongLibrary
{
    public class SongLibraryRecord
    {
        /// <summary>
        /// Row Id of entry
        /// </summary>
        [JsonProperty("rowId")]
        public int Id { get; set; }

        /// <summary>
        /// Song Title of entry
        /// </summary>
        [JsonProperty("colTitle")]
        public string Title { get; set; }

        /// <summary>
        /// Artist of entry
        /// </summary>
        [JsonProperty("colArtist")]
        public string Artist { get; set; }
    }
}