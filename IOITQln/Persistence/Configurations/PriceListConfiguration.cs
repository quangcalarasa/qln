using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class PriceListConfiguration : AppEntityTypeIntConfiguration<PriceList>
    {
        public override void Configure(EntityTypeBuilder<PriceList> builder)
        {
            base.Configure(builder);
            builder.ToTable("PriceList");

            //builder.Property(c => c.ParentId).IsRequired(false);
            builder.Property(c => c.Des).HasMaxLength(4000).IsRequired(false);
            //builder.Property(c => c.NameOfConstruction).HasMaxLength(2000).IsRequired();
            //builder.Property(c => c.IsMezzanine).IsRequired(false);
            //builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
            //builder.Property(c => c.UnitPriceId).IsRequired(false);
            builder.Property(c => c.DecreeType1Id).IsRequired(true);
            builder.Property(c => c.DecreeType2Id).IsRequired(true);
            //builder.Property(c => c.ValueTypePile1).IsRequired(true);
            //builder.Property(c => c.ValueTypePile2).IsRequired(true);
        }
    }
}
