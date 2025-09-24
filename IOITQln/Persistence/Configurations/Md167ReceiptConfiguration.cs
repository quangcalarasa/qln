using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167ReceiptConfiguration : AppEntityTypeLongConfiguration<Md167Receipt>
    {
        public override void Configure(EntityTypeBuilder<Md167Receipt> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167Receipt");

            builder.Property(c => c.ReceiptCode).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.Note).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Note).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Amount).HasColumnType("decimal(18,2)");
        }
    }
}
