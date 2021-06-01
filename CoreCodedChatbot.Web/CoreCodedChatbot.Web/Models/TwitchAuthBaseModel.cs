namespace CoreCodedChatbot.Web.Controllers
{
    public class TwitchAuthBaseModel
    {
        public string Username { get; set; }
        public bool IsModerator { get; set; }
        public int Vips { get; set; }
    }
}