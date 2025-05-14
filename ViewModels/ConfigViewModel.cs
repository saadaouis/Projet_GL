// <copyright file="ConfigViewModel.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using EasySave.Models;

using EasySave.Services.Translation;

namespace EasySave.ViewModels
{
    /// <summary>
    /// View model for managing configuration settings.
    /// </summary>
    public class ConfigViewModel : ViewModelBase
    {
        private readonly ModelConfig modelConfig;
        private readonly TranslationService translationService;
        private ModelConfig.Config currentConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigViewModel"/> class.
        /// </summary>
        /// <param name="translationService">The translation service instance.</param>
        public ConfigViewModel(
            TranslationService translationService)
        {
            this.modelConfig = new ModelConfig();
            this.translationService = translationService;
            this.currentConfig = new ModelConfig.Config();
        }

        /// <summary>
        /// Gets or sets the current configuration.
        /// </summary>
        public ModelConfig.Config CurrentConfig
        {
            get => this.currentConfig;
            set => this.SetProperty(ref this.currentConfig, value);
        }

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                this.CurrentConfig = this.modelConfig.Load();
                Console.WriteLine("Configuration loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load configuration: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Saves or overrides the configuration.
        /// </summary>
        /// <returns>True if the configuration was saved successfully, false otherwise.</returns>
        public bool SaveOrOverrideConfig()
        {
            try
            {
                Console.WriteLine("Starting configuration modification");

                var newConfig = this.modelConfig.SaveOrOverride(this.CurrentConfig);
                if (newConfig != null)
                {
                    this.CurrentConfig = newConfig;
                    this.modelConfig.SaveOrOverride(this.CurrentConfig);
                    Console.WriteLine("Configuration modified and saved successfully");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error modifying configuration: {ex.Message}");
                return false;
            }
        }
    }
}