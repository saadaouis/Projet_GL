// <copyright file="ViewModelBase.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasySave.Services.Translation;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave.ViewModels
{
    /// <summary>
    /// Base class for all view models.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
    /// </remarks>
    /// <param name="translationService">The translation service.</param>
    public abstract class ViewModelBase(TranslationService translationService) : INotifyPropertyChanged
    {
        private readonly TranslationService translationService = translationService;
        private readonly TranslationManager translationManager = App.ServiceProvider!.GetRequiredService<TranslationManager>();

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the translation manager for XAML bindings.
        /// </summary>
        public TranslationManager T => this.translationManager;

        /// <summary>
        /// Gets the translated text for a given key.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <returns>The translated text.</returns>
        protected string GetTranslation(string key)
        {
            return this.translationService.GetTranslation(key);
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets a property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="field">The field to set.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <returns>True if the value was changed, false otherwise.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
} 