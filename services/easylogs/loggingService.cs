using System;
using System.IO;
using System.Collections.Generic;
using EasyLogs;
using EasySave.Models;

namespace EasySave.Services.Logging
{
    // This service handles logging operations based on the configured log type (JSON, XML, TXT)
    public class loggingService
    {
        // Constants for log folder and default log name
        private const string LogFolder = "logs/";
        private const string LogName = "logs";

        // Type of log format to use (JSON, XML, TXT)
        private EasyLogs.LogType logType;

        // Logger instance used to write log entries
        private ILogger logger;

        // Constructor initializes the logger based on the configuration string (log type)
        public loggingService(string configLogType)
        {
            // Convert string config to corresponding enum value
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
            }

            // Create the logs directory if it doesn't exist
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }

            // Instantiate the logger using a factory based on the selected log type
            logger = LoggerFactory.CreateLogger(LogFolder, this.logType);
        }

        // Logs a dictionary of data using the configured logger
        public void Log(Dictionary<string, string> data)
        {
            logger.Log(data);
        }




    }
}
