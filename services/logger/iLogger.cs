using System;

namespace EasySave.Services.Logger
{
    /// <summary>
    /// Interface for logging functionality.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a message with the specified severity level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="severity">The severity level of the message.</param>
        void Log(string message, string severity);

        /// <summary>
        /// Gets or sets whether the logger is enabled.
        /// </summary>
        bool IsEnabled { get; set; }
    }
} 