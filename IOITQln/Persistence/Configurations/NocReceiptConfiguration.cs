using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class NocReceiptConfiguration :  AppEntityTypeGuidConfiguration<NocReceipt>
    {
        public override void Configure(EntityTypeBuilder<NocReceipt> builder)
        {
            base.Configure(builder);
            builder.ToTable("NocReceipt");

            builder.Property(c => c.Price).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Code).HasMaxLength(500);
            builder.Property(c => c.Executor).HasMaxLength(1000);
            builder.Property(c => c.NumberOfTransfer).HasMaxLength(1000);
            builder.Property(c => c.InvoiceCode).HasMaxLength(1000);
            builder.Property(c => c.Note).HasMaxLength(2000);
            builder.Property(c => c.Content).HasMaxLength(2000);
            builder.Property(c => c.Number).HasMaxLength(500);
        }
    }
}
