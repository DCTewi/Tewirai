using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sora.Entities.CQCodes;
using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;

using Tewirai.Logging;

namespace Tewirai.Plugins
{
    public class Lottery : IPlugin
    {
        public enum Mode
        {
            None,
            All,
            Collect,
        }

        private static bool CheckPermission(long groupId)
        {
            if (Tewirai.Config.Get<Lottery>().AllowedGroup.Contains(groupId))
            {
                return true;
            }

            Logger<Lottery>.LogInfo($"{groupId} 没有抽奖权限, 跳过");
            return false;
        }

        private static async Task MakeLotteryAllAsync(GroupMessageEventArgs e)
        {
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
        }


        private Dictionary<long, List<long>> _lotteryPool = new();

        private async Task MakeLotteryCollectAsync(GroupMessageEventArgs e)
        {
            if (_lotteryPool.ContainsKey(e.SourceGroup.Id))
            {
                var random = new Random();
                var group = _lotteryPool[e.SourceGroup.Id];

                if (group.Count == 0)
                {
                    await e.SourceGroup.SendGroupMessage("抽奖结束! 无人参与本次抽奖!");
                    return;
                }

                var targetQQ = group[random.Next(group.Count)];

                await e.SourceGroup.SendGroupMessage(new List<CQCode>
                {
                    CQCode.CQText("抽奖结束! "), CQCode.CQAt(targetQQ), CQCode.CQText("中奖力!")
                });

                _lotteryPool.Remove(e.SourceGroup.Id);
            }
            else
            {
                _lotteryPool[e.SourceGroup.Id] = new List<long>();
                await e.SourceGroup.SendGroupMessage("开始抽奖!\n发送【1】参加抽奖,\n再次发送【/抽奖】结束抽奖.");
            }
        }

        public void StartUp()
        {
            Tewirai.Events.OnGroupMessage += async (_, e) =>
            {
                var mode = e.Message.RawText switch
                {
                    "/全体抽奖" => Mode.All,
                    "/抽奖" => Mode.Collect,
                    _ => Mode.None,
                };

                if (e.Message.RawText == "1" && _lotteryPool.ContainsKey(e.SourceGroup.Id))
                {
                    _lotteryPool[e.SourceGroup.Id].Add(e.Sender.Id);

                    await e.SourceGroup.SendGroupMessage(new List<CQCode>
                    {
                        CQCode.CQAt(e.Sender.Id), CQCode.CQText($" 参与成功!\n当前共{_lotteryPool[e.SourceGroup.Id].Count}人参与")
                    });
                }
                else if (CheckPermission(e.SourceGroup.Id))
                {
                    switch (mode)
                    {
                        case Mode.All:
                            await MakeLotteryAllAsync(e);
                            break;
                        case Mode.Collect:
                            await MakeLotteryCollectAsync(e);
                            break;
                        default:
                            break;
                    }
                }
            };
        }
    }
}
