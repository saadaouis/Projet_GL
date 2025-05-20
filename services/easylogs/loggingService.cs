using System;
using System.IO;
using System.Collections.Generic;
using EasyLogs;
using EasySave.Models;
namespace EasySave.Services.Logging
{
    public class loggingService
    {
        private const string LogFolder = "logs/";
        private const string LogName = "logs";
        private EasyLogs.LogType logType;

        private ILogger logger;
        public loggingService(string configLogType)
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
                    
            }

            // Créer un dossier logs s'il n'existe pas
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }

            logger = LoggerFactory.CreateLogger(LogFolder, this.logType);
        }

        public void Log(Dictionary<string, string> data)
        {
            logger.Log(data);
        }
    }   
}