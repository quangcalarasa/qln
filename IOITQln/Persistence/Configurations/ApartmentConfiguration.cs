using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ApartmentConfiguration : AppEntityTypeIntConfiguration<Apartment>
    {
        public override void Configure(EntityTypeBuilder<Apartment> builder)
        {
            base.Configure(builder);
            builder.ToTable("Apartment");

            builder.Property(c => c.TypeReportApply).IsRequired();
            builder.Property(c => c.BlockId).IsRequired();
            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired();
            //builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            //builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Address).HasMaxLength(4000).IsRequired(false);
            //builder.Property(c => c.Lane).IsRequired();
            //builder.Property(c => c.Ward).IsRequired();
            //builder.Property(c => c.District).IsRequired();
            //builder.Property(c => c.Province).IsRequired();
            builder.Property(c => c.ConstructionAreaValue).IsRequired(false);
            builder.Property(c => c.ConstructionAreaValue1).IsRequired(false);
            builder.Property(c => c.ConstructionAreaValue2).IsRequired(false);
            builder.Property(c => c.ConstructionAreaValue3).IsRequired(false);
            builder.Property(c => c.UseAreaValue).IsRequired(false);
            builder.Property(c => c.UseAreaValue1).IsRequired(false);
            builder.Property(c => c.UseAreaValue2).IsRequired(false);
            builder.Property(c => c.LandscapeAreaValue).IsRequired(false);
            builder.Property(c => c.LandscapeAreaValue1).IsRequired(false);
            builder.Property(c => c.LandscapeAreaValue2).IsRequired(false);
            builder.Property(c => c.LandscapeAreaValue3).IsRequired(false);
            builder.Property(c => c.UsageStatusNote).HasMaxLength(4000);
            builder.Property(c => c.UseAreaNote1).HasMaxLength(4000);
            builder.Property(c => c.ConstructionAreaNote).HasMaxLength(4000);
            builder.Property(c => c.UseAreaNote).HasMaxLength(4000);
            builder.Property(c => c.No).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.CodeEstablishStateOwnership).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.NameBlueprint).HasMaxLength(1000).IsRequired(false);
        }
    }
}
