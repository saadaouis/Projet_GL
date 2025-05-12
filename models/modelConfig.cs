// <copyright file="modelConfig.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Services.Logger;

namespace EasySave.Models
{
    /// <summary> Configuration model class for handling application settings.</summary>
    public class ModelConfig
    {
        private readonly string configPath;
        private readonly ILogger logger;

        /// <summary>Gets or sets the source directory path.</summary>
        [JsonPropertyName("Source")]
        public string? Source { get; set; } = string.Empty;

        /// <summary>Gets or sets the destination directory path.</summary>
        [JsonPropertyName("Destination")]
        public string? Destination { get; set; } = string.Empty;

        /// <summary>Gets or sets the application language.</summary>
        [JsonPropertyName("Language")]
        public string? Language { get; set; } = "En";

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelConfig"/> class.
        /// </summary>
        public ModelConfig()
        {
            this.configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            this.logger = new FileLogger();
        }

        /// <summary>Loads the configuration from the config file.</summary>
        /// <returns>True if the configuration was loaded successfully, false otherwise.</returns>
        public bool Load()
        {
            try
            {
                if (!File.Exists(this.configPath))
                {
                    this.logger.Log("Config file not found", "warning");
                    return false;
                }

                string jsonString = File.ReadAllText(this.configPath);
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

                if (config != null)
                {
                    this.Source = config.GetValueOrDefault("Source");
                    this.Destination = config.GetValueOrDefault("Destination");
                    this.Language = config.GetValueOrDefault("Language");
                    this.logger.Log("Configuration loaded successfully", "info");
                    return true;
                }

                this.logger.Log("Failed to deserialize config file", "error");
                return false;
            }
            catch (Exception ex)
            {
                this.logger.Log($"Error loading configuration: {ex.Message}", "error");
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
                    WriteIndented = true
                });

                File.WriteAllText(this.configPath, jsonString);
                this.logger.Log("Configuration saved successfully", "info");
                return true;
            }
            catch (Exception ex)
            {
                this.logger.Log($"Error saving configuration: {ex.Message}", "error");
                return false;
            }
        }
    }
}