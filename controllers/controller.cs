// <copyright file="controller.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Models;
using EasySave.Views;
using EasySave.Services.Logging;

namespace EasySave.Controllers
{
    /// <summary>
    /// Controller class that manages the application flow and user interactions.
    /// </summary>
    public class Controller
    {
        private bool isRunning;
        private List<ModelBackup.Project> projects;
        private ModelBackup? modelBackup;
        private ModelConfig modelConfig;
        private readonly LoggingService loggingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Controller"/> class.
        /// </summary>
        public Controller()
        {
            this.isRunning = false;
            this.projects = new List<ModelBackup.Project>();
            this.modelConfig = new ModelConfig();
            this.loggingService = Program.ServiceExtensions.GetService<LoggingService>();
        }

        /// <summary>
        /// Starts the controller.
        /// </summary>
        public void Initialization()
        {
            this.isRunning = true;
            View view = new View();
            View.ClearConsole();

            this.loggingService.Log(new Dictionary<string, string>
            {
                { "Operation", "ApplicationStart" },
                { "Status", "Success" },
                { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
            });

            if (this.modelConfig.Load())
            {
                View.ShowMessage("Config loaded", "info");
                if (!string.IsNullOrEmpty(this.modelConfig.Source) && !string.IsNullOrEmpty(this.modelConfig.Destination))
                {
                    view.ChangeLanguage(this.modelConfig.Language!);

                    this.modelBackup = new ModelBackup(this.modelConfig.Source, this.modelConfig.Destination);
                    this.projects = this.modelBackup.FetchProjects();

                    this.loggingService.Log(new Dictionary<string, string>
                    {
                        { "Operation", "InitialConfigLoad" },
                        { "Status", "Success" },
                        { "Source", this.modelConfig.Source },
                        { "Destination", this.modelConfig.Destination },
                        { "Language", this.modelConfig.Language ?? "En" },
                        { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                    });
                }
            }
            else
            {
                View.ShowMessage("No config found", "error");
                Dictionary<string, string> config = view.InitializeForm();
                if (this.modelConfig.SaveOrOverride(config))
                {
                    View.ShowMessage("Config saved", "info");
                    if (this.modelConfig.Load())
                    {
                        View.ShowMessage("Config loaded", "info");
                        if (!string.IsNullOrEmpty(this.modelConfig.Source) && !string.IsNullOrEmpty(this.modelConfig.Destination))
                        {
                            view.ChangeLanguage(this.modelConfig.Language!);
                            this.modelBackup = new ModelBackup(this.modelConfig.Source, this.modelConfig.Destination);
                            this.projects = this.modelBackup.FetchProjects();

                            this.loggingService.Log(new Dictionary<string, string>
                            {
                                { "Operation", "NewConfigCreated" },
                                { "Status", "Success" },
                                { "Source", this.modelConfig.Source },
                                { "Destination", this.modelConfig.Destination },
                                { "Language", this.modelConfig.Language ?? "En" },
                                { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                            });
                        }
                    }
                    else
                    {
                        View.ShowMessage("Failed to load config", "error");
                        this.loggingService.Log(new Dictionary<string, string>
                        {
                            { "Operation", "NewConfigLoad" },
                            { "Status", "Failed" },
                            { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                        });
                    }
                }
                else
                {
                    View.ShowMessage("Failed to save config", "error");
                    this.loggingService.Log(new Dictionary<string, string>
                    {
                        { "Operation", "NewConfigSave" },
                        { "Status", "Failed" },
                        { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                    });
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

                    this.loggingService.Log(new Dictionary<string, string>
                    {
                        { "Operation", "ConfigUpdate" },
                        { "Status", "Success" },
                        { "OldSource", this.modelConfig.Source! },
                        { "NewSource", updatedSource },
                        { "OldDestination", this.modelConfig.Destination! },
                        { "NewDestination", updatedDestination },
                        { "OldLanguage", this.modelConfig.Language! },
                        { "NewLanguage", updatedLanguage },
                        { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                    });
                }
                else
                {
                    View.ShowMessage("Failed to reload configuration.", "error");
                    this.loggingService.Log(new Dictionary<string, string>
                    {
                        { "Operation", "ConfigReload" },
                        { "Status", "Failed" },
                        { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                    });
                }
            }
            else
            {
                View.ShowMessage("Failed to update configuration.", "error");
                this.loggingService.Log(new Dictionary<string, string>
                {
                    { "Operation", "ConfigUpdate" },
                    { "Status", "Failed" },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                });
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

            var isDifferentialBackup = View.ShowDifferentialBackup("Do you want to make a differential backup? (y/n)");
            View.ClearConsole();

            List<int> selectedIndices = View.ShowMultipleProjectList(projects);
            foreach (int index in selectedIndices)
            {
                string projectName = projects[index].Name;
                var state = this.modelBackup.GetBackupState(projectName);
                state.StateChanged += (sender, state) => View.ShowBackupProgress(state);

                bool success = this.modelBackup.SaveProject(projectName, isDifferentialBackup);
                if (!success)
                {
                    View.ShowMessage($"Failed to save backup for project: {projectName}", "error");
                }
            }
        }
    }
}
