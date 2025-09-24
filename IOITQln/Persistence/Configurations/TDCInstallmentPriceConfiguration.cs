using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TDCInstallmentPriceConfiguration : AppEntityTypeIntConfiguration<TDCInstallmentPrice>
    {
        public override void Configure(EntityTypeBuilder<TDCInstallmentPrice> builder)
        {
            base.Configure(builder);
            builder.ToTable("TDCInstallmentPrice");

            builder.Property(c => c.ContractNumber).HasMaxLength(5000);
            builder.Property(c => c.Floor1).HasMaxLength(5000);
            builder.Property(c => c.NewContractValue).HasColumnType("decimal(18,2)");
            builder.Property(c => c.OldContractValue).HasColumnType("decimal(18,2)");
            builder.Property(c => c.DifferenceValue).HasColumnType("decimal(18,2)");
            builder.Property(c => c.TemporaryDecreeNumber).HasMaxLength(5000).IsRequired(false);
            builder.Property(c => c.TemporaryTotalArea).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.TemporaryTotalPrice).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.DecreeNumber).HasMaxLength(5000);
            builder.Property(c => c.TemporaryDecreeDate).IsRequired(false);
            builder.Property(c => c.TotalArea).HasColumnType("decimal(18,2)");
            builder.Property(c => c.TotalPrice).HasColumnType("decimal(18,2)");
            builder.Property(c => c.FirstPay).HasColumnType("decimal(18,2)");
            builder.Property(c => c.TotalPayValue).HasColumnType("decimal(18,2)");
            builder.Property(c => c.PesonalTax).HasColumnType("decimal(18,2)");
            builder.Property(c => c.RegistrationTax).HasColumnType("decimal(18,2)");

        }
    }
}



