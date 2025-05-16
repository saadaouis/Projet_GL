using System;
using System.Windows.Input;

namespace EasySave.Services.Translation
{
    public class TranslationManager
    {
        private readonly TranslationService translationService;

        public TranslationManager(TranslationService translationService)
        {
            this.translationService = translationService;
        }

        public string this[string key] => this.translationService.GetTranslation(key);
    }
} 