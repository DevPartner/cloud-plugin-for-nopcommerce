using DevPartner.Nop.Plugin.CloudStorage.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;

namespace DevPartner.Nop.Plugin.CloudStorage.Data
{
    public class MovingItemMap : NopEntityTypeConfiguration<MovingItem>
    {
        public override void Configure(EntityTypeBuilder<MovingItem> builder)
        {
            builder.ToTable("DP_CloudStorage_Queue");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.EntityId).IsRequired();
            builder.Property(m => m.OldProviderSystemName).IsRequired();
            builder.Property(m => m.Types).IsRequired().HasColumnName("TypeId");
            builder.Property(m => m.Status).IsRequired().HasColumnName("StatusId");

            base.Configure(builder);
        }
    }
}
