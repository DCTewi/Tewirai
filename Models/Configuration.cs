using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Tewirai.Logging;

namespace Tewirai.Models
{
    public class Configuration
    {
        public CqCoreConfig CqCore { get; set; } = new CqCoreConfig
        {
            WorkingDirectory = ".",
            ExcutableName = "go-cqhttp",
            WsListenPort = 9001
        };

        [JsonExtensionData]
        private IDictionary<string, JToken> ExtendData { get; set; }

        public PluginConstraintor Get<T>()
        {
            var key = typeof(T).Name;
            if (ExtendData.ContainsKey(key))
            {
                var configJson = ExtendData[key];
                try
                {
                    var result = JsonConvert.DeserializeObject<PluginConstraintor>(configJson.ToString());
                    return result;
                }
                catch { }
            }

            Logger<Configuration>.LogError($"未找到 {key} 的相关配置, 使用默认配置");
            return new PluginConstraintor();
        }
    }
}
