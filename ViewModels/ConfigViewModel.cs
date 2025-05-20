// <copyright file="ConfigViewModel.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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
        private MainViewModel? mainViewModel; // To navigate back
        private ModelConfig.Config currentConfig;
        private ModelConfig.Config originalConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigViewModel"/> class.
        /// </summary>
        /// <param name="modelConfig">The model configuration instance.</param>
        /// <param name="translationService">The translation service instance.</param>
        public ConfigViewModel(ModelConfig modelConfig, TranslationService translationService)
            : base(translationService)
        {
            this.modelConfig = modelConfig;
            this.translationService = translationService;
            
            this.currentConfig = new ModelConfig.Config();
            this.originalConfig = new ModelConfig.Config();

            this.AvailableLanguages = new List<string> { "En", "Fr", "Gw" };
            this.AvailableLogTypes = new List<string> { "json", "xml", "txt" };
            this.SaveConfigCommand = new MainViewModel.AsyncRelayCommand(this.ExecuteSaveConfigAsync, this.CanExecuteSaveConfig);
            this.CancelCommand = new MainViewModel.RelayCommand(this.ExecuteCancel);
        }

        /// <summary>
        /// Gets or sets the current configuration being edited.
        /// </summary>
        public ModelConfig.Config CurrentConfig
        {
            get => this.currentConfig;
            set
            {
                if (this.SetProperty(ref this.currentConfig, value))
                {
                    this.originalConfig = this.CloneConfig(value);
                    ((MainViewModel.AsyncRelayCommand)this.SaveConfigCommand).RaiseCanExecuteChanged();
                }
            }
        }
        
        /// <summary>
        /// Gets the list of available languages for the UI.
        /// </summary>
        public List<string> AvailableLanguages { get; }

        /// <summary>
        /// Gets the list of available log types for the UI.
        /// </summary>
        public List<string> AvailableLogTypes { get; }

        /// <summary>
        /// Gets the command to save the current configuration.
        /// </summary>
        public ICommand SaveConfigCommand { get; }

        /// <summary>
        /// Gets the command to cancel configuration changes.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Sets the reference to the MainViewModel for navigation purposes.
        /// </summary>
        /// <param name="mainVm">The main view model instance.</param>
        public void SetMainViewModel(MainViewModel mainVm)
        {
            this.mainViewModel = mainVm;
        }

        private bool CanExecuteSaveConfig()
        {
            return this.CurrentConfig != null;
        }

        private async Task ExecuteSaveConfigAsync()
        {
            if (this.CurrentConfig == null)
            {
                return;
            }

            Console.WriteLine("Saving configuration...");
            string previousLanguage = this.originalConfig.Language ?? "en";

            this.modelConfig.SaveOrOverride(this.CurrentConfig);
            this.originalConfig = this.CloneConfig(this.CurrentConfig);

            Console.WriteLine("Configuration saved.");

            if (this.CurrentConfig.Language != previousLanguage)
            {
                Console.WriteLine($"Language changed from {previousLanguage} to {this.CurrentConfig.Language}. Updating TranslationService.");
                await this.translationService.SetLanguageAsync(this.CurrentConfig.Language ?? "en");
            }
            
            this.mainViewModel?.ChangePaths(this.CurrentConfig.Source, this.CurrentConfig.Destination);

            // Navigate back to the main/backup view
            this.mainViewModel?.NavigateToBackupView();
        }

        private void ExecuteCancel()
        {
            Console.WriteLine("Cancelling configuration changes.");
            this.CurrentConfig = this.CloneConfig(this.originalConfig);

            // Also navigate back on cancel
            this.mainViewModel?.NavigateToBackupView(); 
        }

        private ModelConfig.Config CloneConfig(ModelConfig.Config configToClone)
        {
            if (configToClone == null)
            {
                return new ModelConfig.Config();
            }

            return new ModelConfig.Config
            {
                Source = configToClone.Source,
                Destination = configToClone.Destination,
                Language = configToClone.Language,
            };
        }
    }
}