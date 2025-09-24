using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class SalaryCoefficientConfiguration : AppEntityTypeIntConfiguration<SalaryCoefficient>
    {
        public override void Configure(EntityTypeBuilder<SalaryCoefficient> builder)
        {
            base.Configure(builder);
            builder.ToTable("SalaryCoefficient");

            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
