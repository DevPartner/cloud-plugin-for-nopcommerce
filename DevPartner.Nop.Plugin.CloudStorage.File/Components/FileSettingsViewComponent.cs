using DevPartner.Nop.Plugin.CloudStorage.File.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.File.Components
{
    [ViewComponent(Name = FileProviderPlugin.COMPONENT_NAME)]
    public class FileSettingsViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;
        public FileSettingsViewComponent(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string type)
        {
            var model = new SettingsModel
            {
                DirectoryForFiles = await _settingService.GetSettingByKeyAsync<string>(String.Format(FileProviderPlugin.DIRECTORY_FOR_FILES_SETTINGS_KEY, type)),
                ProviderType  = type
            };
            return View("~/Plugins/DevPartner.CloudStorage.File/Views/FileSettingsComponent.cshtml", model);
        }
    }
}