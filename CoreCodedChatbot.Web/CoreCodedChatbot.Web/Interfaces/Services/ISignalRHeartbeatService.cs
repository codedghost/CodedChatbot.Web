namespace CoreCodedChatbot.Web.Interfaces.Services
{
    public interface ISignalRHeartbeatService
    {
        void OnTimerCallback(object state);
        void NotifyClients();
    }
}
