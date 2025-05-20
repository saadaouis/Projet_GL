// <copyright file="modelConfig.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Models
{
    /// <summary> Configuration model class for handling application settings.</summary>
    public class ModelConfig
    {
        private readonly string configPath;
        private readonly JsonSerializerOptions serializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelConfig"/> class.
        /// </summary>
        public ModelConfig()
        {
            this.configPath = "json/config.json";
            this.serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
            };
            this.IsNewConfig = false;
            this.EnsureConfigDirectoryExists(); // Ensures directory exists, not the file itself
        }

        /// <summary>
        /// Gets a value indicating whether the configuration was newly created during the last Load operation 
        /// because the config file was not found or was invalid.
        /// </summary>
        public bool IsNewConfig { get; private set; }

        /// <summary>Loads the configuration from the config file.</summary>
        /// <returns>The loaded configuration object. Returns a new default Config if file not found or error occurs, and sets IsNewConfig accordingly.</returns>
        public Config Load()
        {
            this.IsNewConfig = false; // Assume existing config first
            try
            {
                if (!File.Exists(this.configPath))
                {
                    Console.WriteLine($"Config file not found at {this.configPath}. Will present config view for initial setup.");
                    this.IsNewConfig = true;
                    return new Config(); // Return a new default, unsaved config
                }

                string jsonString = File.ReadAllText(this.configPath);
                Config? loadedConfig = JsonSerializer.Deserialize<Config>(jsonString, this.serializerOptions);

                if (loadedConfig != null)
                {
                    Console.WriteLine($"Configuration loaded successfully from {this.configPath}.");

                    // Log details if needed, already done in previous version
                    return loadedConfig;
                }

                Console.WriteLine($"Warning: Failed to deserialize config file at {this.configPath}, or file was empty. Will present config view.");
                this.IsNewConfig = true;
                return new Config(); // Return a new default, unsaved config
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error deserializing configuration from {this.configPath}: {jsonEx.Message}. Will present config view.");
                this.IsNewConfig = true;
                return new Config(); // Return a new default, unsaved config
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration from {this.configPath}: {ex.Message}. Will present config view.");
                this.IsNewConfig = true;
                return new Config(); // Return a new default, unsaved config
            }
        }

        /// <summary>Saves or overrides the configuration to the config file.</summary>
        /// <param name="configToSave">The configuration to save.</param>
        /// <returns>The saved configuration object.</returns>
        public Config SaveOrOverride(Config configToSave)
        {
            try
            {
                this.EnsureConfigDirectoryExists();
                string jsonString = JsonSerializer.Serialize(configToSave, this.serializerOptions);
                File.WriteAllText(this.configPath, jsonString);
                Console.WriteLine($"Configuration saved successfully to {this.configPath}.");
                return configToSave;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration to {this.configPath}: {ex.Message}");
                return configToSave;
            }
        }

        private void EnsureConfigDirectoryExists()
        {
            var directory = Path.GetDirectoryName(this.configPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// Represents the actual configuration settings.
        /// </summary>
        public class Config
        {
            /// <summary>Gets or sets the default source directory path for backups.</summary>
            public string Source { get; set; } = string.Empty;

            /// <summary>Gets or sets the default destination directory path for backups.</summary>
            public string Destination { get; set; } = string.Empty;

            /// <summary>Gets or sets the application language (e.g., "En", "Fr").</summary>
            public string Language { get; set; } = "En";

            /// <summary>Gets or sets the log type (e.g., "json", "xml", "txt").</summary>
            public string LogType { get; set; } = "json";
        }
    }
}