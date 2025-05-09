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
                modelConfig.LoadConfig();
            }
        }
    }
}
