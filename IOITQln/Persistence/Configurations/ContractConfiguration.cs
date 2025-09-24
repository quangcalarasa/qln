using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ContractConfiguration : AppEntityTypeIntConfiguration<Contract>
    {
        public override void Configure(EntityTypeBuilder<Contract> builder)
        {
            base.Configure(builder);
            builder.ToTable("Contract");

            builder.Property(c => c.Code).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.FullName).HasMaxLength(500).IsRequired();
            builder.Property(c => c.Phone).HasMaxLength(100).IsRequired(false);
            builder.Property(c => c.Email).HasMaxLength(200).IsRequired(false);
            builder.Property(c => c.Address).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Content).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.File).HasMaxLength(2000).IsRequired(false);
        }
    }
}
