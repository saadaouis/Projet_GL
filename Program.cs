// <copyright file="Program.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using EasySave.Controllers;
using EasySave.Models;
using EasySave.Services.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave
{
    /// <summary>
    /// Main class.
    /// </summary>
    internal class Program
    {
        private static IServiceProvider? serviceProvider;
        private static LoggingService? loggingService;

        /// <summary>
        /// Main method.
        /// </summary>
        private static void Main(string[] args)
        {
            var services = new ServiceCollection();

            // Configure services
            services.AddSingleton<ModelConfig>();
            services.AddSingleton<ModelBackup>();

            // Build the service provider
            serviceProvider = services.BuildServiceProvider();
            if (serviceProvider == null)
            {
                Console.WriteLine("Error: Service provider is null");
                return;
            }

            ModelConfig modelConfig = serviceProvider.GetRequiredService<ModelConfig>();
            modelConfig.Load();

            // Configure LoggingService with the log type from config
            services.AddSingleton<LoggingService>(sp => new LoggingService(modelConfig.LogType ?? "json"));

            // Rebuild the service provider with the new LoggingService
            serviceProvider = services.BuildServiceProvider();

            // Get the logging service instance
            loggingService = serviceProvider.GetRequiredService<LoggingService>();

            Controller controller = new Controller();
            controller.Initialization();
            Console.WriteLine("Shutting down...");
        }

        /// <summary>
        /// Service extensions.
        /// </summary>
        public static class ServiceExtensions
        {
            /// <summary>
            /// Get a service from the service provider.
            /// </summary>
            /// <typeparam name="T">The type of the service to get.</typeparam>
            /// <returns>The service instance.</returns>
            public static T GetService<T>()
            where T : class
            {
                return serviceProvider!.GetRequiredService<T>();
            }
        }
    }
}
