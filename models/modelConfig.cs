// <copyright file="modelConfig.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Models
{
    /// <summary> Configuration model class for handling application settings.</summary>
    public class ModelConfig
    {
        private readonly string configPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelConfig"/> class.
        /// </summary>
        public ModelConfig()
        {
            this.configPath = "json/config.json";
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

        /// <summary>Loads the configuration from the config file.</summary>
        /// <returns>True if the configuration was loaded successfully, false otherwise.</returns>
        public Config Load()
        {
            try
            {
                if (!File.Exists(this.configPath))
                {
                    Console.WriteLine("Config file not found");
                    return new Config();
                }

                string jsonString = File.ReadAllText(this.configPath);
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                Console.WriteLine($"Config file found: {this.configPath}");

                if (config != null)
                {
                    var configClass = new Config();
                    configClass.Source = config.GetValueOrDefault("Source");
                    configClass.Destination = config.GetValueOrDefault("Destination");
                    configClass.Language = config.GetValueOrDefault("Language");
                    Console.WriteLine("Configuration loaded successfully");
                    return configClass;
                }

                Console.WriteLine("Failed to deserialize config file");
                return new Config();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                return new Config();
            }
        }

        /// <summary>Saves the configuration to the config file.</summary>
        /// <param name="config">The configuration to save.</param>
        /// <returns>True if the configuration was saved successfully, false otherwise.</returns>
        public Config SaveOrOverride(Config config)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true,
                });

                File.WriteAllText(this.configPath, jsonString);
                Console.WriteLine("Configuration saved successfully");
                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
                return config;
            }
        }

        /// <summary>
        /// Config class for handling application settings.
        /// </summary>
        public class Config
        {
            /// <summary>Gets or sets the source directory path.</summary>
            public string? Source { get; set; } = string.Empty;

            /// <summary>Gets or sets the destination directory path.</summary>
            public string? Destination { get; set; } = string.Empty;

            /// <summary>Gets or sets the application language.</summary>
            public string? Language { get; set; } = "En";
        }
    }
}