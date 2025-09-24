using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class InvestmentRateItemConfiguration : AppEntityTypeIntConfiguration<InvestmentRateItem>
    {
        public override void Configure(EntityTypeBuilder<InvestmentRateItem> builder)
        {
            base.Configure(builder);
            builder.ToTable("InvestmentRateItem");

            builder.Property(c => c.LineInfo).HasMaxLength(1000);
            builder.Property(c => c.DetailInfo).HasMaxLength(2000);
        }
    }
}
