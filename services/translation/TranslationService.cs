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
        private readonly string translationsPath;
        private Dictionary<string, Dictionary<string, string>> translations;
        private string currentLanguage;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationService"/> class.
        /// </summary>
        /// <param name="translationsPath">Path to the translations file.</param>
        public TranslationService(string translationsPath = "resources/translations.json")
        {
            this.translationsPath = translationsPath;
            this.translations = new Dictionary<string, Dictionary<string, string>>();
            this.currentLanguage = "en"; // Default language
        }

        /// <summary>
        /// Gets or sets the current language.
        /// </summary>
        public string CurrentLanguage
        {
            get => this.currentLanguage;
            set
            {
                if (this.currentLanguage != value)
                {
                    this.currentLanguage = value;
                    this.LoadTranslationsAsync().Wait();
                }
            }
        }

        /// <summary>
        /// Loads translations from the JSON file.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadTranslationsAsync()
        {
            try
            {
                if (!File.Exists(this.translationsPath))
                {
                    throw new FileNotFoundException($"Translations file not found at {this.translationsPath}");
                }

                string jsonContent = await File.ReadAllTextAsync(this.translationsPath);
                this.translations = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent)
                    ?? throw new InvalidOperationException("Failed to deserialize translations file");
            }
            catch (Exception ex)
            {
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
            return this.translations.Keys.ToArray();
        }
    }
} 