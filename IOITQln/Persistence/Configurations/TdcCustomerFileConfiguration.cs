using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TdcCustomerFileConfiguration : AppEntityTypeIntConfiguration<TdcCustomerFile>
    {
        public override void Configure(EntityTypeBuilder<TdcCustomerFile> builder)
        {
            base.Configure(builder);
            builder.ToTable("TdcCustomerFile");
        }
    }
}
