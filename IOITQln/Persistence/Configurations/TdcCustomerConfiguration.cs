using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcCustomerConfiguration : AppEntityTypeIntConfiguration<TdcCustomer>
    {
        public override void Configure(EntityTypeBuilder<TdcCustomer> builder)
        {
            base.Configure(builder);
            builder.ToTable("TdcCustomer");

            builder.Property(c => c.Code).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.FullName).HasMaxLength(500).IsRequired();
            builder.Property(c => c.Phone).HasMaxLength(100).IsRequired(false);
            builder.Property(c => c.Email).HasMaxLength(200).IsRequired(false);
            builder.Property(c => c.AddressTT).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.AddressLH).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Note).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.CCCD).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.LaneTT).IsRequired();
            builder.Property(c => c.WardTT).IsRequired();
            builder.Property(c => c.DistrictTT).IsRequired();
            builder.Property(c => c.ProvinceTT).IsRequired();
            builder.Property(c => c.LaneLH).IsRequired();
            builder.Property(c => c.WardLH).IsRequired();
            builder.Property(c => c.DistrictLH).IsRequired();
            builder.Property(c => c.ProvinceLH).IsRequired();

        }

    }
}
