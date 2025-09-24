using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TDCProjcetConfiguration : AppEntityTypeIntConfiguration<TDCProject>
    {
        public override void Configure(EntityTypeBuilder<TDCProject> builder)
        {
            base.Configure(builder);
            builder.ToTable("TDCProject");
             
            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.LandCount).IsRequired();
            builder.Property(c => c.HouseNumber).IsRequired();
            builder.Property(c => c.Lane).IsRequired(false);
            builder.Property(c => c.Ward).IsRequired(false);
            builder.Property(c => c.District).IsRequired();
            builder.Property(c => c.Province).IsRequired();
            builder.Property(c => c.TotalAreas).IsRequired();
            builder.Property(c => c.TotalApartment).IsRequired();
            builder.Property(c => c.TotalPlatform).IsRequired();
            builder.Property(c => c.TotalFloorAreas).IsRequired();
            builder.Property(c => c.TotalUseAreas).IsRequired();
            builder.Property(c => c.TotalBuildAreas).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000);
            builder.Property(c => c.LateRate).HasColumnType("decimal(18,2)");
            builder.Property(c => c.DebtRate).HasColumnType("decimal(18,2)");
        }
    }
}
