using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167ContractConfiguration : AppEntityTypeIntConfiguration<Md167Contract>
    {
        public override void Configure(EntityTypeBuilder<Md167Contract> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167Contract");

            builder.Property(c => c.Code).HasMaxLength(500).IsRequired(true);
            builder.Property(c => c.NoteRentalPeriod).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.NoteRentalPurpose).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.ContractExtension).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Liquidation).HasMaxLength(2000).IsRequired(false);
        }
    }
}
