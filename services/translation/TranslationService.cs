using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasySave.Services.Translation
{
    /// <summary>
    /// Service responsible for handling application translations.
    /// </summary>
    public class TranslationService
    {
        private Dictionary<string, Dictionary<string, string>> _translations;
        private string _currentLanguage;
        private readonly string _translationsPath;

        /// <summary>
        /// Gets or sets the current language.
        /// </summary>
        public string CurrentLanguage
        {
            get => this._currentLanguage;
            set
            {
                if (this._currentLanguage != value)
                {
                    this._currentLanguage = value;
                    this.LoadTranslationsAsync().Wait();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationService"/> class.
        /// </summary>
        /// <param name="translationsPath">Path to the translations file.</param>
        public TranslationService(string translationsPath = "resources/translations.json")
        {
            this._translationsPath = translationsPath;
            this._translations = new Dictionary<string, Dictionary<string, string>>();
            this._currentLanguage = "en"; // Default language
        }

        /// <summary>
        /// Loads translations from the JSON file.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadTranslationsAsync()
        {
            try
            {
                if (!File.Exists(this._translationsPath))
                {
                    throw new FileNotFoundException($"Translations file not found at {this._translationsPath}");
                }

                string jsonContent = await File.ReadAllTextAsync(this._translationsPath);
                this._translations = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent)
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
            if (this._translations.TryGetValue(this._currentLanguage, out var languageTranslations) &&
                languageTranslations.TryGetValue(key, out var translation))
            {
                return translation;
            }

            // Fallback to English if translation not found
            if (this._currentLanguage != "en" &&
                this._translations.TryGetValue("en", out var englishTranslations) &&
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
            return this._translations.Keys.ToArray();
        }
    }
} 