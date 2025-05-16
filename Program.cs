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
                var logger = new Logger();

                // Exemple d'entrée JSON : démarrage de l'application
                logger.LogJson(
                    name: "System",
                    fileSource: "N/A",
                    fileTarget: "N/A",
                    fileSize: 0,
                    fileTransferTime: 0
                );

                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);

                // Exemple d'entrée JSON : fin de l'application
                logger.LogJson(
                    name: "System",
                    fileSource: "N/A",
                    fileTarget: "N/A",
                    fileSize: 0,
                    fileTransferTime: 0
                );
            }
            catch (Exception ex)
            {
                var logger = new Logger();

                logger.LogJson(
                    name: "Exception",
                    fileSource: "Startup",
                    fileTarget: "N/A",
                    fileSize: 0,
                    fileTransferTime: 0
                );

                Console.WriteLine("Erreur au lancement : " + ex.Message);
                throw;
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
