// <copyright file="consoleLogger.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;

namespace EasySave.Services.Logger
{
    /// <summary>
    /// Console logger class that displays logs with color-coded severity levels.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private bool isEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        public ConsoleLogger()
        {
            this.isEnabled = true;
        }

        /// <summary>
        /// Gets or sets whether the logger is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.isEnabled = value;
        }

        /// <summary>
        /// Logs a message to the console with color-coded severity.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="severity">The severity level of the message.</param>
        public void Log(string message, string severity)
        {
            if (!this.isEnabled) return;

            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = GetColorForSeverity(severity);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"[{timestamp}] [{severity.ToUpper()}] {message}");

            Console.ForegroundColor = originalColor;
        }

        private static ConsoleColor GetColorForSeverity(string severity)
        {
            return severity.ToLower() switch
            {
                "error" => ConsoleColor.Red,
                "warning" => ConsoleColor.Yellow,
                "info" => ConsoleColor.Cyan,
                _ => ConsoleColor.White
            };
        }
    }
}
