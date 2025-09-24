using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcMemberCustomerConfigurations : AppEntityTypeIntConfiguration<TdcMemberCustomer>
    {
        public override void Configure(EntityTypeBuilder<TdcMemberCustomer> builder)
        {
            base.Configure(builder);
            builder.ToTable("TdcMemberCustomer");
        }
    }
}
