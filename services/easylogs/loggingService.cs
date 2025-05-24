// <copyright file="loggingService.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using EasyLogs;

namespace EasySave.Services.Logging
{
    /// <summary>
    /// Logging service.
    /// </summary>
    public class LoggingService
    {
        private const string LogFolder = "logs/";
        private const string LogName = "logs";
        private EasyLogs.LogType logType;

        private ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingService"/> class.
        /// </summary>
        /// <param name="configLogType">The log type to use.</param>
        public LoggingService(string configLogType)
        {
            switch (configLogType)
            {
                case "json":
                    this.logType = EasyLogs.LogType.JSON;
                    break;
                case "xml":
                    this.logType = EasyLogs.LogType.XML;
                    break;
                case "txt":
                    this.logType = EasyLogs.LogType.TXT;
                    break;
                default:
                    this.logType = EasyLogs.LogType.JSON;
                    break;
            }

            // Créer un dossier logs s'il n'existe pas
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }

            this.logger = LoggerFactory.CreateLogger(LogFolder, this.logType);
        }

        /// <summary>
        /// Logs a message to the logger.
        /// </summary>
        /// <param name="data">The data to log.</param>
        public void Log(Dictionary<string, string> data)
        {
            this.logger.Log(data);
        }
    }
}