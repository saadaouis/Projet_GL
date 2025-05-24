// <copyright file="modelConfig.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Services.Logging;

namespace EasySave.Models
{
    /// <summary> Configuration model class for handling application settings.</summary>
    public class ModelConfig
    {
        private readonly string configPath;
        private readonly LoggingService loggingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelConfig"/> class.
        /// </summary>
        public ModelConfig()
        {
            this.configPath = "config/config.json";
            this.loggingService = Program.ServiceExtensions.GetService<LoggingService>();
        }

        /// <summary>Gets or sets the source directory path.</summary>
        [JsonPropertyName("Source")]
        public string? Source { get; set; } = string.Empty;

        /// <summary>Gets or sets the destination directory path.</summary>
        [JsonPropertyName("Destination")]
        public string? Destination { get; set; } = string.Empty;

        /// <summary>Gets or sets the application language.</summary>
        [JsonPropertyName("Language")]
        public string? Language { get; set; } = "En";

        /// <summary>Gets or sets the log type.</summary>
        [JsonPropertyName("LogType")]
        public string? LogType { get; set; } = "json";

        /// <summary>Loads the configuration from the config file.</summary>
        /// <returns>True if the configuration was loaded successfully, false otherwise.</returns>
        public bool Load()
        {
            try
            {
                if (!File.Exists(this.configPath))
                {
                    this.loggingService.Log(new Dictionary<string, string>
                    {
                        { "Operation", "ConfigLoad" },
                        { "Status", "Failed" },
                        { "Reason", "Config file not found" },
                        { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                    });
                    return false;
                }

                string jsonString = File.ReadAllText(this.configPath);
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

                if (config != null)
                {
                    this.Source = config.GetValueOrDefault("Source");
                    this.Destination = config.GetValueOrDefault("Destination");
                    this.Language = config.GetValueOrDefault("Language");
                    this.LogType = config.GetValueOrDefault("LogType");

                    this.loggingService.Log(new Dictionary<string, string>
                    {
                        { "Operation", "ConfigLoad" },
                        { "Status", "Success" },
                        { "Source", this.Source ?? "Not set" },
                        { "Destination", this.Destination ?? "Not set" },
                        { "Language", this.Language ?? "En" },
                        { "LogType", this.LogType ?? "json" },
                        { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                    });
                    return true;
                }

                this.loggingService.Log(new Dictionary<string, string>
                {
                    { "Operation", "ConfigLoad" },
                    { "Status", "Failed" },
                    { "Reason", "Invalid config format" },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                });
                return false;
            }
            catch (Exception ex)
            {
                this.loggingService.Log(new Dictionary<string, string>
                {
                    { "Operation", "ConfigLoad" },
                    { "Status", "Failed" },
                    { "Reason", ex.Message },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                });
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>Saves the configuration to the config file.</summary>
        /// <param name="config">The configuration to save.</param>
        /// <returns>True if the configuration was saved successfully, false otherwise.</returns>
        public bool SaveOrOverride(Dictionary<string, string> config)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true,
                });

                File.WriteAllText(this.configPath, jsonString);

                this.loggingService.Log(new Dictionary<string, string>
                {
                    { "Operation", "ConfigSave" },
                    { "Status", "Success" },
                    { "Source", config.GetValueOrDefault("Source", "Not set") },
                    { "Destination", config.GetValueOrDefault("Destination", "Not set") },
                    { "Language", config.GetValueOrDefault("Language", "En") },
                    { "LogType", config.GetValueOrDefault("LogType", "json") },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                });
                return true;
            }
            catch (Exception ex)
            {
                this.loggingService.Log(new Dictionary<string, string>
                {
                    { "Operation", "ConfigSave" },
                    { "Status", "Failed" },
                    { "Reason", ex.Message },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                });
                Console.WriteLine($"Error saving configuration: {ex.Message}");
                return false;
            }
        }
    }
}