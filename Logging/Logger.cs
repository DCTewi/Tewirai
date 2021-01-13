using System;

namespace Tewirai.Logging
{
    public static class Logger<T>
    {
        private enum LogType
        {
            Info,
            Warning,
            Error,
            Fatal
        }

        private static readonly string TypeName = typeof(T).Name;

        private static void LogRaw(LogType logType, string msg)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}][{logType}][{TypeName}]{msg}");
        }

        public static void LogInfo(string msg)
        {
            LogRaw(LogType.Info, msg);
        }

        public static void LogWarning(string msg)
        {
            LogRaw(LogType.Warning, msg);
        }

        public static void LogError(string msg)
        {
            LogRaw(LogType.Error, msg);
        }

        public static void LogFatal(string msg)
        {
            LogRaw(LogType.Fatal, msg);
            Environment.Exit(-1);
        }
    }
}
