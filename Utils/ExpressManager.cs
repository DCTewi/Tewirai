using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tewirai.Models;

namespace Tewirai.Utils
{
    public class ExpressManager : IUtil
    {
        public static Dictionary<long, ExpressInfo> ExpressInfos
        {
            get
            {
                string jsonthings = System.IO.File.ReadAllText("expressinfo.json");
                var result = JsonConvert.DeserializeObject<List<ExpressInfo>>(jsonthings)
                    .ToDictionary(e => e.QQ, e => e);

                return result;
            }
        }

        public void StartUp()
        {
            Program.SoraServer.Event.OnFriendRequest += CheckPermission;

            Program.SoraServer.Event.OnPrivateMessage += Receiver;
        }

        private async ValueTask Receiver(object sender, Sora.EventArgs.SoraEvent.PrivateMessageEventArgs eventArgs)
        {
            if (ExpressInfos.ContainsKey(eventArgs.Sender.Id) && eventArgs.Message.RawText.Contains("查询"))
            {
                ExpressInfo info = ExpressInfos[eventArgs.Sender.Id];
                string result = $"收件人：{info.RecNick}\n电话号码：{info.Phone}\n收件地址：{info.Address}\n若有问题，请联系冻葱Tewi";

                await eventArgs.Sender.SendPrivateMessage(result);
            }
        }

        private async ValueTask CheckPermission(object sender, Sora.EventArgs.SoraEvent.FriendRequestEventArgs eventArgs)
        {
            if (ExpressInfos.ContainsKey(eventArgs.Sender.Id))
            {
                await eventArgs.Accept();
            }
            else
            {
                await eventArgs.Reject();
            }
        }
    }
}
