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
    /// Controller class.
    /// </summary>
    public class Controller
    {
        private bool isRunning = false;
        private List<ModelBackup.Project> projects = new();
        private ModelBackup modelBackup = new();
        private ModelConfig modelConfig = new();
        private View view = new();
        private ILogger logger = new ConsoleLogger();

        /// <summary>
        /// Starts the controller.
        /// </summary>
        public void Initialization()
        {
            this.isRunning = true;

            if (this.modelConfig.Load())
            {
                View.ShowMessage("Config loaded", "info");
                if (!string.IsNullOrEmpty(modelConfig.Source) && !string.IsNullOrEmpty(modelConfig.Destination))
                {
                    this.projects = this.modelBackup.FetchProjects(modelConfig.Source, modelConfig.Destination);
                }
            }
            else
            {
                View.ShowMessage("No config found", "error");
                Dictionary<string, string> config = view.InitializeForm();
                if (modelConfig.Save(config))
                {
                    View.ShowMessage("Config saved", "info");
                    if (modelConfig.Load())
                    {
                        View.ShowMessage("Config loaded", "info");
                        if (!string.IsNullOrEmpty(modelConfig.Source) && !string.IsNullOrEmpty(modelConfig.Destination))
                        {
                            this.projects = this.modelBackup.FetchProjects(modelConfig.Source, modelConfig.Destination);
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
                        // À compléter selon ta logique métier
                        break;

                    case 3:
                        View.ClearConsole();
                        View.ShowMessage("Toggle AutoSave", "info");
                        // À compléter selon ta logique métier
                        break;

                    case 4:
                       
                        ModifyConfig();
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
            Console.WriteLine("Current Configuration:");
            Console.WriteLine($"Source: {modelConfig.Source}");
            Console.WriteLine($"Destination: {modelConfig.Destination}");
            Console.WriteLine();

            Console.Write("Enter new source path (leave empty to keep current): ");
            string newSource = Console.ReadLine();
            Console.Write("Enter new destination path (leave empty to keep current): ");
            string newDestination = Console.ReadLine();

            string updatedSource = string.IsNullOrWhiteSpace(newSource) ? modelConfig.Source : newSource;
            string updatedDestination = string.IsNullOrWhiteSpace(newDestination) ? modelConfig.Destination : newDestination;

            var newConfig = new Dictionary<string, string>
            {
                { "Source", updatedSource },
                { "Destination", updatedDestination }
            };

            if (modelConfig.Save(newConfig))
            {
                View.ShowMessage("Configuration updated successfully.", "info");

                if (modelConfig.Load())
                {
                    this.projects = this.modelBackup.FetchProjects(modelConfig.Source, modelConfig.Destination);
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
