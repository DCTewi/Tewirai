using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Sora.Interfaces;
using Sora.Net;
using Sora.OnebotInterface;
using Sora.OnebotModel;

using Tewirai.Logging;
using Tewirai.Models;
using Tewirai.Plugins;
namespace Tewirai
{
    public class Tewirai : IDisposable
    {
        private static Tewirai _instance;
        public static Tewirai Instance
        {
            get { return _instance ??= new(); }
        }

        public static EventInterface Events => Instance._service.Event;

        public static ISoraService Service => Instance._service;

        public static Configuration Config => Instance._config;

        private Tewirai() { }

        private ISoraService _service;
        private Configuration _config;
        private Process _cqProcess;

        public async Task RunAsync()
        {
            GetOrCreateAppConfig();
            StartUpCqCore();
            StartUpSoraService();
            TraversePlugins();

            Logger<Tewirai>.LogInfo("正在启动bot");
            await _service.StartService();
        }

        private void TraversePlugins()
        {
            var pluginType = typeof(IPlugin);
            var actualTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => pluginType.IsAssignableFrom(type) && !type.IsInterface);

            foreach (var t in actualTypes)
            {
                Logger<Tewirai>.LogInfo($"正在注入插件: {t.Name}");
                IPlugin plugin = Activator.CreateInstance(t) as IPlugin;
                plugin.StartUp();
            }
        }

        private void StartUpSoraService()
        {
            Logger<Tewirai>.LogInfo("正在启动Sora Server");
            var serverConfig = new ServerConfig
            {
                Port = _config.CqCore.WsListenPort,
            };

            _service = SoraServiceFactory.CreateInstance(serverConfig);
        }

        private void StartUpCqCore()
        {
            Logger<Tewirai>.LogInfo("正在启动CQ Core");
            try
            {
                foreach (var process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_config.CqCore.ExcutableName)))
                {
                    process.Kill();
                }

                _cqProcess = new Process();
                _cqProcess.StartInfo.WorkingDirectory = _config.CqCore.WorkingDirectory;
                _cqProcess.StartInfo.UseShellExecute = false;
                _cqProcess.StartInfo.FileName = Path.Combine(_config.CqCore.WorkingDirectory, _config.CqCore.ExcutableName);

                _cqProcess.Start();
            }
            catch (Exception ex)
            {
                Logger<Tewirai>.LogFatal(ex.Message);
            }
        }

        private void GetOrCreateAppConfig()
        {
            Logger<Tewirai>.LogInfo("正在加载配置文件");
            if (!File.Exists(@"config.json"))
            {
                var json = JsonConvert.SerializeObject(new Configuration(), Formatting.Indented);

                File.WriteAllText(@"config.json", json);

                Logger<Tewirai>.LogFatal("未找到配置文件, 已经重新生成配置文件, 请修改后重启");
            }

            _config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(@"config.json"));
        }

        public void Dispose()
        {
            Logger<Tewirai>.LogFatal("正在关闭远端CQ服务器");
            _cqProcess.Kill();
            _cqProcess.Close();
            ((IDisposable)_cqProcess).Dispose();
        }
    }
}
