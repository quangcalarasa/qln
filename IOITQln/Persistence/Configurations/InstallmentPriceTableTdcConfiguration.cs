using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class InstallmentPriceTableTdcConfiguration : AppEntityTypeIntConfiguration<InstallmentPriceTableTdc>
    {
        public override void Configure(EntityTypeBuilder<InstallmentPriceTableTdc> builder)
        {
            base.Configure(builder);
            builder.ToTable("InstallmentPriceTableTdc");

            builder.Property(c => c.RowStatus).IsRequired(false);
            builder.Property(c => c.PayTimeId).IsRequired(false);
            builder.Property(c => c.TypeRow).IsRequired(false);
            builder.Property(c => c.PaymentTimes).IsRequired(false);
            builder.Property(c => c.PayDateDefault).IsRequired(false);
            builder.Property(c => c.PayDateBefore).IsRequired(false);
            builder.Property(c => c.PayDateGuess).IsRequired(false);
            builder.Property(c => c.PayDateReal).IsRequired(false);
            builder.Property(c => c.MonthInterest).IsRequired(false);
            builder.Property(c => c.DailyInterest).IsRequired(false);
            builder.Property(c => c.MonthInterestRate).HasColumnType("decimal(18,5)").IsRequired(false);
            builder.Property(c => c.DailyInterestRate).HasColumnType("decimal(18,5)").IsRequired(false);
            builder.Property(c => c.TotalPay).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.PayAnnual).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.TotalInterest).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.TotalPayAnnual).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.Pay).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.Paid).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.PriceDifference).HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);

        }
    }
}
