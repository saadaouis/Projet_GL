using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace EasySave.Services.Logger
{
    /// <summary>
    /// Console logger class that displays logs from the log file.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private readonly string logFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        public ConsoleLogger()
        {
            this.logFilePath = "log.json";
        }

        /// <summary>
        /// Gets or sets whether the logger is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Displays all logs from the log file in the console.
        /// </summary>
        public void Log(string message, string severity)
        {
            try
            {
                if (File.Exists(this.logFilePath))
                {
                    string jsonString = File.ReadAllText(this.logFilePath);
                    var entries = JsonSerializer.Deserialize<List<LogEntry>>(jsonString);

                    if (entries != null)
                    {
                        Console.Clear();
                        Console.WriteLine("\n=== Log Entries ===\n");
                        
                        foreach (var entry in entries)
                        {
                            Console.WriteLine($"[{entry.Timestamp}] [{entry.Severity}] {entry.Message}");
                        }
                        
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading log file: {ex.Message}");
            }
        }

        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public string Severity { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }
    }
}
