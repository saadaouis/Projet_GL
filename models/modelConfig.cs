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
            PropertyNamingPolicy = null, // Use exact property names
        };

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
                var configFile = JsonSerializer.Deserialize<ModelConfig>(jsonString, JsonOptions);

                if (configFile == null)
                {
                    return false;
                }

                this.Source = configFile.Source;
                this.Destination = configFile.Destination;
                this.Language = configFile.Language;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
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
                // Ensure the directory exists
                string? directoryPath = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Update the current instance with the new values
                if (config.ContainsKey("Source"))
                {
                    this.Source = config["Source"];
                }

                if (config.ContainsKey("Destination"))
                {
                    this.Destination = config["Destination"];
                }

                if (config.ContainsKey("Language"))
                {
                    this.Language = config["Language"];
                }

                // Serialize the current instance
                string jsonString = JsonSerializer.Serialize(this, JsonOptions);
                File.WriteAllText(ConfigPath, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
    }
}