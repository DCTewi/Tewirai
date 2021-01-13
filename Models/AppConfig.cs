namespace Tewirai.Models
{
    public class AppConfig
    {
        public CqCoreConfig CqCore { get; set; } = new CqCoreConfig
        {
            WorkingDirectory = ".",
            ExcutableName = "go-cqhttp",
            WsListenPort = 9001
        };
        public UtilConfig Repeater { get; set; } = new UtilConfig
        {
            AllowedGroup = new System.Collections.Generic.List<long>(),
            AllowedUser = new System.Collections.Generic.List<long>()
        };
    }
}
