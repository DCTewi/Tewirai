using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sora.Entities.CQCodes;
using Sora.Enumeration.ApiType;

using Tewirai.Logging;

namespace Tewirai.Plugins
{
    public class Lottery : IPlugin
    {
        public void StartUp()
        {
            Tewirai.Events.OnGroupMessage += async (_, e) =>
            {
                if (e.Message.RawText != "/抽奖")
                {
                    return;
                }

                if (!Tewirai.Config.Get<Lottery>().AllowedGroup.Contains(e.SourceGroup.Id))
                {
                    Logger<Lottery>.LogInfo($"{e.SourceGroup.Id} 没有抽奖权限, 跳过");
                    return;
                }

                var (apiStatus, members) = await e.SourceGroup.GetGroupMemberList();

                if (apiStatus == APIStatusType.OK)
                {
                    var random = new Random();
                    var index = random.Next(members.Count);

                    var targetQQ = members[index].UserId;

                    await e.SourceGroup.SendGroupMessage(new List<CQCode>
                    {
                        CQCode.CQAt(targetQQ), CQCode.CQText("中奖力！")
                    });
                }
                else
                {
                    await e.SourceGroup.SendGroupMessage("获取群成员列表失败!");
                }
            };
        }
    }
}
