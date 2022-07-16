using DevPartner.Nop.Plugin.CloudStorage.Domain;
using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;

namespace DevPartner.Nop.Plugin.CloudStorage.Data
{
    [NopMigration("2020/05/29 09:10:17:6455422", "DevPartner.CloudStorage schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            Create.TableFor<MovingItem>();
        }
    }
}
