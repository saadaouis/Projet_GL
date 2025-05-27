// <copyright file="TranslationManager.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.Services.Translation
{
    /// <summary>
    /// Translation manager class for handling translations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TranslationManager"/> class.
    /// </remarks>
    /// <param name="translationService">The translation service.</param>
    public class TranslationManager : INotifyPropertyChanged
    {
        private readonly TranslationService translationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationManager"/> class.
        /// </summary>
        /// <param name="translationService">The translation service.</param>
        public TranslationManager(TranslationService translationService)
        {
            this.translationService = translationService;
            this.translationService.LanguageChanged += this.OnLanguageChanged;
        }

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the translation for a given key.
        /// </summary>
        /// <param name="key">The key to get the translation for.</param>
        /// <returns>The translation for the given key.</returns>
        public string this[string key] => this.translationService.GetTranslation(key);

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            this.OnPropertyChanged(string.Empty); // Notify that all translations have changed
        }
    }
} 