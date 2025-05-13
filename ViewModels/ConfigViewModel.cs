using System;
using System.Threading.Tasks;
using EasySave.Models;
using EasySave.Services.Logger;
using EasySave.Services.Translation;

namespace EasySave.ViewModels
{
    public class ConfigViewModel : ViewModelBase
    {
        private readonly ModelConfig modelConfig;
        private readonly ILogger logger;
        private readonly TranslationService translationService;
        private Config currentConfig;

        public Config CurrentConfig
        {
            get => currentConfig;
            set => SetProperty(ref currentConfig, value);
        }

        public ConfigViewModel(
            ModelConfig modelConfig,
            ILogger logger,
            TranslationService translationService)
        {
            this.modelConfig = modelConfig;
            this.logger = logger;
            this.translationService = translationService;
        }

        public async Task LoadConfigAsync()
        {
            try
            {
                CurrentConfig = await modelConfig.LoadConfigAsync();
                logger.Log(LogLevel.Info, "Configuration loaded successfully");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to load configuration: {ex.Message}");
                throw;
            }
        }

        public async Task ModifyConfigAsync()
        {
            try
            {
                logger.Log(LogLevel.Info, "Starting configuration modification");
                
                var newConfig = await modelConfig.ModifyConfigAsync(CurrentConfig);
                if (newConfig != null)
                {
                    CurrentConfig = newConfig;
                    await modelConfig.SaveConfigAsync(CurrentConfig);
                    logger.Log(LogLevel.Info, "Configuration modified and saved successfully");
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Error modifying configuration: {ex.Message}");
                throw;
            }
        }

        public async Task SaveConfigAsync()
        {
            try
            {
                await modelConfig.SaveConfigAsync(CurrentConfig);
                logger.Log(LogLevel.Info, "Configuration saved successfully");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to save configuration: {ex.Message}");
                throw;
            }
        }
    }
} 