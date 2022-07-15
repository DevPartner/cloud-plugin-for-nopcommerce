using System;
using System.Collections.Generic;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;

namespace DevPartner.Nop.Plugin.CloudStorage.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddOrUpdatePluginLocaleResource(this Dictionary<string, string> dictionary)
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            var languageService = EngineContext.Current.Resolve<ILanguageService>();

            foreach (var localizationResource in dictionary)
            {
                AddOrUpdatePluginLocaleResource(localizationService,
                languageService, localizationResource.Key, localizationResource.Value);
            }

        }
        /// <summary>
        /// Add a locale resource (if new) or update an existing one
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="resourceName">Resource name</param>
        /// <param name="resourceValue">Resource value</param>
        /// <param name="languageCulture">Language culture code. If null or empty, then a resource will be added for all languages</param>
        public static void AddOrUpdatePluginLocaleResource(
            ILocalizationService localizationService, ILanguageService languageService,
            string resourceName, string resourceValue, string languageCulture = null)
        {

            if (localizationService == null)
                throw new ArgumentNullException("localizationService");
            if (languageService == null)
                throw new ArgumentNullException("languageService");

            foreach (var lang in languageService.GetAllLanguages(true))
            {
                if (!String.IsNullOrEmpty(languageCulture) && !languageCulture.Equals(lang.LanguageCulture))
                    continue;

                var lsr = localizationService.GetLocaleStringResourceByName(resourceName, lang.Id, false);
                if (lsr == null)
                {
                    lsr = new LocaleStringResource
                    {
                        LanguageId = lang.Id,
                        ResourceName = resourceName,
                        ResourceValue = resourceValue
                    };
                    localizationService.InsertLocaleStringResource(lsr);
                }
                else
                {
                    lsr.ResourceValue = resourceValue;
                    localizationService.UpdateLocaleStringResource(lsr);
                }
            }
        }
    }
}
