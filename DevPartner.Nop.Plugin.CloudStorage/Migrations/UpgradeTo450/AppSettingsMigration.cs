using DevPartner.Nop.Plugin.CloudStorage.Configuration;
using FluentMigrator;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Data.Migrations;
using System.Collections.Generic;

namespace DevPartner.Nop.Plugin.CloudStorage.Migrations.UpgradeTo450
{
    [NopMigration("2022-07-21 14:03:00", "Pseudo-migration to update appSettings.json file", MigrationProcessType.Update)]
    public class AppSettingsMigration : MigrationBase
    {
        public override void Up()
        {
            var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();

            var config = new CloudConfig
            {
                FileProviderRuleConfig = new[] {
                    new FileProviderRuleConfig
                    {
                        Name = "Use Cloud For Images",
                        Pattern = "(.*?)(\\\\images)(\\\\|$)(.*)",
                        Replace = "$3$4"
                    } 
                }
            };

            AppSettingsHelper.SaveAppSettings(new List<IConfig> { config }, fileProvider);
        }

        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}
