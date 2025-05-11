// <copyright file="modelConfig.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Models
{
    /// <summary> Configuration model class for handling application settings.</summary>
    public class ModelConfig
    {
        // Class variables
        private const string ConfigPath = "config/config.json";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>Gets or sets the source directory path.</summary>
        [JsonPropertyName("source")]
        public string? Source { get; set; } = string.Empty;

        /// <summary>Gets or sets the destination directory path.</summary>
        [JsonPropertyName("destination")]
        public string? Destination { get; set; } = string.Empty;

        /// <summary>Gets or sets the application language.</summary>
        [JsonPropertyName("language")]
        public string? Language { get; set; } = "En";

        /// <summary>Loads the configuration from the config file.</summary>
        /// <returns>The loaded configuration or null if the file doesn't exist or is invalid.</returns>
        public bool Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    return false;
                }

                string jsonString = File.ReadAllText(ConfigPath);
                var config = JsonSerializer.Deserialize<ModelConfig>(jsonString, JsonOptions);

                if (config == null)
                {
                    return false;
                }

                this.Source = config.Source;
                this.Destination = config.Destination;
                this.Language = config.Language;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Saves the configuration to the config file.</summary>
        /// <param name="config">The configuration to save.</param>
        /// <returns>True if the configuration was saved successfully, false otherwise.</returns>
        public bool Save(Dictionary<string, string> config)
        {
            try
            {
                // Ensure the directory exists
                string? directoryPath = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string jsonString = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(ConfigPath, jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}