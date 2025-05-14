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
            EnsureConfigDirectoryExists();
        }

        private void EnsureConfigDirectoryExists()
        {
            var directory = Path.GetDirectoryName(this.configPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>Loads the configuration from the config file.</summary>
        /// <returns>The loaded configuration object. Returns a new default Config if file not found or error occurs.</returns>
        public Config Load()
        {
            try
            {
                if (!File.Exists(this.configPath))
                {
                    Console.WriteLine($"Config file not found at {this.configPath}. Creating default config.");
                    var defaultConfig = new Config();
                    SaveOrOverride(defaultConfig);
                    return defaultConfig;
                }

                string jsonString = File.ReadAllText(this.configPath);
                Config? loadedConfig = JsonSerializer.Deserialize<Config>(jsonString, this.serializerOptions);
                
                if (loadedConfig != null)
                {
                    Console.WriteLine($"Configuration loaded successfully from {this.configPath}.");
                    Console.WriteLine($"  Source: {loadedConfig.Source}");
                    Console.WriteLine($"  Destination: {loadedConfig.Destination}");
                    Console.WriteLine($"  Language: {loadedConfig.Language}");
                    return loadedConfig;
                }

                Console.WriteLine($"Warning: Failed to deserialize config file at {this.configPath}, or file was empty. Returning default config.");
                return new Config();
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error deserializing configuration from {this.configPath}: {jsonEx.Message}. Returning default config.");
                return new Config();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration from {this.configPath}: {ex.Message}. Returning default config.");
                return new Config();
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
        }
    }
}