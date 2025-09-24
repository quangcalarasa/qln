using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ConstructionPriceConfiguration : AppEntityTypeIntConfiguration<ConstructionPrice>
    {
        public override void Configure(EntityTypeBuilder<ConstructionPrice> builder)
        {
            base.Configure(builder);
            builder.ToTable("ConstructionPrice");

            builder.Property(c => c.ParentId).IsRequired(false);
            builder.Property(c => c.DecreeType1Id).IsRequired(true);
            builder.Property(c => c.DecreeType2Id).IsRequired(true);
            builder.Property(c => c.Des).HasMaxLength(4000).IsRequired();
            builder.Property(c => c.NameOfConstruction).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
