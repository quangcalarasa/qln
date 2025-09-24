using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcPriceRentConfigurations : AppEntityTypeIntConfiguration<TdcPriceRent>
    {
        public override void Configure(EntityTypeBuilder<TdcPriceRent> builder)
        {
            base.Configure(builder);
            builder.ToTable("TdcPriceRent");

            builder.Property(c => c.Code).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.DecisionNumberTT).IsRequired(false);
            builder.Property(c => c.DecisionDateTT).IsRequired(false);
            builder.Property(c => c.PriceTC).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PriceMonth).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PriceToTal).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PriceTT).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.TotalPriceTT).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.TotalPriceCT).HasColumnType("decimal(18,2)");
            builder.Property(c => c.TotalAreaTT).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.TotalAreaCT).HasColumnType("decimal(18,2)");
        }

    }
}


