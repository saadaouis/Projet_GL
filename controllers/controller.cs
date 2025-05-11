// <copyright file="controller.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
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
        private ModelBackup modelBackup;
        private ModelConfig modelConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="Controller"/> class.
        /// </summary>
        public Controller()
        {
            this.isRunning = false;
            this.projects = new List<ModelBackup.Project>();
            this.modelBackup = new ModelBackup();
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
                    this.projects = this.modelBackup.FetchProjects(this.modelConfig.Source, this.modelConfig.Destination);
                }
            }
            else
            {
                View.ShowMessage("No config found", "error");
                Dictionary<string, string> config = view.InitializeForm();
                if (this.modelConfig.Save(config))
                {
                    View.ShowMessage("Config saved", "info");
                    if (this.modelConfig.Load())
                    {
                        View.ShowMessage("Config loaded", "info");
                        if (!string.IsNullOrEmpty(this.modelConfig.Source) && !string.IsNullOrEmpty(this.modelConfig.Destination))
                        {
                            this.projects = this.modelBackup.FetchProjects(this.modelConfig.Source, this.modelConfig.Destination);
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
                        View.ShowMessage("Download backup", "info");
                        Console.WriteLine($"Number of projects: {this.projects.Count}");
                        foreach (var project in this.projects)
                        {
                            Console.WriteLine($"Project: {project.Name}");
                            Console.WriteLine($"Last Backup: {project.LastBackup}");
                            Console.WriteLine($"Size: {project.Size} MB");
                            Console.WriteLine("---");
                        }

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

            string newSource = View.ConsoleWriteLine("Enter new source path (leave empty to keep current): ", "info") ?? string.Empty;
            string newDestination = View.ConsoleWriteLine("Enter new destination path (leave empty to keep current): ", "info") ?? string.Empty;
            string newLanguage = View.ConsoleWriteLine("Enter new language (leave empty to keep current): ", "info") ?? string.Empty;

            string? updatedSource = string.IsNullOrWhiteSpace(newSource) ? this.modelConfig.Source : newSource;
            string? updatedDestination = string.IsNullOrWhiteSpace(newDestination) ? this.modelConfig.Destination : newDestination;
            string? updatedLanguage = string.IsNullOrWhiteSpace(newLanguage) ? this.modelConfig.Language : newLanguage;

            var newConfig = new Dictionary<string, string>
            {
                { "source", updatedSource ?? string.Empty },
                { "destination", updatedDestination ?? string.Empty },
                { "language", updatedLanguage ?? "En" },
            };

            if (this.modelConfig.Save(newConfig))
            {
                View.ShowMessage("Configuration updated successfully.", "info");

                if (this.modelConfig.Load())
                {
                    this.projects = this.modelBackup.FetchProjects(this.modelConfig.Source ?? string.Empty, this.modelConfig.Destination ?? string.Empty);
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
    }
}
