using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class InvestmentRateConfiguration : AppEntityTypeIntConfiguration<InvestmentRate>
    {
        public override void Configure(EntityTypeBuilder<InvestmentRate> builder)
        {
            base.Configure(builder);
            builder.ToTable("InvestmentRate");

            //builder.Property(c => c.Code).HasMaxLength(1000);
            //builder.Property(c => c.Name).HasMaxLength(2000);
            builder.Property(c => c.Des).HasMaxLength(4000);
        }
    }
}
