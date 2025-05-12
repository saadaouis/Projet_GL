// <copyright file="iLogger.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

namespace EasySave.Services.Logger
{
    /// <summary>
    /// Interface for logging functionality.
    /// </summary>
    public interface ILogger
    {
                /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether the logger is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Logs a message with the specified severity level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="severity">The severity level of the message.</param>
        void Log(string message, string severity);
    }
}