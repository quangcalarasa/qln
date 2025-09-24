using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class RentBctTableConfigurations : AppEntityTypeGuidConfiguration<RentBctTable>
    {
        public override void Configure(EntityTypeBuilder<RentBctTable> builder)
        {
            base.Configure(builder);
            builder.ToTable("RentBctTable");

            builder.Property(c => c.PriceAfterDiscount).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PriceRent).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PriceRent1m2).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PriceVAT).HasColumnType("decimal(18,2)");
            builder.Property(c => c.StandardPrice).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Ktdbt).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Ktlcb).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PolicyReduction).HasColumnType("decimal(18,2)");
            builder.Property(c => c.TotalK).HasColumnType("decimal(18,2)");
            builder.Property(c => c.DiscountCoff).HasColumnType("decimal(18,2)");
            builder.Property(c => c.TotalPrice).HasColumnType("decimal(18,2)");
        }
    }
}
