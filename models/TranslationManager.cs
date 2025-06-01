using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.Models
{
    /// <summary>
    /// Singleton class responsible for managing translations for UI strings.
    /// It supports changing the current language dynamically and notifies UI components of changes.
    /// </summary>
    public class TranslationManager : INotifyPropertyChanged
    {
        // Singleton instance (lazy-loaded)
        private static TranslationManager? _instance;

        // Dictionary storing translation key-value pairs
        private Dictionary<string, string> _translations = new();

        // The currently selected language (default is English)
        private string _currentLanguage = "en";

        /// <summary>
        /// Gets the singleton instance of the TranslationManager.
        /// Ensures that only one instance is used throughout the application.
        /// </summary>
        public static TranslationManager Instance
        {
            get
            {
                _instance ??= new TranslationManager();
                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets the current language code (e.g., "en" or "fr").
        /// Changing the language will reload translations and notify observers.
        /// </summary>
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    LoadTranslations();       // Reload translation values
                    OnPropertyChanged();      // Notify data binding system (WPF, etc.)
                    OnLanguageChanged();      // Notify subscribers of the language change
                }
            }
        }

        // Event raised when a property value changes (used for data binding)
        public event PropertyChangedEventHandler? PropertyChanged;

        // Event triggered when the language is changed
        public event EventHandler? LanguageChanged;

        /// <summary>
        /// Private constructor to enforce singleton pattern.
        /// Loads default translations based on the initial language.
        /// </summary>
        private TranslationManager()
        {
            LoadTranslations();
        }

        /// <summary>
        /// Loads translations based on the current language setting.
        /// This method is where translations are defined. Ideally, this would load from
        /// resource files, databases, or external sources. Currently hardcoded for demo.
        /// </summary>
        private void LoadTranslations()
        {
            _translations = new Dictionary<string, string>
            {
                // Translation keys and their corresponding values based on the language
                { "backup.title", CurrentLanguage == "fr" ? "Sauvegarde" : "Backup" },
                { "backup.source", CurrentLanguage == "fr" ? "Source" : "Source" },
                { "backup.destination", CurrentLanguage == "fr" ? "Destination" : "Destination" },
                { "backup.save_project", CurrentLanguage == "fr" ? "Sauvegarder le projet" : "Save Project" },
                { "backup.differential_backup", CurrentLanguage == "fr" ? "Sauvegarde différentielle" : "Differential Backup" },
                { "logs.refresh", CurrentLanguage == "fr" ? "Actualiser" : "Refresh" }
            };
        }

        /// <summary>
        /// Retrieves the translated string for the given key.
        /// If the key doesn't exist, it returns the key itself as a fallback.
        /// </summary>
        /// <param name="key">The translation key (e.g., "backup.title")</param>
        /// <returns>The translated string or the key if not found</returns>
        public string GetTranslation(string key)
        {
            return _translations.TryGetValue(key, out var translation) ? translation : key;
        }

        /// <summary>
        /// Manually reloads the translation dictionary and notifies data bindings.
        /// Can be used after dynamic updates or external language changes.
        /// </summary>
        public void RefreshTranslations()
        {
            LoadTranslations();
            OnPropertyChanged();
        }

        /// <summary>
        /// Notifies UI components that a property has changed (used by WPF or similar frameworks).
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the LanguageChanged event to inform subscribers that the language has been updated.
        /// </summary>
        protected virtual void OnLanguageChanged()
        {
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
