using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167DebtConfiguration : AppEntityTypeLongConfiguration<Md167Debt>
    {
        public override void Configure(EntityTypeBuilder<Md167Debt> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167Debt");

            builder.Property(c => c.HouseCode).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.Title).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Note).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.AmountPaidPerMonth).HasColumnType("decimal(18,2)");
            builder.Property(c => c.AmountInterest).HasColumnType("decimal(18,2)");
            builder.Property(c => c.AmountPaidInPeriod).HasColumnType("decimal(18,2)");
            builder.Property(c => c.AmountToBePaid).HasColumnType("decimal(18,2)");
            builder.Property(c => c.AmountPaid).HasColumnType("decimal(18,2)");
            builder.Property(c => c.AmountDiff).HasColumnType("decimal(18,2)");
        }
    }
}
