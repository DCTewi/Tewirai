using Newtonsoft.Json;
using Sora.Server;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tewirai.Logging;
using Tewirai.Models;
using Tewirai.Utils;

namespace Tewirai
{
    public static class Program
    {
        public static SoraWSServer Instance { get; private set; }
        public static AppConfig AppConfig { get; private set; }
        public static SoraWSServer SoraServer { get; private set; }

        public static async Task Main(string[] args)
        {
            AddAppConfig();
            AddCqCore();
            AddSoraServer();
            AddUtils();

            Logger<Task>.LogInfo("Starting bot...");
            await SoraServer.StartServer();
        }

        private static void AddAppConfig()
        {
            Logger<Task>.LogInfo("Checking application configs...");
            if (!File.Exists(@"config.json"))
            {
                using var fs = File.OpenWrite(@"config.json");
                var initStr = JsonConvert.SerializeObject(new AppConfig(), Formatting.Indented);
                fs.Write(Encoding.UTF8.GetBytes(initStr));
                fs.Close();

                Logger<Task>.LogError("Please edit the new config.json");
                Environment.Exit(0);
            }

            AppConfig = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(@"config.json"));
        }

        private static void AddCqCore()
        {
            Logger<Task>.LogInfo("Starting cq core...");
            try
            {
                using var process = new Process();
                process.StartInfo.WorkingDirectory = AppConfig.CqCore.WorkingDirectory;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = Path.Combine(AppConfig.CqCore.WorkingDirectory, AppConfig.CqCore.ExcutableName);
                process.Start();
            }
            catch (Exception e)
            {
                Logger<Task>.LogFatal(e.Message);
            }
        }

        private static void AddSoraServer()
        {
            Logger<Task>.LogInfo("Starting sora server...");
            var serverConfig = new ServerConfig
            {
                Port = AppConfig.CqCore.WsListenPort
            };

            SoraServer = new SoraWSServer(serverConfig);
        }

        private static void AddUtils()
        {
            var type = typeof(IUtil);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

            foreach (var t in types)
            {
                Logger<Task>.LogInfo($"Injecting {t.Name} to sora server...");
                IUtil util = Activator.CreateInstance(t) as IUtil;
                util.StartUp();
            }
        }
    }
}
