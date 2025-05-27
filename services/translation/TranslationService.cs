// <copyright file="TranslationService.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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

        /// <summary>
        /// Event raised when the language changes.
        /// </summary>
        public event EventHandler? LanguageChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationService"/> class.
        /// </summary>
        /// <param name="translationsPath">Path to the translations file.</param>
        public TranslationService(string translationsPath = "Resources/translations.json")
        {
            this.translationsPath = translationsPath;
            this.currentLanguage = "En"; // Default language with correct case

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
                Console.WriteLine("Warning: Attempted to set an empty or null language. Using default.");
                newLanguage = "En"; // Fallback to a default with correct case
            }

            // Capitalize first letter to match the JSON format
            newLanguage = char.ToUpper(newLanguage[0]) + newLanguage.Substring(1).ToLower();

            if (this.currentLanguage != newLanguage || this.translations == null)
            {
                Console.WriteLine($"Setting language to: {newLanguage}");
                this.CurrentLanguage = newLanguage;
                await this.LoadTranslationsAsync(); // Await the loading
                Console.WriteLine($"Translations presumably loaded for language: {this.currentLanguage}");
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
                    Console.WriteLine($"Error: Translations file not found at {this.translationsPath}");
                    this.translations = new Dictionary<string, Dictionary<string, string>>(); // Ensure translations is not null
                    throw new FileNotFoundException($"Translations file not found at {this.translationsPath}");
                }

                Console.WriteLine($"Translations file confirmed to exist at: {this.translationsPath}");
                Console.WriteLine($"Initiating read for language: {this.currentLanguage}");

                string jsonContent = await File.ReadAllTextAsync(this.translationsPath).ConfigureAwait(false);
                
                Console.WriteLine("File.ReadAllTextAsync completed.");

                // Console.WriteLine($"Translations file content: {jsonContent.Substring(0, Math.Min(jsonContent.Length, 500))}..."); // Log snippet
                var loadedTranslations = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent);

                if (loadedTranslations == null)
                {
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
                Console.WriteLine($"!!! Critical JSON deserialization error in LoadTranslationsAsync: {jsonEx.ToString()}");
                this.translations = new Dictionary<string, Dictionary<string, string>>(); // Ensure translations is not null
                throw new InvalidOperationException($"Failed to parse translations: {jsonEx.Message}", jsonEx);
            }
            catch (Exception ex)
            {
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
                return englishTranslation;
            }

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