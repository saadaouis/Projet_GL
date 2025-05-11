// <copyright file="controller.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Controller"/> class.
        /// </summary>
        ///
        public Controller()
        {
        }

        /// <summary>
        /// Starts the controller.
            /// </summary>
        public void Initialization()
        {
            this.isRunning = true;
            ILogger logger = new ConsoleLogger();
            View view = new();
            ModelConfig modelConfig = new();
            ModelBackup modelBackup = new();

            while (this.isRunning)
            {
                if (modelConfig.Load())
                {
                    View.ShowMessage("Config loaded", "info");
                }
                else
                {
                    View.ShowMessage("No config found", "error");
                    Dictionary<string, string> config = view.InitializeForm();
                    if (modelConfig.Save(config))
                    {
                        View.ShowMessage("Config saved", "info");
                    }
                    else
                    {
                        View.ShowMessage("Failed to save config", "error");
                    }
                }

                this.isRunning = false;
            }
        }
    }
}
