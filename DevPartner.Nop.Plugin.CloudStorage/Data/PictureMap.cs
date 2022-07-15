using DevPartner.Nop.Plugin.CloudStorage.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Media;
using Nop.Data.Mapping;

namespace DevPartner.Nop.Plugin.CloudStorage.Data
{
    public class PictureMap : NopEntityTypeConfiguration<PictureExt>
    {
        public override void Configure(EntityTypeBuilder<PictureExt> builder)
        {
            builder.ToTable("Picture");
            //builder.HasKey(picture => picture.Id);

            builder.Property(picture => picture.MimeType).HasMaxLength(40).IsRequired();
            builder.Property(picture => picture.SeoFilename).HasMaxLength(300);
            builder.Property(p =>p.ExternalUrl);
            base.Configure(builder);
        }
    }
}
