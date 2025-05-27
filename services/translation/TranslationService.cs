// <copyright file="TranslationService.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave.Services.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave.Services.Translation
{
    /// <summary>
    /// Service responsible for handling application translations.
    /// </summary>
    public class TranslationService
    {
        private static readonly string[] AvailableLanguages = ["En", "Fr", "Gw"];
        private readonly string translationsPath;
        private Dictionary<string, Dictionary<string, string>>? translations; // Can be null initially
        private string currentLanguage;
        private readonly LoggingService logger;

        /// <summary>
        /// Event raised when the language changes.
        /// </summary>
        public event EventHandler? LanguageChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationService"/> class.
        /// </summary>
        /// <param name="translationsPath">Path to the translations file.</param>
        /// <param name="logger">The logger instance.</param>
        public TranslationService(string translationsPath = "Resources/translations.json", LoggingService? logger = null)
        {
            this.translationsPath = translationsPath;
            this.currentLanguage = "En"; // Default language with correct case
            this.logger = logger ?? App.ServiceProvider!.GetRequiredService<LoggingService>();

            // Translations are not loaded in constructor; to be loaded explicitly via LoadTranslationsAsync or SetLanguageAsync
        }

        /// <summary>
        /// Gets the current language.
        /// </summary>
        public string CurrentLanguage
        {
            get => this.currentLanguage;
            
            // Setter is now simple, language change and loading is handled by SetLanguageAsync
            private set => this.currentLanguage = value;
        }

        /// <summary>
        /// Sets the current language and loads the corresponding translations.
        /// </summary>
        /// <param name="newLanguage">The new language code (e.g., "En", "Fr").</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SetLanguageAsync(string newLanguage)
        {
            if (string.IsNullOrEmpty(newLanguage)) 
            {
                var errorLog = new Dictionary<string, string>
                {
                    { "operation", "SetLanguage" },
                    { "error", "Attempted to set an empty or null language" },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                };
                this.logger.Log(errorLog);
                Console.WriteLine("Warning: Attempted to set an empty or null language. Using default.");
                newLanguage = "En"; // Fallback to a default with correct case
            }

            // Capitalize first letter to match the JSON format
            newLanguage = char.ToUpper(newLanguage[0]) + newLanguage.Substring(1).ToLower();

            if (this.currentLanguage != newLanguage || this.translations == null)
            {
                Console.WriteLine($"Setting language to: {newLanguage}");
                this.CurrentLanguage = newLanguage;
                try
                {
                    await this.LoadTranslationsAsync(); // Await the loading
                    Console.WriteLine($"Translations loaded successfully for language: {this.currentLanguage}");
                }
                catch (Exception ex)
                {
                    var errorLog = new Dictionary<string, string>
                    {
                        { "operation", "SetLanguage" },
                        { "error", $"Failed to load translations: {ex.Message}" },
                        { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                    };
                    this.logger.Log(errorLog);
                    Console.WriteLine($"Error loading translations: {ex.Message}");
                    throw;
                }
                this.OnLanguageChanged();
            }
            else
            {
                Console.WriteLine($"Language {newLanguage} is already set and translations were loaded.");
            }
        }

        /// <summary>
        /// Loads translations from the JSON file for the current language.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadTranslationsAsync()
        {
            Console.WriteLine($"Attempting to load translations for: {this.currentLanguage} from {this.translationsPath}");
            try
            {
                if (!File.Exists(this.translationsPath))
                {
                    var errorLog = new Dictionary<string, string>
                    {
                        { "operation", "LoadTranslations" },
                        { "error", $"Translations file not found at {this.translationsPath}" },
                        { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                    };
                    this.logger.Log(errorLog);
                    Console.WriteLine($"Error: Translations file not found at {this.translationsPath}");
                    this.translations = new Dictionary<string, Dictionary<string, string>>(); // Ensure translations is not null
                    throw new FileNotFoundException($"Translations file not found at {this.translationsPath}");
                }

                Console.WriteLine($"Translations file confirmed to exist at: {this.translationsPath}");
                Console.WriteLine($"Initiating read for language: {this.currentLanguage}");

                string jsonContent = await File.ReadAllTextAsync(this.translationsPath).ConfigureAwait(false);
                
                Console.WriteLine("File.ReadAllTextAsync completed.");

                var loadedTranslations = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent);

                if (loadedTranslations == null)
                {
                    var errorLog = new Dictionary<string, string>
                    {
                        { "operation", "LoadTranslations" },
                        { "error", "Failed to deserialize translations file, result is null" },
                        { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                    };
                    this.logger.Log(errorLog);
                    Console.WriteLine("Error: Failed to deserialize translations file, result is null.");
                    this.translations = new Dictionary<string, Dictionary<string, string>>(); // Ensure translations is not null
                    throw new InvalidOperationException("Failed to deserialize translations file, result is null.");
                }

                this.translations = loadedTranslations;
                Console.WriteLine($"Translations deserialized. Available languages in file: {string.Join(", ", this.translations.Keys)}");
                Console.WriteLine($"Translations loaded successfully for requested language: {this.currentLanguage}");
            }
            catch (JsonException jsonEx)
            {
                var errorLog = new Dictionary<string, string>
                {
                    { "operation", "LoadTranslations" },
                    { "error", $"JSON deserialization error: {jsonEx.Message}" },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                };
                this.logger.Log(errorLog);
                Console.WriteLine($"!!! Critical JSON deserialization error in LoadTranslationsAsync: {jsonEx.ToString()}");
                this.translations = new Dictionary<string, Dictionary<string, string>>(); // Ensure translations is not null
                throw new InvalidOperationException($"Failed to parse translations: {jsonEx.Message}", jsonEx);
            }
            catch (Exception ex)
            {
                var errorLog = new Dictionary<string, string>
                {
                    { "operation", "LoadTranslations" },
                    { "error", $"Failed to load translations: {ex.Message}" },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                };
                this.logger.Log(errorLog);
                Console.WriteLine($"!!! Critical error in LoadTranslationsAsync: {ex.ToString()}");
                this.translations = new Dictionary<string, Dictionary<string, string>>(); // Ensure translations is not null
                throw new InvalidOperationException($"Failed to load translations: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the translation for a given key in the current language.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <returns>The translated string.</returns>
        public string GetTranslation(string key)
        {
            if (this.translations == null)
            {
                var translationErrorLog = new Dictionary<string, string>
                {
                    { "operation", "GetTranslation" },
                    { "error", "Translations not loaded" },
                    { "key", key },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                };
                this.logger.Log(translationErrorLog);
                Console.WriteLine("Warning: Translations not loaded, attempting to get translation for key: " + key);
                return key; // Or throw, or attempt a load
            }

            if (this.translations.TryGetValue(this.currentLanguage, out var languageTranslations) &&
                languageTranslations.TryGetValue(key, out var translation))
            {
                return translation;
            }

            // Fallback to English if translation not found
            if (this.currentLanguage != "en" &&
                this.translations.TryGetValue("en", out var englishTranslations) &&
                englishTranslations.TryGetValue(key, out var englishTranslation))
            {
                var warningLog = new Dictionary<string, string>
                {
                    { "operation", "GetTranslation" },
                    { "warning", $"Translation not found for key '{key}' in language '{this.currentLanguage}', using English fallback" },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                };
                this.logger.Log(warningLog);
                Console.WriteLine($"Warning: Translation not found for key '{key}' in language '{this.currentLanguage}', using English fallback");
                return englishTranslation;
            }

            var missingTranslationLog = new Dictionary<string, string>
            {
                { "operation", "GetTranslation" },
                { "error", $"Translation not found for key '{key}' in any language" },
                { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
            };
            this.logger.Log(missingTranslationLog);
            Console.WriteLine($"Error: Translation not found for key '{key}' in any language");
            return key; // Return the key if no translation is found
        }

        /// <summary>
        /// Gets the list of available languages.
        /// </summary>
        /// <returns>An array of available language codes.</returns>
        public string[] GetAvailableLanguages()
        {
            return AvailableLanguages;
        }

        /// <summary>
        /// Raises the LanguageChanged event.
        /// </summary>
        protected virtual void OnLanguageChanged()
        {
            this.LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
} 