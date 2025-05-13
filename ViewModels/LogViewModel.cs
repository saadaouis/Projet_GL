using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasySave.Services.Logger;
using EasySave.Services.Translation;

namespace EasySave.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private readonly ILogger logger;
        private readonly TranslationService translationService;
        private List<LogEntry> logEntries;

        public List<LogEntry> LogEntries
        {
            get => logEntries;
            set => SetProperty(ref logEntries, value);
        }

        public LogViewModel(
            ILogger logger,
            TranslationService translationService)
        {
            this.logger = logger;
            this.translationService = translationService;
            logEntries = new List<LogEntry>();
        }

        public async Task ShowLogsAsync()
        {
            try
            {
                logger.Log(LogLevel.Info, "Displaying logs");
                LogEntries = await logger.GetLogsAsync();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Error displaying logs: {ex.Message}");
                throw;
            }
        }

        public async Task ClearLogsAsync()
        {
            try
            {
                await logger.ClearLogsAsync();
                LogEntries.Clear();
                logger.Log(LogLevel.Info, "Logs cleared successfully");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Error clearing logs: {ex.Message}");
                throw;
            }
        }
    }
} 