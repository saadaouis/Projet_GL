// <copyright file="controller.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        }

        /// <summary>
        /// Starts the controller and initializes the application.
        /// </summary>
        public void Initialization()
        {
            this.isRunning = true;
            ILogger logger = new ConsoleLogger();
            View view = new View();

            if (this.modelConfig.Load())
            {
                View.ShowMessage("Config loaded", "info");
                if (!string.IsNullOrEmpty(this.modelConfig.Source) && !string.IsNullOrEmpty(this.modelConfig.Destination))
                {
                    this.modelBackup = new ModelBackup(this.modelConfig.Source, this.modelConfig.Destination);
                    this.projects = this.modelBackup.FetchProjects();
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
                            this.modelBackup = new ModelBackup(this.modelConfig.Source, this.modelConfig.Destination);
                            this.projects = this.modelBackup.FetchProjects();
                        }
                    }
                    else
                    {
                        View.ShowMessage("Failed to load config", "error");
                    }
                }
                else
                {
                    View.ShowMessage("Failed to save config", "error");
                }
            }

            while (this.isRunning)
            {
                int choice = View.ShowMenu();
                switch (choice)
                {
                    case 1:
                        View.ClearConsole();
                        this.DownloadFunction();
                        break;
                    case 2:
                        View.ClearConsole();
                        View.ShowMessage("Save backup", "info");
                        break;
                    case 3:
                        View.ClearConsole();
                        View.ShowMessage("Toggle AutoSave", "info");
                        break;
                    case 4:
                        View.ClearConsole();
                        View.ShowMessage("Modify config", "info");
                        this.ModifyConfig();
                        break;
                    case 5:
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

        /// <summary>
        /// Allows the user to download a backup version.
        /// </summary>
        public void DownloadFunction()
        {
            this.projects = this.modelBackup!.FetchProjects();
            Console.WriteLine(this.projects);
            if (this.projects.Count == 0)
            {
                View.ShowMessage("No projects available.", "error");
                return;
            }

            int selectedIndex = View.ShowProjectList(this.projects);
            string selectedProject = this.projects[selectedIndex].Name;
            int projectNumber = int.Parse(selectedProject.Replace("Project", ""));

            var versionList = this.modelBackup.FetchVersions(projectNumber);
            if (versionList.Count == 0)
            {
                View.ShowMessage("No versions available.", "error");
                return;
            }

            int selectedVersion = View.ShowProjectVersion(versionList);
            View.ShowMessage("Downloading project...", "info");
            this.modelBackup.DownloadVersion(projectNumber, selectedVersion);
            View.ShowMessage("Download complete.", "info");
        }
    }
}
