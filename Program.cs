using System;
using System.IO;
using Avalonia;
using EasySave.Logging;

namespace EasySave
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                string logDirectory = @"C:\Logs";
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                var logger = new Logger();
                logger.SetLogFilePath(Path.Combine(logDirectory, "easysave.log"));
                logger.Log("Application EasySave démarrée.");

                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);

                logger.Log("Application EasySave terminée.");
            }
            catch (Exception ex)
            {
                string logDirectory = @"C:\Logs";
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                var logger = new Logger();
                logger.SetLogFilePath(Path.Combine(logDirectory, "easysave.log"));
                logger.Log("Exception lors du démarrage : " + ex.Message);
                throw;
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
