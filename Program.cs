// <copyright file="Program.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using EasySave.Controllers;
using EasySave.Models;
using EasySave.Services.Logging;
using EasySave.Services.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

            // First, configure the logging service
            services.AddSingleton<LoggingService>(sp => new LoggingService("json")); // Default to JSON initially

            // Build initial service provider for logging
            serviceProvider = services.BuildServiceProvider();
            loggingService = serviceProvider.GetRequiredService<LoggingService>();

            // Now configure the rest of the services
            services.AddSingleton<ModelConfig>();
            services.AddSingleton<ModelBackup>();
            services.AddSingleton<StateService>();

            // Rebuild the service provider with all services
            serviceProvider = services.BuildServiceProvider();

            // Load configuration
            var modelConfig = serviceProvider.GetRequiredService<ModelConfig>();
            if (modelConfig.Load())
            {
                // Update logging service with configured log type
                var configuredLogType = modelConfig.LogType ?? "json";
                services.RemoveAll<LoggingService>();
                services.AddSingleton<LoggingService>(sp => new LoggingService(configuredLogType));
                serviceProvider = services.BuildServiceProvider();
                loggingService = serviceProvider.GetRequiredService<LoggingService>();
            }

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
