using System;
using DevPartner.Nop.Plugin.CloudStorage.File.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace DevPartner.Nop.Plugin.CloudStorage.File.Components
{
    [ViewComponent(Name = FileProviderPlugin.ComponentName)]
    public class FileSettingsViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;
        public FileSettingsViewComponent(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public IViewComponentResult Invoke(string type)
        {
            var model = new SettingsModel
            {
                DirectoryForFiles = _settingService.GetSettingByKey<string>(String.Format(FileProviderPlugin.DirectoryForFilesSettingsKey, type)),
                ProviderType  = type
            };
            return View("~/Plugins/DevPartner.CloudStorage.File/Views/FileSettingsComponent.cshtml", model);
        }
    }
}