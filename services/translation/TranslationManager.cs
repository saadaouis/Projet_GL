// <copyright file="TranslationManager.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

namespace EasySave.Services.Translation
{
    /// <summary>
    /// Translation manager class for handling translations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TranslationManager"/> class.
    /// </remarks>
    /// <param name="translationService">The translation service.</param>
    public class TranslationManager(TranslationService translationService)
    {
        private readonly TranslationService translationService = translationService;

        /// <summary>
        /// Gets the translation for a given key.
        /// </summary>
        /// <param name="key">The key to get the translation for.</param>
        /// <returns>The translation for the given key.</returns>
        public string this[string key] => this.translationService.GetTranslation(key);
    }
} 