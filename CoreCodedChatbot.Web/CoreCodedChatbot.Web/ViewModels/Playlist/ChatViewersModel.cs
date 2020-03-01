using System;
using System.Linq;

namespace CoreCodedChatbot.Web.ViewModels.Playlist
{
    public class ChatViewersModel
    {
        public object _links { get; set; }
        public int chatter_count { get; set; }
        public ChattersModel chatters { get; set; }

        public bool IsUserMod(string username) =>
            chatters?.moderators?.Any(mod =>
                string.Equals(mod, username, StringComparison.InvariantCultureIgnoreCase)) ?? false;
    }

    public class ChattersModel
    {
        public string[] broadcaster { get; set; }
        public string[] moderators { get; set; }
        public string[] staff { get; set; }
        public string[] admins { get; set; }
        public string[] global_mods { get; set; }
        public string[] vips { get; set; }
        public string[] viewers { get; set; }
    }
}