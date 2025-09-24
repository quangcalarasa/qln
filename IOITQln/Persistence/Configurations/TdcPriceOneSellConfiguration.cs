using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcPriceOneSellConfiguration: AppEntityTypeIntConfiguration<TdcPriceOneSell>
    {
        public override void Configure(EntityTypeBuilder<TdcPriceOneSell> builder)
        {
            base.Configure(builder);
            builder.ToTable("TdcPriceOneSell");

            builder.Property(c => c.Code).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.PersonalTax).HasColumnType("decimal(18,2)");
            builder.Property(c => c.RegistrationTax).HasColumnType("decimal(18,2)");
            builder.Property(c => c.TotalPriceTT).HasColumnType("decimal(18,2)");
            builder.Property(c => c.TotalPriceCT).HasColumnType("decimal(18,2)");
            builder.Property(c => c.TotalAreaTT).HasColumnType("decimal(18,2)");
            builder.Property(c => c.TotalAreaCT).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PaymentCenter).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PaymentPublic).HasColumnType("decimal(18,2)");
            
        }
    }
}
