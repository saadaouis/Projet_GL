using System;
using System.IO;
using System.Collections.Generic;
using EasyLogs;

namespace EasySave.services
{
    public class loggingService
    {
        private const string LogFolder = "json/";
        private const string LogName = "logs";

        private ILogger logger;
        public loggingService()
        {
            // Créer un dossier logs s'il n'existe pas
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }

            logger = LoggerFactory.CreateLogger(LogFolder, LogType.JSON);
        }

        public void Log(Dictionary<string, string> data)
        {
            logger.Log(data);
        }
    }   
}