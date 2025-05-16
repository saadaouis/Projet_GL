using System;
using System.IO;
using System.Text.Json;

namespace EasySave.Logging
{
    public class Logger
    {
        private string logFilePath;

        public Logger()
        {
            logFilePath = Path.Combine(AppContext.BaseDirectory, "easysave_logs.json");
        }

        public void SetLogFilePath(string path)
        {
            logFilePath = path;
        }

        public void LogJson(string name, string fileSource, string fileTarget, long fileSize, double fileTransferTime)
        {
            var logEntry = new
            {
                Name = name,
                FileSource = fileSource,
                FileTarget = fileTarget,
                FileSize = fileSize,
                FileTransferTime = fileTransferTime,
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };

            try
            {
                string json = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });

                // Append comma if needed to maintain a valid JSON array
                if (!File.Exists(logFilePath))
                {
                    File.WriteAllText(logFilePath, "[\n" + json + "\n]");
                }
                else
                {
                    var content = File.ReadAllText(logFilePath).TrimEnd();
                    if (content.EndsWith("]"))
                    {
                        content = content.Substring(0, content.Length - 1); // Remove closing ]
                        content += ",\n" + json + "\n]";
                        File.WriteAllText(logFilePath, content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'écriture du log JSON : " + ex.Message);
            }
        }
    }
}
