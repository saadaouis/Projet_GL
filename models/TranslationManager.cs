using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.Models
{
    public class TranslationManager : INotifyPropertyChanged
    {
        private static TranslationManager? _instance;
        private Dictionary<string, string> _translations = new();
        private string _currentLanguage = "en";

        public static TranslationManager Instance
        {
            get
            {
                _instance ??= new TranslationManager();
                return _instance;
            }
        }

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    LoadTranslations();
                    OnPropertyChanged();
                    OnLanguageChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? LanguageChanged;

        private TranslationManager()
        {
            LoadTranslations();
        }

        private void LoadTranslations()
        {
            // TODO: Load translations from resource files or database
            // For now, using hardcoded translations
            _translations = new Dictionary<string, string>
            {
                // Add your translations here
                { "backup.title", CurrentLanguage == "fr" ? "Sauvegarde" : "Backup" },
                { "backup.source", CurrentLanguage == "fr" ? "Source" : "Source" },
                { "backup.destination", CurrentLanguage == "fr" ? "Destination" : "Destination" },
                { "backup.save_project", CurrentLanguage == "fr" ? "Sauvegarder le projet" : "Save Project" },
                { "backup.differential_backup", CurrentLanguage == "fr" ? "Sauvegarde différentielle" : "Differential Backup" },
                { "logs.refresh", CurrentLanguage == "fr" ? "Actualiser" : "Refresh" }
            };
        }

        public string GetTranslation(string key)
        {
            return _translations.TryGetValue(key, out var translation) ? translation : key;
        }

        public void RefreshTranslations()
        {
            LoadTranslations();
            OnPropertyChanged();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnLanguageChanged()
        {
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
} 