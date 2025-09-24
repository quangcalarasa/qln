using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class SalaryConfiguration : AppEntityTypeIntConfiguration<Salary>
    {
        public override void Configure(EntityTypeBuilder<Salary> builder)
        {
            base.Configure(builder);
            builder.ToTable("Salary");
            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
