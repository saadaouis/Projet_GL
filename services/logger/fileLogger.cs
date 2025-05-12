// <copyright file="fileLogger.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System.Text.Json;

namespace EasySave.Services.Logger
{
    /// <summary>
    /// File logger class that writes logs in JSON format.
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string logFilePath;
        private bool isEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogger"/> class.
        /// </summary>
        public FileLogger()
        {
            this.logFilePath = "log.json";
            this.isEnabled = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether the logger is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.isEnabled = value;
        }

        /// <summary>
        /// Logs a message to the file in JSON format.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="severity">The severity level of the message.</param>
        public void Log(string message, string severity)
        {
            if (!this.isEnabled)
            {
                return;
            }

            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Severity = severity,
                Message = message,
            };

            try
            {
                List<LogEntry> entries = new List<LogEntry>();

                // Read existing entries if file exists
                if (File.Exists(this.logFilePath))
                {
                    string existingJson = File.ReadAllText(this.logFilePath);
                    var existingEntries = JsonSerializer.Deserialize<List<LogEntry>>(existingJson);
                    if (existingEntries != null)
                    {
                        entries.AddRange(existingEntries);
                    }
                }

                // Add new entry
                entries.Add(logEntry);

                // Write all entries back to file
                string jsonString = JsonSerializer.Serialize(entries, new JsonSerializerOptions
                {
                    WriteIndented = true,
                });
                File.WriteAllText(this.logFilePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
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
