// <copyright file="controller.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Models;
using EasySave.Services.Logger;
using EasySave.Views;

namespace EasySave.Controllers
{
    /// <summary>
    /// Controller class that manages the application flow and user interactions.
    /// </summary>
    public class Controller
    {
        private readonly ILogger fileLogger;
        private readonly ILogger consoleLogger;
        private bool isRunning;
        private List<ModelBackup.Project> projects;
        private ModelBackup? modelBackup;
        private ModelConfig modelConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="Controller"/> class.
        /// </summary>
        public Controller()
        {
            this.isRunning = false;
            this.projects = new List<ModelBackup.Project>();
            this.modelConfig = new ModelConfig();
            this.fileLogger = new FileLogger();
            this.consoleLogger = new ConsoleLogger { IsEnabled = false }; // Disabled by default

            // Log initialization
            this.fileLogger.Log("Controller initialized", "info");
        }

        /// <summary>
        /// Starts the controller.
        /// </summary>
        public void Initialization()
        {
            this.isRunning = true;
            this.fileLogger.Log("Starting application", "info");
            View view = new View();
            View.ClearConsole();

            if (this.modelConfig.Load())
            {
                this.fileLogger.Log("Config loaded", "info");
                View.ShowMessage("Config loaded", "info");
                if (!string.IsNullOrEmpty(this.modelConfig.Source) && !string.IsNullOrEmpty(this.modelConfig.Destination))
                {
                    view.ChangeLanguage(this.modelConfig.Language!);
                    this.fileLogger.Log($"Initializing ModelBackup with source: {this.modelConfig.Source} and destination: {this.modelConfig.Destination}", "info");
                    this.modelBackup = new ModelBackup(this.modelConfig.Source, this.modelConfig.Destination, this.fileLogger);
                    this.projects = this.modelBackup.FetchProjects();
                }
            }
            else
            {
                this.fileLogger.Log("No config found", "error");
                View.ShowMessage("No config found", "error");
                Dictionary<string, string> config = view.InitializeForm();
                if (this.modelConfig.SaveOrOverride(config))
                {
                    this.fileLogger.Log("Config saved", "info");
                    View.ShowMessage("Config saved", "info");
                    if (this.modelConfig.Load())
                    {
                        this.fileLogger.Log("Config loaded", "info");
                        View.ShowMessage("Config loaded", "info");
                        if (!string.IsNullOrEmpty(this.modelConfig.Source) && !string.IsNullOrEmpty(this.modelConfig.Destination))
                        {
                            view.ChangeLanguage(this.modelConfig.Language!);
                            this.fileLogger.Log($"Initializing ModelBackup with source: {this.modelConfig.Source} and destination: {this.modelConfig.Destination}", "info");
                            this.modelBackup = new ModelBackup(this.modelConfig.Source, this.modelConfig.Destination, this.fileLogger);
                            this.projects = this.modelBackup.FetchProjects();
                        }
                    }
                    else
                    {
                        this.fileLogger.Log("Failed to load config", "error");
                        View.ShowMessage("Failed to load config", "error");
                    }
                }
                else
                {
                    this.fileLogger.Log("Failed to save config", "error");
                    View.ShowMessage("Failed to save config", "error");
                }
            }

            while (this.isRunning)
            {
                int choice = view.ShowMenu();
                switch (choice)
                {
                    case 1:
                        View.ClearConsole();
                        this.SaveProject();
                        break;
                    case 2:
                        View.ClearConsole();
                        this.ModifyConfig();
                        break;
                    case 3:
                        View.ClearConsole();
                        this.ToggleConsoleLogging();
                        break;
                    case 4:
                        View.ShowMessage("Exit", "info");
                        this.isRunning = false;
                        break;
                    default:
                        View.ClearConsole();
                        View.ShowMessage("Invalid choice", "error");
                        break;
                }
            }
        }

        /// <summary>
        /// Toggles console logging on/off.
        /// </summary>
        private void ToggleConsoleLogging()
        {
            this.consoleLogger.Log("Toggle Console Logging", "info"); // This will display all logs from the file
        }

        /// <summary>
        /// Logs a message to all enabled loggers.
        /// </summary>
        private void LogMessage(string message, string severity)
        {
            this.fileLogger.Log(message, severity);
            if (this.consoleLogger.IsEnabled)
            {
                this.consoleLogger.Log(message, severity);
            }
        }

        /// <summary>
        /// Allows the user to modify the current configuration.
        /// </summary>
        private void ModifyConfig()
        {
            View.ShowMessage("Current Configuration:", "info");
            View.ShowMessage($"source: {this.modelConfig.Source}", "info");
            View.ShowMessage($"destination: {this.modelConfig.Destination}", "info");
            View.ShowMessage($"language: {this.modelConfig.Language}", "info");
            View.ShowMessage(string.Empty, "info");

            string newSource = View.ConsoleReadLine("Enter new source path (leave empty to keep current): ") ?? string.Empty;
            string newDestination = View.ConsoleReadLine("Enter new destination path (leave empty to keep current): ") ?? string.Empty;
            string newLanguage = View.ConsoleReadLine("Enter new language (leave empty to keep current): ") ?? string.Empty;

            string updatedSource = string.IsNullOrWhiteSpace(newSource) ? this.modelConfig.Source! : newSource;
            string updatedDestination = string.IsNullOrWhiteSpace(newDestination) ? this.modelConfig.Destination! : newDestination;
            string updatedLanguage = string.IsNullOrWhiteSpace(newLanguage) ? this.modelConfig.Language! : newLanguage;

            var newConfig = new Dictionary<string, string>
            {
                { "Source", updatedSource },
                { "Destination", updatedDestination },
                { "Language", updatedLanguage },
            };

            if (this.modelConfig.SaveOrOverride(newConfig))
            {
                View.ShowMessage("Configuration updated successfully.", "info");

                if (this.modelConfig.Load())
                {
                    this.projects = this.modelBackup!.FetchProjects();
                    View.ShowMessage("Projects reloaded with new configuration.", "info");
                }
                else
                {
                    View.ShowMessage("Failed to reload configuration.", "error");
                }
            }
            else
            {
                View.ShowMessage("Failed to update configuration.", "error");
            }
        }

        private void SaveProject()
        {
            View.ShowMessage("Save project", "info");
            var projects = this.modelBackup!.FetchProjects("source");
            if (projects.Count == 0)
            {
                View.ShowMessage("No source projects found.", "warning");
                return;
            }

            List<int> selectedIndices = View.ShowMultipleProjectList(projects);
            foreach (int index in selectedIndices)
            {
                string projectName = projects[index].Name;
                var state = this.modelBackup.GetBackupState(projectName);
                state.StateChanged += (sender, state) => View.ShowBackupProgress(state);

                bool success = this.modelBackup.SaveProject(projectName);
                if (!success)
                {
                    View.ShowMessage($"Failed to save backup for project: {projectName}", "error");
                }
            }
        }
    }
}
