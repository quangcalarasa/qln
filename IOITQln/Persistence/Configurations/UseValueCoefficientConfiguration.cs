using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class UseValueCoefficientConfiguration : AppEntityTypeIntConfiguration<UseValueCoefficient>
    {
        public override void Configure(EntityTypeBuilder<UseValueCoefficient> builder)
        {
            base.Configure(builder);
            builder.ToTable("UseValueCoefficient");

            builder.Property(c => c.Des).HasMaxLength(4000);
        }
    }
}
