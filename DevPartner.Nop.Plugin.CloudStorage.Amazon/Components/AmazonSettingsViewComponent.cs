using DevPartner.Nop.Plugin.CloudStorage.Amazon.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using System;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Amazon.Components
{
    [ViewComponent(Name = AmazonCloudStoragePlugin.ComponentName)]
    public class AmazonSettingsViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;
        public AmazonSettingsViewComponent(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string type)
        {
            var model = new SettingsModel
            {
                BucketName = await _settingService.GetSettingByKeyAsync<string>(String.Format(AmazonCloudStoragePlugin.BucketNameSettingsKey, type)),
                ProviderType  = type
            };
            return View("~/Plugins/DevPartner.CloudStorage.Amazon/Views/AmazonSettingsComponent.cshtml", model);
        }
    }
}