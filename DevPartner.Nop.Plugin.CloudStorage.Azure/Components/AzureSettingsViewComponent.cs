using System;
using DevPartner.Nop.Plugin.CloudStorage.Azure.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace DevPartner.Nop.Plugin.CloudStorage.Azure.Components
{
    [ViewComponent(Name = AzureBlobProviderPlugin.ComponentName)]
    public class AzureSettingsViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;
        public AzureSettingsViewComponent(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public IViewComponentResult Invoke(string type)
        {
            var model = new SettingsModel
            {
                Container = _settingService.GetSettingByKey<string>(String.Format(AzureBlobProviderPlugin.ContainerSettingsKey, type)),
                EndPoint = _settingService.GetSettingByKey<string>(String.Format(AzureBlobProviderPlugin.EndPointSettingsKey, type)),
                ProviderType  = type
            };
            return View("~/Plugins/DevPartner.CloudStorage.Azure/Views/AzureSettingsComponent.cshtml", model);
        }
    }
}