namespace Tewirai.Utils
{
    public class Repeater : IPlugin
    {
        public void StartUp()
        {
            Tewirai.Events.OnPrivateMessage += async (sender, e) =>
            {
                if (!Tewirai.Config.Get<Repeater>().AllowedUser.Contains(e.SenderInfo.UserId))
                {
                    return;
                }

                await e.Sender.SendPrivateMessage(e.Message.MessageList);
            };
        }
    }
}
