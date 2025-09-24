using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcACustomerDetailConfiguration : AppEntityTypeIntConfiguration<TdcAuthCustomerDetail>
    {
        public override void Configure(EntityTypeBuilder<TdcAuthCustomerDetail> builder)
        {
            base.Configure(builder);
            builder.ToTable("TdcAuthCustomerDetail");
        }
    }
}
