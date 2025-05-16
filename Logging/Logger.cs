using System;
using System.IO;

namespace EasySave.Logging
{
    public class Logger
    {
        private string logFilePath;

        public Logger()
        {
            logFilePath = Path.Combine(AppContext.BaseDirectory, "easysave.log");
        }

        public void SetLogFilePath(string path)
        {
            logFilePath = path;
        }

        public void Log(string message)
        {
            try
            {
                var fullMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(logFilePath, fullMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'écriture dans le fichier log : " + ex.Message);
            }
        }
    }
}