// <copyright file="TranslationManager.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.Models
{
    /// <summary>
    /// Translation manager class for handling translations.
    /// </summary>
    public class TranslationManager : INotifyPropertyChanged
    {
        private static TranslationManager? instance;
        private Dictionary<string, string> translations = new();
        private string currentLanguage = "en";

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationManager"/> class.
        /// </summary>
        private TranslationManager()
        {
            this.LoadTranslations();
        }

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Event raised when the language changes.
        /// </summary>
        public event EventHandler? LanguageChanged;

        /// <summary>
        /// Gets the instance of the translation manager.
        /// </summary>
        public static TranslationManager Instance
        {
            get
            {
                instance ??= new TranslationManager();
                return instance;
            }
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
                    this.LoadTranslations();
                    this.OnPropertyChanged();
                    this.OnLanguageChanged();
                }
            }
        }

        /// <summary>
        /// Gets the translation for a given key.
        /// </summary>
        /// <param name="key">The key to get the translation for.</param>
        /// <returns>The translation for the given key.</returns>
        public string GetTranslation(string key)
        {
            return this.translations.TryGetValue(key, out var translation) ? translation : key;
        }

        /// <summary>
        /// Refreshes the translations.
        /// </summary>
        public void RefreshTranslations()
        {
            this.LoadTranslations();
            this.OnPropertyChanged();
        }

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the language changed event.
        /// </summary>
        protected virtual void OnLanguageChanged()
        {
            this.LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Loads the translations.
        /// </summary>
        private void LoadTranslations()
        {
            // TODO: Load translations from resource files or database
            // For now, using hardcoded translations
            this.translations = new Dictionary<string, string>
            {
                // Add your translations here
                { "backup.title", this.CurrentLanguage == "fr" ? "Sauvegarde" : "Backup" },
                { "backup.source", this.CurrentLanguage == "fr" ? "Source" : "Source" },
                { "backup.destination", this.CurrentLanguage == "fr" ? "Destination" : "Destination" },
                { "backup.save_project", this.CurrentLanguage == "fr" ? "Sauvegarder le projet" : "Save Project" },
                { "backup.differential_backup", this.CurrentLanguage == "fr" ? "Sauvegarde différentielle" : "Differential Backup" },
                { "logs.refresh", this.CurrentLanguage == "fr" ? "Actualiser" : "Refresh" },
            };
        }
    }
} 