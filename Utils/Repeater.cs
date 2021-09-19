using Tewirai.Logging;

namespace Tewirai.Utils
{
    public class Repeater : IUtil
    {
        public void StartUp()
        {
            Program.SoraServer.Event.OnPrivateMessage += async (sender, e) =>
            {
                if (!Program.AppConfig.Repeater.AllowedUser.Contains(e.SenderInfo.UserId))
                {
                    //Logger<Repeater>.LogWarning($"User [{e.SenderInfo.UserId}] don't have the permission!");
                    return;
                }

                await e.Sender.SendPrivateMessage(e.Message.MessageList);
            };
        }
    }
}
