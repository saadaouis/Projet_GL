using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Logging
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public string FileSource { get; set; }
        public string FileTarget { get; set; }
        public long FileSize { get; set; }
        public long FileTransferTime { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class Logger
    {
        private readonly string logFilePath = "easysave_log.json";

        public void LogJson(string name, string fileSource, string fileTarget, long fileSize, long fileTransferTime, string status = "OK", string message = "")
        {
            List<LogEntry> logs = new List<LogEntry>();

            if (File.Exists(logFilePath))
            {
                string existingJson = File.ReadAllText(logFilePath);
                if (!string.IsNullOrWhiteSpace(existingJson))
                {
                    logs = JsonSerializer.Deserialize<List<LogEntry>>(existingJson) ?? new List<LogEntry>();
                }
            }

            logs.Add(new LogEntry
            {
                Timestamp = DateTime.Now,
                Name = name,
                FileSource = fileSource,
                FileTarget = fileTarget,
                FileSize = fileSize,
                FileTransferTime = fileTransferTime,
                Status = status,
                Message = message
            });

            var options = new JsonSerializerOptions { WriteIndented = true };
            string newJson = JsonSerializer.Serialize(logs, options);
            File.WriteAllText(logFilePath, newJson);
        }
    }
}
