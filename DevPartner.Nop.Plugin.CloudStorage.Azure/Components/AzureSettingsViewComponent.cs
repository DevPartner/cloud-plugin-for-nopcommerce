using DevPartner.Nop.Plugin.CloudStorage.Azure.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System;
using System.Threading.Tasks;

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

        public async Task<IViewComponentResult> InvokeAsync(string type)
        {
            var model = new SettingsModel
            {
                Container = await _settingService.GetSettingByKeyAsync<string>(String.Format(AzureBlobProviderPlugin.ContainerSettingsKey, type)),
                EndPoint = await _settingService.GetSettingByKeyAsync<string>(String.Format(AzureBlobProviderPlugin.EndPointSettingsKey, type)),
                ProviderType  = type
            };
            return View("~/Plugins/DevPartner.CloudStorage.Azure/Views/AzureSettingsComponent.cshtml", model);
        }
    }
}