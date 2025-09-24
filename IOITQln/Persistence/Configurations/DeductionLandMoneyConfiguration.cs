using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class DeductionLandMoneyConfiguration : AppEntityTypeIntConfiguration<DeductionLandMoney>
    {
        public override void Configure(EntityTypeBuilder<DeductionLandMoney> builder)
        {
            base.Configure(builder);
            builder.ToTable("DeductionLandMoney");

            builder.Property(c => c.Condition).HasMaxLength(4000);
            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
