namespace CoreCodedChatbot.Web.Interfaces.Services
{
    public interface IModService
    {
        void UpdateModList();
        bool IsUserModerator(string username);
    }
}
