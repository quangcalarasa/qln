using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class CustomerConfiguration : AppEntityTypeIntConfiguration<Customer>
    {
        public override void Configure(EntityTypeBuilder<Customer> builder)
        {
            base.Configure(builder);
            builder.ToTable("Customer");

            builder.Property(c => c.Code).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.FullName).HasMaxLength(500).IsRequired();
            builder.Property(c => c.Phone).HasMaxLength(100).IsRequired(false);
            builder.Property(c => c.Email).HasMaxLength(200).IsRequired(false);
            builder.Property(c => c.Address).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Avatar).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.PlaceCode).HasMaxLength(2000).IsRequired(false);
        }
    }
}
